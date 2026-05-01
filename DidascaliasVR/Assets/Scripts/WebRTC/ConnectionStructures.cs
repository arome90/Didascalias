using System;
using System.Net.Sockets;
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

[Serializable]
public class SignalingMessage
{
    public string sourceIp;
    public string destinationIp;       // IP destino, vacío = broadcast
    public ConnectionEvent type;
    public string body;    // SDP serializado o JSON del ICE candidate
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
    public string type;  // "offer" o "answer"
    public string sdp;

    public SessionDescriptionData(RTCSessionDescription desc)
    {
        type = desc.type.ToString().ToLower();  // RTCSdpType.Offer -> "offer"
        sdp = desc.sdp;
    }

    public RTCSessionDescription ToDesc()
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
