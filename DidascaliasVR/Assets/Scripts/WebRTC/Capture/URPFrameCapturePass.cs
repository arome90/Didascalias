using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Custom ScriptableRendarPass that runs as part of the URP rendering pipeline. It captures
/// the camera's rendered frame into a RenderTexture for further use.
/// </summary>
public class URPFrameCapturePass : FrameCapturePass
{
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

        // Defines the parameters for the blit operation:
        //  - source -> the camera's active color buffer (what URP just rendered)
        //  - destination -> our OutputTexture
        //  - scale -> (1, 1), which means the whole source texture
        //  - offset -> (0, 0), no offset
        //  name of the pass
        renderGraph.AddBlitPass(source, destination, Vector2.one, Vector2.zero, passName: "FrameCapture");
    }
    #endregion
}