using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Unity.WebRTC;
using UnityEngine;

/// <summary>
/// HOST CONNECTION STRUCTURES
/// </summary>

#region Enums

/// <summary>
/// Indicates which phase the message we are sending belongs too
/// </summary>
public enum ConnectionEvent
{
    MULTICAST,
    HANDSHAKE,
    DISCONNECT,
    SDP, 
    ICE
}

/// <summary>
/// Indicates the type of connection stablished with a client
/// </summary>
public enum ClientConnectionType
{
    NONE,
    WEB_SOCKET,
    TCP,
    ADB
}

#endregion

#region Connection Structures

/// <summary>
/// Class that represents each device connected via USB cable
/// </summary>
public class WiredDeviceData
{
    public string deviceId;
    public int localPort;
    public TcpListener listener;
    public Thread acceptThread;
    public TcpClient tcpClient;
    public string clientID;
}

/// <summary>
/// Class that encapsulates information needed when establishing connection between devices
/// </summary>
[Serializable]
public class ConnectionData
{
    public string sessionName;
    public int sessionID;
    public string ipAddress;
    public int port;
    public ConnectionEvent connEvent;
    public ClientConnectionType clientType;

    private ConnectionData(string _ipAddress, int _port, ConnectionEvent _connEvent, ClientConnectionType _clientType = ClientConnectionType.NONE)
    {
        ipAddress = _ipAddress;
        port = _port;
        connEvent = _connEvent;
        clientType = _clientType;
    }

    /// <summary>
    /// Payload that the host multicast over UDP multicast so nearby devices can discover it.
    /// </summary>
    /// <param name="hostIP">Host's IP, so the client knows where to connect.</param>
    /// <param name="listenPort">TCP port SignalingServer is listening on.</param>
    public static ConnectionData ForMulticast(string hostIP, int listenPort)
    {
        return new ConnectionData(hostIP, listenPort, ConnectionEvent.MULTICAST, ClientConnectionType.NONE);
    }

    /// <summary>
    /// Payload a client sends back over TCP to complete the handshake and register itself.
    /// </summary>
    /// <param name="clientIP">Client's own IP, used as its identifier until replaced by a GUID.</param>
    /// <param name="clientType">Declares what kind of client this device is.</param>
    public static ConnectionData ForHandshake(string clientIP, ClientConnectionType clientType)
    {
        return new ConnectionData(clientIP, 0, ConnectionEvent.HANDSHAKE, clientType);
    }
}

/// <summary>
/// Class that identifies a connected client and links it to its HIMUClient
/// </summary>
public class ClientData
{
    public string clientID;                 // For WebSocket it is the clientID sent by the Node server; for the others it is a GUID             
    public ClientConnectionType type;
    public HIMUClient himuClient;

    private ClientData(ClientConnectionType _type, string _clientID)
    {
        type = _type;
        clientID = _clientID;
    }

    /// <summary>
    /// Builds a ClientData for a TCP device that just completed the handshake.
    /// </summary>
    /// <param name="connData">Handshake payload received from the device.</param>
    /// <param name="clientID">GUID assigned by SignalingServer to key this client.</param>
    public static ClientData ForDevice(ConnectionData connData, string clientID)
    {
        return new ClientData(connData.clientType, clientID);
    }

    /// <summary>
    /// Builds a ClientData for a browser client registered via the Node WebSocket.
    /// </summary>
    /// <param name="clientID">Unique key for this client received from Node server.</param>
    public static ClientData ForBrowser(string clientID)
    {
        return new ClientData(ClientConnectionType.WEB_SOCKET, clientID);
    }

    /// <summary>
    /// Builds a ClientData for a device connected through an ADB tunnel.
    /// </summary>
    /// <param name="clientID">GUID assigned by ADBConnectionServer to key this client.</param>
    public static ClientData ForADB(string clientID)
    {
        return new ClientData(ClientConnectionType.ADB, clientID);
    }
}

#endregion

#region Communication Structures

/// <summary>
/// Message shared between two devices
/// </summary>
[Serializable]
public class SignalingMessage
{
    public ConnectionEvent type;
    public string body;             // Serialized SDP or ICE's candidate JSON

    public SignalingMessage(ConnectionEvent e, string b)
    {
        type = e;
        body = b;
    }
}

/// <summary>
/// Information of a touch, with its position and finger id
/// </summary>
[System.Serializable]
public struct TouchData
{
    public float x;
    public float y;

    public TouchData(Vector2 position)
    {
        x = position.x;
        y = position.y;
    }
}

/// <summary>
/// Structure that represents the entire input state from a connected device
/// </summary>
[Serializable]
public class InputFrame
{
    public List<TouchData> touches;
    public Vector3 accelerometer;
    public float sentAt;                    // Time controlled by the SENDER
    public float receivedAt;                // Time controller by the RECIVER

    public InputFrame() 
    {
        touches = new List<TouchData>();
        accelerometer = Vector3.zero;
        sentAt = 0;
        receivedAt = 0;
    }

    public InputFrame(List<TouchData> t, Vector3 a, float sent)
    {
        touches = t;
        accelerometer = a;
        sentAt = sent;
    }
}

/// <summary>
/// Message structure for communication to NodeServer
/// </summary>
[Serializable]
public class WSMessage
{
    public int type;
    public int clientId;
    public string body;
}

#endregion

#region WebRTC Structures

/// <summary>
/// Wrapper that saves the values of an RTCIceCandidate so it can be serialized and sent
/// </summary>
[Serializable]
public class IceCandidateData
{
    public string candidate;
    public string sdpMid;
    public int sdpMLineIndex;

    public IceCandidateData(RTCIceCandidate c)
    {
        candidate = c.Candidate;
        sdpMid = c.SdpMid;
        sdpMLineIndex = c.SdpMLineIndex ?? 0;
    }
}

/// <summary>
/// Wrapper that saves the values of RTCSessionDescription
/// </summary>
[Serializable]
public class SessionDescriptionData
{
    public string type;  // "offer" or "answer"
    public string sdp;

    public SessionDescriptionData(RTCSessionDescription desc)
    {
        type = desc.type.ToString().ToLower();  // RTCSdpType.Offer -> "offer"
        sdp = desc.sdp;
    }

    /// <summary>
    /// Rebuilds the original RTCSessionDescription from the stored values
    /// </summary>
    public RTCSessionDescription ToRTCDesc()
    {
        return new RTCSessionDescription
        {
            type = type == "offer" ? RTCSdpType.Offer : RTCSdpType.Answer,
            sdp = this.sdp
        };
    }
}

#endregion