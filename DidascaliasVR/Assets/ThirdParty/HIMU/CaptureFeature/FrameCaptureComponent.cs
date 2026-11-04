using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameCaptureComponent : MonoBehaviour
{

    #region Operations

    /// <summary>
    /// Copies the source camera rendered texture to the one being sent to the client.
    /// </summary>
    private void BlitToBrowsers()
    {
        RenderTexture frame = FrameCaptureFeature.Instance.GetFrame();
        if (frame == null) return;

        List<ClientData> browsers = StreamManager.Instance.GetBrowserClients();
        foreach (ClientData c in browsers)
        {
            if (c.himuClient.renderTexture != null)
                Graphics.Blit(frame, c.himuClient.renderTexture);
        }
    }

    #endregion

    #region Monobehaviour

    private IEnumerator Start()
    {
        // If we don't wait till the end of Unity's pipeline, we are goint to send the frame N-1 in frame N. In order to blit the current
        // one we start a corrutine that waits till the frame is done rendering.
        var wait = new WaitForEndOfFrame();
        while (true)
        {
            yield return wait;
            BlitToBrowsers();
        }
    }

    private void LateUpdate()
    {
        if (FrameCaptureFeature.Instance == null || StreamManager.Instance == null) return;

        // Set the capturing of the current frame if there are browsers connected.
        FrameCaptureFeature.Instance.SetCaptureEnabled(StreamManager.Instance.GetBrowserClients().Count > 0);
    }

    #endregion

}
