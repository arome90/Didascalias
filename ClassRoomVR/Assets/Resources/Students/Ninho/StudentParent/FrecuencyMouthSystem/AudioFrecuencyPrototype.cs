using UnityEngine;
using System.Collections; // Required for IEnumerator

[RequireComponent(typeof(AudioSource))]
public class SimpleAudioFrequencyRotationEulerClamped : MonoBehaviour
{
    [Header("Rotation by Frequency")]
    public float SizeReduction = 500; // Scale factor to adjust the frequency's effect on the rotation
    public float smoothSpeed = 5f; // Base speed of the smoothing, used for minor adjustments
    public float fastSmoothSpeed = 20f; // Increased speed for significant changes
    public float sensitivity = 1f; // Sensitivity for determining significant changes

    private float targetXRotation; // To keep track of the target X rotation
    private float lastMaxValue = 0f; // To store the last frame's max spectrum value for comparison

    [Header("Real Audio")]
    public AudioClip soundClip; // The sound clip to play
    public float delayInSeconds = 1.0f; // Delay in seconds before the sound plays, editable in Inspector
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlaySoundAfterDelay());
    }

    void Update()
    {
        if (!audioSource.isPlaying) return; // Only proceed if the audio is playing

        float[] spectrum = new float[1024];
        audioSource.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
        int maxIndex = 0;
        float maxValue = 0f;
        for (int i = 0; i < spectrum.Length; i++)
        {
            if (spectrum[i] > maxValue)
            {
                maxIndex = i;
                maxValue = spectrum[i];
            }
        }
        float frequency = (maxIndex * AudioSettings.outputSampleRate / 2 / spectrum.Length) / SizeReduction;

        frequency = Mathf.Clamp(frequency, 0, 40); // Clamping frequency to desired range

        float valueDifference = Mathf.Abs(maxValue - lastMaxValue);

        float currentSmoothSpeed = valueDifference > sensitivity * lastMaxValue ? fastSmoothSpeed : smoothSpeed;

        targetXRotation = Mathf.Lerp(targetXRotation, frequency, currentSmoothSpeed * Time.deltaTime);
        targetXRotation = Mathf.Clamp(targetXRotation, 0, 40); // Ensuring target rotation is also clamped

        transform.eulerAngles = new Vector3(targetXRotation, transform.eulerAngles.y, transform.eulerAngles.z);

        lastMaxValue = maxValue;
    }

    IEnumerator PlaySoundAfterDelay()
    {
        yield return new WaitForSeconds(delayInSeconds);

        audioSource.clip = soundClip;
        audioSource.Play();
    }
}
