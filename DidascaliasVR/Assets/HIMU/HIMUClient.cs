using System;
using System.Collections;
using Unity.WebRTC;
using UnityEngine;

/// <summary>
/// Representation of a client peer to this host, incharge of initializing and handling the WebRTC connection and its exchange of data.
/// </summary>
public class HIMUClient : MonoBehaviour
{

    #region Variables

    /// <summary>
    /// Object that represents the P2P connection.
    /// </summary>
    RTCPeerConnection peer;

    /// <summary>
    /// This client ID in the overall connected network.
    /// </summary>
    private string clientID;

    /// <summary>
    /// Texture where the camera's view will be stored.
    /// </summary>
    public RenderTexture renderTexture;

    /// <summary>
    /// Referencia al sender del video track.
    /// </summary>
    RTCRtpSender videoSender;

    /// <summary>
    /// Object incharged of tracking the RenderTexture encodiing and transmision.
    /// </summary>
    VideoStreamTrack videoTrack;

    /// <summary>
    /// Object incharged of tracking JSON packages.
    /// </summary>
    RTCDataChannel inputTrack;

    /// <summary>
    /// Callback to send SDP/ICE to the remote client via TCP.
    /// </summary>
    public System.Action<SignalingMessage> OnSignalingMessage;

    /// <summary>
    /// Whether this client needs a DataChannel for input or not.
    /// </summary>
    private bool handlesInput = false;

    #endregion

    #region Methods

    /// <summary>
    /// Initializes all WebRTC components for this peer: WebRTC configuration and streaming and data
    /// reception callbacks.
    /// </summary>
    /// <param name="id">Id of this client.</param>
    /// <param name="rt">RenderTexture of the attached camera for streaming.</param>
    /// <param name="onSignalingMsg">Callback for when a SignalingMessage is received.</param>
    public void Initialize(string id, RenderTexture rt, System.Action<SignalingMessage> onSignalingMsg, bool input)
    {
        clientID = id;
        renderTexture = rt;
        OnSignalingMessage = onSignalingMsg;
        handlesInput = input;

        // Connection configuration. Uses STUN to discover the public IP of this device if needed due to an intermediate NAT.
        RTCConfiguration config = new RTCConfiguration
        {
            iceServers = new[] { new RTCIceServer { urls = new[] { "stun:stun.l.google.com:19302" } } }
        };

        // Creates the connection with the previous configuration.
        peer = new RTCPeerConnection(ref config);

        // Callback for when receiving a candidate.
        peer.OnIceCandidate = candidate =>
        {
            SignalingMessage msg = new SignalingMessage(ConnectionEvent.ICE, JsonUtility.ToJson(new IceCandidateData(candidate)));
            OnSignalingMessage?.Invoke(msg);
        };

        peer.OnIceConnectionChange = state =>
            Debug.Log($"[WebRTCPeer] ICE state -> {state}");

        // Add videotrack to the connection.
        videoTrack = new VideoStreamTrack(renderTexture);
        videoSender = peer.AddTrack(videoTrack);

        // Data channel configuration - Input
        if (handlesInput)
        {
            var dataChannelConfig = new RTCDataChannelInit { ordered = false, maxRetransmits = 0 };
            inputTrack = peer.CreateDataChannel("input", dataChannelConfig);

            inputTrack.OnOpen = () => Debug.Log("[DataChannel] Open");
            inputTrack.OnClose = () => Debug.Log("[DataChannel] Closed");
            inputTrack.OnMessage = bytes =>
            {
                string msg = System.Text.Encoding.UTF8.GetString(bytes);
                UnityMainThreadDispatcher.Instance().Enqueue(() => DeliverInputMessage(msg));
            };
        }
    }

    /// <summary>
    /// Called when the remote client sends their SDP Answer.
    /// </summary>
    /// <param name="answer">Client's SDP Answer.</param>
    /// <returns></returns>
    public IEnumerator SetRemoteAnswer(RTCSessionDescription answer)
    {
        RTCSetSessionDescriptionAsyncOperation op = peer.SetRemoteDescription(ref answer);
        yield return op;
        if (op.IsError) Debug.LogError($"[WebRTCPeer] SetRemoteDescription: {op.Error.message}");
    }

    /// <summary>
    /// Called when an ICE candidate is received from the remote client. Adds it to the connection.
    /// </summary>
    /// <param name="init">Initialization options for creating an ICE candidate.</param>
    public void AddIceCandidate(RTCIceCandidateInit init)
    {
        peer.AddIceCandidate(new RTCIceCandidate(init));
    }

    /// <summary>
    /// Generates an SDP offer and sends it.
    /// </summary>
    /// <returns></returns>
    public IEnumerator CreateOffer()
    {
        // Create offer
        RTCSessionDescriptionAsyncOperation offerOp = peer.CreateOffer();
        yield return offerOp;

        // Assign the qualities of this device.
        RTCSessionDescription offer = offerOp.Desc;
        RTCSetSessionDescriptionAsyncOperation setOp = peer.SetLocalDescription(ref offer);
        yield return setOp;

        // Sends the SignalingMessage with this information.
        string json = JsonUtility.ToJson(new SessionDescriptionData(offer));
        SignalingMessage msg = new SignalingMessage(ConnectionEvent.SDP, json);
        OnSignalingMessage?.Invoke(msg);
    }

    /// <summary>
    /// Sends the input message with the GUID of this peer attached to it.
    /// </summary>
    /// <param name="msg"></param>
    private void DeliverInputMessage(string msg)
    {
        if (HostInputManager.Instance == null)
        {
            Debug.LogError("[HIMUClient] No InputManager in the scene: input from " + clientID + " is being dropped.");
            return;
        }

        HostInputManager.Instance.ParseInputMessage(clientID, msg);
    }

    /// <summary>
    /// Swaps the RenderTexture being streamed to this client, replacing the current video
    /// track with a new one without renegotiating the connection.
    /// </summary>
    /// <param name="texture">New RenderTexture to stream.</param>
    public void ChangeTexture(RenderTexture texture)
    {
        if (texture == null)
        {
            Debug.LogError("[HIMUClient] Texture is null");
            return;
        }

        if (videoSender == null)
        {
            Debug.LogError("[HIMUClient] VideoSender is null");
            return;
        }

        try
        {
            VideoStreamTrack newVST = new VideoStreamTrack(texture);

            bool replaced = videoSender.ReplaceTrack(newVST);
            Debug.Log($"ReplaceTrack returned: {replaced}");

            if (!replaced)
            {
                newVST.Dispose();
                return;
            }

            videoTrack?.Dispose();
            videoTrack = newVST;
        }
        catch (ObjectDisposedException ex)
        {
            Debug.LogError($"ObjectDisposedException: {ex.StackTrace}");
            throw;
        }
    }

    #endregion

    #region Getters

    /// <summary>
    /// Returns the video track object.
    /// </summary>
    /// <returns>Video track object.</returns>
    public RTCRtpSender GetVideoSender()
    {
        return videoSender;
    }

    /// <summary>
    /// Returns this peer's id
    /// </summary>
    /// <returns>Client id assigned</returns>
    public string GetClientID()
    {
        return clientID;
    }

    #endregion

    #region Monobehaviour

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Closes the connection and cleans its objects.
    /// </summary>
    void OnDestroy()
    {
        videoTrack?.Dispose();
        inputTrack?.Close();
        inputTrack?.Dispose();
        peer?.Close();
        peer?.Dispose();
    }
    #endregion

}

