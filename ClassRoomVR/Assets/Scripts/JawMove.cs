using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(AudioSource))]
public class JawMove : MonoBehaviour
{
    private AudioSource audioSource;
    [SerializeField] Transform jaw;
    [SerializeField] float SizeReduction = 500; // Scale factor to adjust the frequency's effect on the rotation
    [SerializeField] float smoothSpeed = 5f; // Base speed of the smoothing, used for minor adjustments
    [SerializeField] float fastSmoothSpeed = 20f; // Increased speed for significant changes
    [SerializeField] float sensitivity = 1f; // Sensitivity for determining significant changes

    private float targetZRotation; // To keep track of the target X rotation
    private float lastMaxValue = 0f; // To store the last frame's max spectrum value for comparison
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
   void UpdateJaw()
    {        
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

        targetZRotation = Mathf.Lerp(targetZRotation, frequency, currentSmoothSpeed * Time.deltaTime);
        targetZRotation = Mathf.Clamp(targetZRotation, 0, 40); // Ensuring target rotation is also clamped
        jaw.eulerAngles = new Vector3(jaw.eulerAngles.x, jaw.eulerAngles.y,25 + targetZRotation);
        lastMaxValue = maxValue;
    }

   public IEnumerator OnCompleteSpeach()
   {
        while (audioSource.isPlaying)
        {
            UpdateJaw();
            yield return new WaitForSeconds(0.4f);
        }
        jaw.eulerAngles = new Vector3(jaw.eulerAngles.x, jaw.eulerAngles.y, 16);

   }
}
