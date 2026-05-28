using System;
using System.Net.Sockets;
using System.Net.WebSockets;
using Unity.WebRTC;
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

public enum ClientType
{
    NONE,
    STREAM,
    PLAYER
}

[Serializable]
public class SignalingMessage
{
    public string sourceIp;
    public string destinationIp;       // IP destino, vacío = broadcast
    public ConnectionEvent type;
    public string body;    // SDP serializado o JSON del ICE candidate

    public SignalingMessage(string sIP, string dIP, ConnectionEvent e, string b)
    {
        sourceIp = sIP;
        destinationIp = dIP;
        type = e;
        body = b;
    }
}

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

    public RTCSessionDescription ToRTCDesc()
    {
        return new RTCSessionDescription
        {
            type = type == "offer" ? RTCSdpType.Offer : RTCSdpType.Answer,
            sdp = this.sdp
        };
    }
}

[Serializable]
public class ConnectionData
{
    public int port;
    public string name;
    public string info;
    public string ipAddress;
    public ConnectionEvent connType;
    public ClientType clientType;

    public ConnectionData(string ipAddress, int port, ConnectionEvent connEvent, ClientType clientType = ClientType.NONE)
    {
        this.ipAddress = ipAddress;
        this.port = port;
        this.connType = connEvent;
        this.clientType = clientType;
    }
}

public class ClientData
{
    public string ipAddress;
    public int port;
    public ClientType type;
    public NetworkStream stream;
    public WebRTCPeer webRtcPeer;

    public ClientData(ConnectionData connData, NetworkStream stream)
    {
        this.ipAddress = connData.ipAddress;
        this.port = connData.port;
        this.stream = stream;
        this.type = connData.clientType;
    }
}

[Serializable]
public class InputData
{
    public Vector2 move;      // Dirección de movimiento
    public Vector2 rotation;      // Delta de rotación
    public bool sprint;
    public bool moveUp;
    public bool moveDown;

    public InputData(Vector2 move, Vector2 rotation, bool sprint, bool moveUp, bool moveDown)
    {
        this.move = move;
        this.rotation = rotation;
        this.sprint = sprint;
        this.moveUp = moveUp;
        this.moveDown = moveDown;
    }
}

// Estructuras para la comunicacion con WebSocket
[Serializable] public class WSBaseMessage { public int type; }

[Serializable] public class WSNewClientMessage { public int type; public int clientId; }

[Serializable]
public class WSTaggedMessage
{
    public int type;
    public int clientId;
    public string body;
}