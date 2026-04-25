using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Custom ScriptableRendarPass that runs as part of the URP rendering pipeline. It captures
/// the camera's rendered frame into a RenderTexture for further use.
/// </summary>
public class FrameCapturePass : ScriptableRenderPass
{
    #region Variables
    /// <summary>
    /// The captured frame
    /// </summary>
    private RenderTexture outputTexture;

    /// <summary>
    /// Handle to OutputTexture so that Unity's RenderGraph system can track and manage it (wrapper)
    /// </summary>
    RTHandle outputHandle;

    /// <summary>
    /// Width of the captured frame (reduce to lower streaming bandwith)
    /// </summary>
    int width = 1280;

    /// <summary>
    /// Height of the captured frame (reduce to lower streaming bandwith)
    /// </summary>
    int height = 720;
    #endregion

    #region Methods
    /// <summary>
    /// Initializes the pass by creating the output RenderTexture with the defined values.
    /// </summary>
    public FrameCapturePass()
    {
        // BGRA32 -> format expected by WebRTC
        outputTexture = new RenderTexture(width, height, 0, RenderTextureFormat.BGRA32);

        // Allows the GPU to write on the texture if needed (like in Blit operations)
        outputTexture.enableRandomWrite = true;

        outputTexture.Create();

        // Asing in Unity's RenderGraph a handler to our capture texture
        outputHandle = RTHandles.Alloc(outputTexture);
    }


    /// <summary>
    /// Called by URP every frame to record the rendering commands into the render graph.
    /// Copies the camera's active color buffer into the OutputTexture via a blit pass,
    /// making the frame available for WebRTC streaming.
    /// </summary>
    /// <param name="renderGraph">The render graph used to declare and schedule GPU passes<</param>
    /// <param name="frameData">Container holding frame rendering data such as camera and resource info</param>
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // Retrieves the handles to the textures URP is currently using for rendering
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        // Obtain the camera's color buffer and check if it points to a valid buffer (should
        // never happen otherwise because it's being captured in AfterRenderingPostProcessing,
        // but just in case)
        TextureHandle source = resourceData.cameraColor;
        if (!source.IsValid())
        {
            Debug.LogWarning("[FrameCapturePass] cameraColor is not valid, skipping capture");
            return;
        }

        // Imports the external RenderTexture into the render graph so it can be used as
        // the destination of the copy of the camera's color buffer
        TextureHandle destination = renderGraph.ImportTexture(outputHandle);

        // Defines the parameters for the blit operation:
        //  - source -> the camera's active color buffer (what URP just rendered)
        //  - destination -> our OutputTexture
        //  - scale -> (1, 1), which means the whole source texture
        //  - offset -> (0, 0), no offset
        //  name of the pass
        renderGraph.AddBlitPass(source, destination, Vector2.one, Vector2.zero, passName: "FrameCapture");
    }

    public RenderTexture GetFrame()
    {
        return outputTexture;
    }

    /// <summary>
    /// Relases the memory allocated by OutputTexture when the pass is no longer needed
    /// </summary>
    public void Cleanup()
    {
        outputHandle?.Release();
        outputTexture?.Release();
    }
    #endregion
}