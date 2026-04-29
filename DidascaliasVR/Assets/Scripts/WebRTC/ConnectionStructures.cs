using System;
using System.Net.Sockets;
using UnityEngine;

public enum ConnectionEvent
{
    DEFAULT,
    BROADCAST,
    HANDSHAKE,
    SEND,
    DISCONNECT,
    SDP,        // SDP: Session Description Protocol (offer/answer)
    ICE         // ICE: Interactive Connectivity Establishment (ICE candidates)
}

[Serializable]
public class SignalingMessage
{
    public string sourceIp;
    public string destinationIp;       // IP destino, vacío = broadcast
    public ConnectionEvent type;
    public string body;    // SDP serializado o JSON del ICE candidate
}

[Serializable]
public class ConnectionData
{
    public string ipAddress;
    public int port;
    public ConnectionEvent type;

    public ConnectionData(string ipAddress, int port, ConnectionEvent connEvent)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        this.type = connEvent;
    }
}

public class ClientWebRTC
{   
    public string ipAddress;

    public int port;

    public NetworkStream stream;
    // public WebRTCPeer webRtcPeer;

    public Camera cam;

    public ClientWebRTC(ConnectionData connData, NetworkStream stream)
    {
        this.ipAddress = connData.ipAddress;
        this.port = connData.port;
        this.stream = stream;
    }
}
