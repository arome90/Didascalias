using UnityEngine;
using System.IO;

/// <summary>
/// Debug component that saves the frame captured by FrameCaptureFeature as a PNG file.
/// Attach to any GameObject in the scene. Press Space to capture.
/// </summary>
public class FrameCaptureDebug : MonoBehaviour
{
    /// <summary>
    /// Key to press to trigger the capture. Space by default.
    /// </summary>
    [SerializeField] KeyCode captureKey = KeyCode.Space;

    /// <summary>
    /// Folder where the PNG will be saved, relative to the project root.
    /// </summary>
    [SerializeField] string outputFolder = "DebugCaptures";

    private void Start()
    {
        //InvokeRepeating(nameof(SaveFrame), 0f, 1f / 30f);
    }

    void Update()
    {
        
    }


    void SaveFrame()
    {
        //RenderTexture rt = FrameCaptureFeature.Instance?.GetFrame();

        //// Check that the feature has captured a frame
        //if (rt == null)
        //{
        //    Debug.LogError("[FrameCaptureDebug] No frame available. Is FrameCaptureFeature active?");
        //    return;
        //}

        //// Read the RenderTexture pixels back from the GPU into a Texture2D on the CPU
        //// NOTE: This is expensive (GPU readback stall) but acceptable for debug purposes
        //Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.BGRA32, false);
        //RenderTexture previous = RenderTexture.active;
        //RenderTexture.active = rt;
        //tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        //tex.Apply();
        //RenderTexture.active = previous;

        //// Encode to PNG and save to disk
        //byte[] bytes = tex.EncodeToPNG();
        //Destroy(tex);

        //string folder = Path.Combine(Application.dataPath, "..", outputFolder);
        //Directory.CreateDirectory(folder);

        //string filename = $"frame_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        //string path = Path.Combine(folder, filename);

        //File.WriteAllBytes(path, bytes);
        //Debug.Log($"[FrameCaptureDebug] Frame saved to: {Path.GetFullPath(path)}");

        //StreamManager.Instance?.SendFrame(FrameCaptureFeature.Instance?.GetFrame());
        Debug.Log("[Sender] Image sent");
    }
}