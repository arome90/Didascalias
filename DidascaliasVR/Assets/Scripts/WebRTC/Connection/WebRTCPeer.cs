using System.Collections;
using System.Text;
using Unity.WebRTC;
using UnityEngine;
using UnityEngine.tvOS;

public class WebRTCPeer : MonoBehaviour
{

    #region Variables
    /// <summary>
    /// Object that represents the P2P connection
    /// </summary>
    RTCPeerConnection peer;

    /// <summary>
    /// IP from the client
    /// </summary>
    private string remoteIp;
    
    /// <summary>
    /// Object incharged of tracking the RenderTexture encodiing and transmision
    /// </summary>
    VideoStreamTrack videoTrack;

    /// <summary>
    /// Texture where the camera's view will be stored
    /// </summary>
    public RenderTexture renderTexture;

    /// <summary>
    /// Object incharged of tracking JSON packages
    /// </summary>
    RTCDataChannel dataChannel;

    /// <summary>
    /// Callback to send SDP/ICE to the remote client via TCP
    /// </summary>
    public System.Action<SignalingMessage> OnSignalingMessage;

    /// <summary>
    /// 
    /// </summary>
    PeerMovementComponent peerMovementComponent;

    #endregion

    #region Methods
    public void Initialize(string ip, RenderTexture rt, System.Action<SignalingMessage> onSignalingMsg)
    {
        remoteIp = ip;
        renderTexture = rt;
        OnSignalingMessage = onSignalingMsg;

        // Configuración de la conexión. Se usa STUN para descubrir la IP pública del dispositivo
        var config = new RTCConfiguration
        {
            iceServers = new[]
                {
                    new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } },
                    // Candidato host directo como fallback
                    new RTCIceServer { urls = new[] { "stun:stun1.l.google.com:19302" } }
                }
        };

        // Crear la conexión a partir de la configuración anterior
        peer = new RTCPeerConnection(ref config);
        
        // Recibir la información de un candidato y llamar a un callback
        peer.OnIceCandidate = candidate =>
        {
            SignalingMessage msg = new SignalingMessage(SignalingServer.ipAddress, remoteIp, ConnectionEvent.ICE, JsonUtility.ToJson(new IceCandidateData(candidate)));
            OnSignalingMessage?.Invoke(msg);
        };

        // Debug para cuando la conexión cambia con el candidato
        peer.OnIceConnectionChange = state =>
            Debug.Log($"[WebRTCPeer] ICE state -> {state}");

        // Añadir el track de vídeo
        videoTrack = new VideoStreamTrack(renderTexture);
        peer.AddTrack(videoTrack);

        // Data channel
        var dataChannelConfig = new RTCDataChannelInit { ordered = true };
        dataChannel = peer.CreateDataChannel("input", dataChannelConfig);

        dataChannel.OnOpen = () => Debug.Log("[DataChannel] Open");
        dataChannel.OnClose = () => Debug.Log("[DataChannel] Closed");
        dataChannel.OnMessage = bytes =>
        {
            string msg = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log($"[DataChannel] Recieved Message: {msg}");
            var inputMsg = JsonUtility.FromJson<InputData>(msg);
            peerMovementComponent.ApplyNetworkInput(inputMsg);
        };
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
        SignalingMessage msg = new SignalingMessage(SignalingServer.ipAddress, remoteIp, ConnectionEvent.SDP, JsonUtility.ToJson(new SessionDescriptionData(offer)));
        OnSignalingMessage?.Invoke(msg);
    }
    #endregion

    #region Monobehaviour
    private void Start()
    {
        peerMovementComponent = GetComponent<PeerMovementComponent>();
    }

    void OnDestroy()
    {
        videoTrack?.Dispose();
        peer?.Close();
        peer?.Dispose();
    }
    #endregion
}
