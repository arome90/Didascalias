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
    /// Width of the captured frame.
    /// </summary>
    [SerializeField]
    private int width = 1280;

    /// <summary>
    /// Height of the captured frame.
    /// </summary>
    [SerializeField]
    private int height = 720;

    /// <summary>
    /// The pass incharge of capturing the frame each render cycle.
    /// </summary>
    FrameCapturePass pass = null;

    /// <summary>
    /// The only camera whose frame gets captured (the game's main camera).
    /// </summary>
    private Camera sourceCamera;

    /// <summary>
    /// If the capture of the frame is or not enabled (as to say, if there are browsers connected).
    /// </summary>
    private bool captureEnabled;

    #endregion

    #region URP Methods

    /// <summary>
    /// Called by URP when the feature is initialized. It creates our custom pass and appends it
    /// at the end of the render pipeline.
    /// </summary>
    public override void Create()
    {
        Instance = this;

        // URP calls Create without a matching Dispose, so the previous data needs to be realesed by
        // explicitly calling its release now (Cleanup)
        pass?.Cleanup();
        pass = new FrameCapturePass(width, height);

        // Adds the pass to URP's render pipeline after everything has been rendered. This way
        // allows postprocessing and UI to be included in the captured frame because it will
        // always be the last pass the pipeline will do.
        // NOTE: For the UI to be present in this phase -> Canvas: Screen Space Camera, with
        // the camera as the rendertarget
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
        if (!Application.isPlaying) return;

        // No need to enqueue if there are no consumers
        if (!captureEnabled) return;

        // Don't enqueue if a camera wasn't assigned but there are consumers waiting
        if (sourceCamera == null)
        {
            //Debug.LogError("[FrameCaptureFeature] Capture is enabled but there is no camera assigned to render (Did you call SetSourceCamera()?).");
            return;
        }

        // Only captures the camera set to be streamed
        if (renderingData.cameraData.camera != sourceCamera) return;

        // Adds the pass into URP's render loop for this frame
        renderer.EnqueuePass(pass);
    }

    /// <summary>
    /// Called by URP when the feature is destroyed. It calls the pass cleanup to avoid
    /// memory leaks.
    /// </summary>
    /// <param name="disposing"></param>
    protected override void Dispose(bool disposing)
    {
        pass?.Cleanup();
        pass = null;
    }

    #endregion

    #region Setters & Getters

    /// <summary>
    /// Registers the camera to capture from. Called once by the scene's game manager.
    /// </summary>
    /// /// <param name="cam">The streamed camera.</param>
    public void SetSourceCamera(Camera cam)
    {
        sourceCamera = cam;
    }

    /// <summary>
    /// Marks the capture as enabled so that the pass is done when URP tries to enqueue it.
    /// </summary>
    /// <param name="enabled">If the capture is enabled.</param>
    public void SetCaptureEnabled(bool enabled)
    {
        captureEnabled = enabled;
    }

    /// <summary>
    /// Whether the capture is enabled or not
    /// </summary>
    /// <returns></returns>
    public bool IsEnabled()
    {
        return captureEnabled;
    }

    /// <summary>
    /// Returns the frame captured by the pass.
    /// </summary>
    /// <returns>Captured frame, null if one wasn't captured</returns>
    public RenderTexture GetFrame()
    {
        return pass?.OutputTexture;
    }

    #endregion

}