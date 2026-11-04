using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

/// <summary>
/// Contains utils for networks shared between the client and the host projects. Network utilities shared
/// between host and client. Centralizes the resolution of the device's local IP and the framing (JSON
/// messages with a 4-byte length prefix) used in the TCP signaling, avoiding maintaining the same 
/// duplicate logic across multiple scripts from both projects.
/// </summary>
public static class NetworkUtils
{

    #region IPAddress

    /// <summary>
    /// IP Addres of this machine.
    /// </summary>
    private static string ipAddress;

    /// <summary>
    /// Synchronizes IP resolution to prevent two threads from calculating it at the same time
    /// (e.g. if two components call GetIP() in the same frame from different threads).
    /// </summary>
    private static readonly object ipResolutionLock = new object();

    /// <summary>
    /// Returns the IP Address of this device.
    /// </summary>
    /// <returns>IP Address of this device.</returns>
    public static string GetIP()
    {
        if (ipAddress != null) return ipAddress;

        lock (ipResolutionLock)
        {
            // Doble comprobación: otro hilo pudo resolverla mientras esperábamos el lock.
            if (ipAddress == null)
                ipAddress = ResolveLocalIPAddress();
        }

        return ipAddress;
    }

    /// <summary>
    /// Resets the IPAddres in case we want to assign a new one.
    /// </summary>
    public static void ResetIP()
    {
        lock (ipResolutionLock)
        {
            ipAddress = null;
        }
    }

    /// <summary>
    /// Gets and assigns the IP Address of this device. The method used depends on the platform
    /// the code is running of. This method should be called ONLY ONCE.
    /// </summary>
    private static string ResolveLocalIPAddress()
    {
        string ipAddress = "No disponible";
        try
        {
         #if UNITY_EDITOR || UNITY_STANDALONE_WIN
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) continue;
                if (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel) continue;

                // Excluir adaptadores virtuales (VirtualBox, VMware, Hyper-V, etc.)
                string name = ni.Name.ToLower();
                string desc = ni.Description.ToLower();
                if (name.Contains("virtual") || desc.Contains("virtual") ||
                    name.Contains("vmware") || desc.Contains("vmware") ||
                    name.Contains("vbox") || desc.Contains("vbox")) continue;

                IPInterfaceProperties props = ni.GetIPProperties();
                if (props.GatewayAddresses.Count == 0) continue;

                foreach (UnicastIPAddressInformation addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork) continue;
                    ipAddress = addr.Address.ToString();
                    Debug.Log($"[Network] Adaptador: {ni.Name} - IP: {ipAddress}");
                    return ipAddress;
                }
            }
         #elif UNITY_ANDROID
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("239.0.0.1", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                ipAddress = endPoint.Address.ToString();
            }
            Debug.Log($"[Network] IP seleccionada: {ipAddress}");
        #endif
        }
        catch (Exception e)
        {
            Debug.LogError($"[Network] Error obteniendo IP: {e}");
        }

        return ipAddress;
    }

    #endregion

    #region ReadingMessages
    /// <summary>
    /// It tries to read exactly count bytes from the stream to the buffer.
    /// If the connection closes cleanly before reading any bytes, returns false. If it closes
    /// mid-reading, it launches IOException because at that point it has already committed to a
    /// complete message.
    /// </summary>
    /// <param name="stream">TCP Stream to read from.</param>
    /// <param name="buffer">Destination buffer where the read bytes will be stored.</param>
    /// <param name="count">Number of bytes to read.</param>
    /// <returns>True if all bytes where read, false if there was a clean closure before reading.</returns>
    private static bool TryReadExact(NetworkStream stream, byte[] buffer, int count)
    {
        int total = 0;
        while (total < count)
        {
            int read = stream.Read(buffer, total, count - total);
            if (read == 0)
            {
                if (total == 0) return false; // cierre limpio, no hay más mensajes
                throw new IOException("Conexión cerrada a mitad de un mensaje.");
            }
            total += read;
        }
        return true;
    }

    /// <summary>
    /// It reads exactly count bytes from the stream to the buffer. Unlike TryReadExact, it does not
    /// support a clean closure as a valid case: it is used when the size of the message is already known
    /// (after reading the header), so any close while reading is always a connection error in the middle
    /// of the message.
    /// </summary>
    /// <param name="stream">TCP Stream to read from.</param>
    /// <param name="buffer">Destination buffer where the read bytes will be stored.</param>
    /// <param name="count">Number of bytes to read.</param>
    private static void ReadExact(NetworkStream stream, byte[] buffer, int count)
    {
        if (!TryReadExact(stream, buffer, count))
            throw new IOException("Conexión cerrada durante la lectura.");
    }

    /// <summary>
    /// It reads a full message with the project's framing protocol: 4 bytes of header followed by the body
    /// in UTF-8. Encapsulates the "read header + read body" pattern used on both the server and client.
    /// </summary>
    /// <param name="stream">TCP Stream to read from.</param>
    /// <param name="json">Message decoded as string, or null if it could not be read.</param>
    /// <returns></returns>
    public static bool TryReadFramedMessage(NetworkStream stream, out string json)
    {
        json = null;

        byte[] header = new byte[4];
        if (!TryReadExact(stream, header, 4)) return false;

        int size = BitConverter.ToInt32(header, 0);
        byte[] body = new byte[size];
        ReadExact(stream, body, size);

        json = Encoding.UTF8.GetString(body);
        return true;
    }

    /// <summary>
    /// It writes a full message with the project's framing protocol: 4 bytes of header followed by the body
    /// in UTF-8.
    /// </summary>
    /// <param name="stream">TCP Stream to read from.</param>
    /// <param name="json">Message content (typically a serialized SignalingMessage).</param>
    /// <param name="syncRoot">An optional object to synchronize the write to (lock), required when multiple
    /// threads can write to the same stream concurrently, such as in SignalingServer when sending to 
    /// different clients. If it is null no lock is applied.</param>
    public static void WriteFramedMessage(NetworkStream stream, string json, object syncRoot = null)
    {
        byte[] data = Encoding.UTF8.GetBytes(json);
        byte[] header = BitConverter.GetBytes(data.Length);

        if (syncRoot != null)
        {
            lock (syncRoot)
            {
                stream.Write(header, 0, 4);
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
        }
        else
        {
            stream.Write(header, 0, 4);
            stream.Write(data, 0, data.Length);
            stream.Flush();
        }
    }

    #endregion

}
