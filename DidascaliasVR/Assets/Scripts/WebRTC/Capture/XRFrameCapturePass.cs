using Didascalia;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

/// <summary>
/// Custom ScriptableRendarPass that runs as part of the URP rendering pipeline. It captures
/// the sliced camera's rendered frame into a RenderTexture for further use.
/// </summary>
public class XRFrameCapturePass : FrameCapturePass
{
    #region Variables
    class PassData
    {
        public TextureHandle source;
    }
    #endregion

    #region Methods
    /// <summary>
    /// Called by URP every frame to record the rendering commands into the render graph.
    /// Copies the camera's active color buffer into the OutputTexture via a blit pass,
    /// making the frame available for WebRTC streaming.
    /// </summary>
    /// <param name="renderGraph">The render graph used to declare and schedule GPU passes<</param>
    /// <param name="frameData">Container holding frame rendering data such as camera and resource info</param>
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        // Calls the parent RecordRenderGraph to set source and destination data
        base.RecordRenderGraph(renderGraph, frameData);

        // Custom raster pass: copies slice 0 of the source (left eye in stereo) into our 2D
        // destination using Blitter.BlitTexture. This avoids the "too many slices" error that
        // RenderGraphUtils.AddBlitPass throws when source is a Texture2DArray and destination
        // is a regular 2D RenderTexture.
        using (var builder = renderGraph.AddRasterRenderPass<PassData>("FrameCapture", out var passData))
        {
            passData.source = source;

            // Declare resource usage so RenderGraph schedules barriers correctly
            builder.UseTexture(source, AccessFlags.Read);
            builder.SetRenderAttachment(destination, 0, AccessFlags.Write);

            // Capture pass shouldn't be culled even if no other pass reads its output
            builder.AllowPassCulling(false);

            builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
            {
                // In case there's any transformation needed, but right now this is just
                // the identity copy
                Vector4 scaleBias = new Vector4(1f, 1f, 0f, 0f);

                // sourceSlice = 0 -> left eye in stereo, also valid for non-XR sources.
                // mipLevel = 0, bilinear = false (1:1 copy, no filtering needed)
                Blitter.BlitTexture(ctx.cmd, data.source, scaleBias, 0, false);
            });
        }
    }
    #endregion
}
