using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// An Scriptabler Renderer Feature that adds the pass into URP's rendering pipeline. It is the
/// bridge between:
///     - URP and our custom pass, via the methods it overrides that will be called in the pipeline
///     - The pass and WebRTC, via the singleton instance so that the captured frame can be access
///     externally.
/// </summary>
public class FrameCaptureFeature : ScriptableRendererFeature
{
    #region Variables
    /// <summary>
    /// Singleton instace of this class allowing the access to the captured frame. Is the 
    /// bridge between the capture components and the WebRTC components.
    /// </summary>
    public static FrameCaptureFeature Instance { get; private set; }

    /// <summary>
    /// The URP pass incharge of capturing the frame each render cycle.
    /// </summary>
    FrameCapturePass pass;
    #endregion

    #region Methods
    /// <summary>
    /// Called by URP when the feature is initialized. It creates our custom pass and appends it
    /// at the end of the render pipeline.
    /// </summary>
    public override void Create()
    {
        // Registers this instance as a singleton
        Instance = this;

        // Create the passes for each render type
        pass = new FrameCapturePass();

        // Adds the passes to URP's render pipeline after everything has been rendered. This way
        // allows postprocessing and UI to be included in the captured frame because it will
        // always be the last pass the pipeline will do.
        pass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    }


    /// <summary>
    /// Called by URP every frame to determine if FrameCapturePass should be enqueued to the
    /// render pipeline.
    /// </summary>
    /// <param name="renderer">The active URP renderer</param>
    /// <param name="renderingData">Rendering data for the current frame</param>
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        // Prevents the pass from running in the Editor outside of Play Mode
        if (!Application.isPlaying)
            return;

        // Only captures the Game camera ignoring other camera types like the scene view
        if (renderingData.cameraData.cameraType != CameraType.Game)
            return;

        // If it is an XR Camera, don't capture it
        if (renderingData.cameraData.xrRendering)
            return;

        // Adds the pass into URP's render loop for this frame
        renderer.EnqueuePass(pass);
    }


    /// <summary>
    /// Returns the frame captured by the pass.
    /// </summary>
    /// <returns>Captured frame, null if one wasn't captured</returns>
    public RenderTexture GetFrame()
    {
        return pass.GetFrame();
    }
    

    /// <summary>
    /// Called by URP when the feature is destroyed. It calls the pass cleanup to avoid
    /// memory leaks.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        pass?.Cleanup();
    }
    #endregion
}