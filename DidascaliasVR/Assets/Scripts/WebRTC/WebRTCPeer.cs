using System.Collections;
using Unity.WebRTC;
using UnityEngine;

public class WebRTCPeer : MonoBehaviour
{
    RTCPeerConnection peer;
    VideoStreamTrack videoTrack;

    // Callback para enviar SDP/ICE al cliente remoto vía TCP
    public System.Action<SignalingMessage> OnSignalingMessage;
    public string RemoteIp;

    public void Initialize(RenderTexture source)
    {
        // Configuración de la conexión. Se usa STUN para descubrir la IP pública del dispositivo
        var config = new RTCConfiguration
        {
            iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } }
        };

        // Crear la conexión a partir de la configuración anterior
        peer = new RTCPeerConnection(ref config);
        
        // Recibir la información de un candidato y llamar a un callback
        peer.OnIceCandidate = candidate =>
        {
            SignalingMessage msg = new SignalingMessage(SignalingServer.ipAddress, RemoteIp, ConnectionEvent.ICE, JsonUtility.ToJson(new IceCandidateData(candidate)));
            OnSignalingMessage?.Invoke(msg);
        };

        // Debug para cuando la conexión cambia con el candidato
        peer.OnIceConnectionChange = state =>
            Debug.Log($"[WebRTCPeer] ICE state -> {state}");

        // Añadir el track de vídeo
        videoTrack = new VideoStreamTrack(source);
        peer.AddTrack(videoTrack);
    }

    // Llamado cuando el cliente remoto nos envía su SDP Answer
    public IEnumerator SetRemoteAnswer(RTCSessionDescription answer)
    {
        RTCSetSessionDescriptionAsyncOperation op = peer.SetRemoteDescription(ref answer);
        yield return op;
        if (op.IsError) Debug.LogError($"[WebRTCPeer] SetRemoteDescription: {op.Error.message}");
    }

    // Llamado cuando llega un ICE candidate del cliente remoto
    public void AddIceCandidate(RTCIceCandidateInit init)
    {
        peer.AddIceCandidate(new RTCIceCandidate(init));
    }

    // Genera la SDP Offer y la devuelve por callback
    public IEnumerator CreateOffer()
    {
        // Crea la oferta
        RTCSessionDescriptionAsyncOperation offerOp = peer.CreateOffer();
        yield return offerOp;

        // Asigna las cualidades de este dispositivo
        RTCSessionDescription offer = offerOp.Desc;
        RTCSetSessionDescriptionAsyncOperation setOp = peer.SetLocalDescription(ref offer);
        yield return setOp;

        // Envía el mensaje
        SignalingMessage msg = new SignalingMessage(SignalingServer.ipAddress, RemoteIp, ConnectionEvent.SDP, JsonUtility.ToJson(new SessionDescriptionData(offer)));
        OnSignalingMessage?.Invoke(msg);
    }

    void OnDestroy()
    {
        videoTrack?.Dispose();
        peer?.Close();
        peer?.Dispose();
    }
}
