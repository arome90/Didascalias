using EmteqLabs.Models;
using EmteqLabs.Video;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Video;

namespace EmteqLabs
{
    public class AffectiveVideoDemo : MonoBehaviour
    {
        [FormerlySerializedAs("customCalibration")] [SerializeField] private CustomCalibration _customCalibration;
        [SerializeField] private SrtVideoPlayer _videoPlayer;

        private void Awake()
        {
            _customCalibration.OnExpressionCalibrationComplete += OnExpressionCalibrationComplete;
        }

        private void Start()
        {
            // Sync framerate to monitors refresh rate
            QualitySettings.vSyncCount = 1;
            _customCalibration.gameObject.SetActive(true);
            _videoPlayer.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            _videoPlayer.OnShowSubtitle -= OnShowSubtitle;
        }

        private void OnExpressionCalibrationComplete(EmgCalibrationData expressionCalibrationData)
        {
            _customCalibration.OnExpressionCalibrationComplete -= OnExpressionCalibrationComplete;
            _customCalibration.gameObject.SetActive(false);
            _videoPlayer.gameObject.SetActive(true);
            PlayVideo();
        }

        private void PlayVideo()
        {
            _videoPlayer.OnShowSubtitle += OnShowSubtitle;
            _videoPlayer.OnHideSubtitle += OnHideSubtitle;
            _videoPlayer.OnVideoClipFinished += OnVideoClipFinished;
            _videoPlayer.Play();
        }
        
        private void OnShowSubtitle(string subtitle)
        {
            //Debug.Log(subtitle+" Show");
            EmteqVRManager.StartDataSection(subtitle);
        }

        private void OnHideSubtitle(string subtitle)
        {
            //Debug.Log(subtitle+" Hide");
            EmteqVRManager.EndDataSection(subtitle);
        }

        private void OnVideoClipFinished(VideoClip videoClip)
        {
            _videoPlayer.OnShowSubtitle -= OnShowSubtitle;
            _videoPlayer.OnHideSubtitle -= OnHideSubtitle;
            _videoPlayer.OnVideoClipFinished -= OnVideoClipFinished;
        }
    }
}
