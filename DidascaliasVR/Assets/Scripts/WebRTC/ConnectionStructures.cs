using System;
using System.Net.Sockets;

public enum ConnectionEvent
{
    DEFAULT,
    BROADCAST,
    HANDSHAKE,
    SEND,
    DISCONNECT
}

[Serializable]
public class ConnectionData
{
    public string ipAddress;
    public int port;
    public ConnectionEvent connEvent;

    public ConnectionData(string ipAddress, int port, ConnectionEvent connEvent)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        this.connEvent = connEvent;
    }
}

public class ClientWebRTC
{   
    public string ipAddress;

    public int port;

    NetworkStream stream;

    public ClientWebRTC(ConnectionData connData, NetworkStream stream)
    {
        this.ipAddress = connData.ipAddress;
        this.port = connData.port;
        this.stream = stream;
    }
}
