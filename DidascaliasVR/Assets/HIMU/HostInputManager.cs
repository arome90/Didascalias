using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Parses input events received from clients. It stores them into a list that is accessible for other scripts and flushes the events at lateUpdate (all processing must be done in update)
/// </summary>
public class HostInputManager : MonoBehaviour
{

    #region Variables

    /// <summary>
    /// Instance of InputManager (Singleton)
    /// </summary>
    public static HostInputManager Instance { get; private set; }

    /// <summary>
    /// Defines whether this scripts object persists between scenes or not
    /// </summary>
    [SerializeField]
    private bool shouldPersist = false;

    /// <summary>
    /// Time in seconds after which a client's input state stops being considered valid.
    /// </summary>
    [SerializeField]
    private float inputTimeout = 0.5f;

    /// <summary>
    /// Dictionary of events pending of processing (accessible)
    /// </summary>
    private readonly Dictionary<string, InputFrame> pendingInputFrames = new Dictionary<string, InputFrame>();

    #endregion

    #region Methods

    /// <summary>
    /// Parses data received from client
    /// </summary>
    /// <param name="data"></param>
    public void ParseInputMessage(string clientID, string data)
    {
        InputFrame frame;
        try
        {
            frame = JsonUtility.FromJson<InputFrame>(data);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("[InputManager] Malformed input frame from " + clientID + ":" + e.Message);
            return;
        }

        // Discards unordered inputs if the previous one is newer than the one received by comparing times
        if (pendingInputFrames.TryGetValue(clientID, out InputFrame previous)
            && frame.sentAt < previous.sentAt)
            return;

        frame.receivedAt = Time.unscaledTime;

        pendingInputFrames[clientID] = frame;
    }

    /// <summary>
    /// Removes a client from the input data map
    /// </summary>
    /// <param name="clientID"></param>
    public void RemoveClient(string clientID)
    {
        if (string.IsNullOrEmpty(clientID)) return;
        pendingInputFrames.Remove(clientID);
    }

    #endregion

    #region Getters

    /// <summary>
    /// Access to pending InputFrames list
    /// </summary>
    /// <returns></returns>
    public InputFrame GetInputFrame(string clientID)
    {
        InputFrame frame = GetValidFrame(clientID);
        return frame != null ? frame : null;
    }

    /// <summary>
    /// Returns a valid frame for a client by checking if it exists and if it passes the timeout threshold
    /// </summary>
    /// <param name="clientID">Client of the frame</param>
    /// <returns>InputFrame if valid, null otherwise</returns>
    private InputFrame GetValidFrame(string clientID)
    {
        if (string.IsNullOrEmpty(clientID)) return null;
        if (!pendingInputFrames.TryGetValue(clientID, out InputFrame frame)) return null;
        if (Time.unscaledTime - frame.receivedAt > inputTimeout) return null;

        return frame;
    }

    #endregion

    #region Monobehaviour

    private void Awake()
    {
        if (Instance) {
            Instance.gameObject.SetActive(false);
            Destroy(Instance.gameObject);
        }

        Instance = this;

        if (shouldPersist) DontDestroyOnLoad(gameObject);
    }

    #endregion

}
