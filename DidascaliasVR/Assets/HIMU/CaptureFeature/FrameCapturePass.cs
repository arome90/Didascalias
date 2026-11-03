using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Custom ScriptableRendarPass that runs as part of the URP rendering pipeline. It captures
/// the camera's rendered frame into a RenderTexture that will be available to WebRTC to be
/// used for streaming.
/// </summary>
public class FrameCapturePass : ScriptableRenderPass
{

    #region Variables

    /// <summary>
    /// Data passed from the recording phase to the execution phase. RenderGraph records
    /// first and executes later, so nothing may be captured by closure.
    /// </summary>
    private class PassData
    {
        public TextureHandle source;
    }

    /// <summary>
    /// The captured frame. Accesible externally for WebRTC streaming.
    /// </summary>
    public RenderTexture OutputTexture { get; private set; }

    /// <summary>
    /// RTHandle wrapper required to import OutputTexture into the render graph.
    /// </summary>
    private RTHandle outputHandle;

    #endregion

    #region Methods

    /// <summary>
    /// Initializes the pass by creating the output RenderTexture with the defined values.
    /// </summary>
    public FrameCapturePass(int width, int height)
    {
        // BGRA32 -> format expected by WebRTC
        OutputTexture = new RenderTexture(width, height, 0, RenderTextureFormat.BGRA32);
        OutputTexture.name = "_FrameCaptureOutput";

        // The feature that owns this pass is an asset and survives Play Mode transitions.
        // Without DontSave, Unity destroys this texture on those transitions while the
        // managed wrapper stays alive, leaving a dangling reference behind.
        OutputTexture.hideFlags = HideFlags.HideAndDontSave;
        OutputTexture.Create();

        outputHandle = RTHandles.Alloc(OutputTexture);

        // Forces URP to render into an intermediate texture instead of straight to the
        // backbuffer. Without this, activeColorTexture may be the backbuffer, which
        // RenderGraph refuses to let us read from.
        requiresIntermediateTexture = true;
    }

    /// <summary>
    /// Declares the copy operation to the render graph.
    /// </summary>
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

        TextureHandle source = resourceData.activeColorTexture;
        if (!source.IsValid()) return;

        TextureHandle destination = renderGraph.ImportTexture(outputHandle);
        if (!destination.IsValid()) return;

        using (var builder = renderGraph.AddRasterRenderPass<PassData>("FrameCapture", out PassData passData))
        {
            passData.source = source;

            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

            // Nothing inside the graph consumes OutputTexture (WebRTC reads it from outside),
            // so the graph would otherwise consider this pass dead code and cull it.
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                Blitter.BlitTexture(ctx.cmd, data.source, new Vector4(1f, 1f, 0f, 0f), 0f, false));
        }
    }


    /// <summary>
    /// Relases the memory allocated by OutputTexture when the pass is no longer needed
    /// </summary>
    public void Cleanup()
    {
        if (outputHandle != null)
            outputHandle?.Release();
            
        if (OutputTexture != null)
        {
            OutputTexture?.Release();

#if UNITY_EDITOR
            if (!Application.isPlaying) Object.DestroyImmediate(OutputTexture);
            else Object.Destroy(OutputTexture);
#else
        Object.Destroy(OutputTexture);
#endif
        }

        outputHandle = null;
        OutputTexture = null;
    }

    #endregion

}