using ElevenLabs;
using ElevenLabs.Voices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class VoiceGenerator : MonoBehaviour
{
    [SerializeField] ElevenLabsConfiguration apiKey; // API key for ElevenLabs
    [SerializeField] Voice voice; // Voice to use for generating the clip
    [SerializeField] VoiceSettings defaultVoiceSettings; // Default voice settings

    private ElevenLabsClient api; // Instance of the ElevenLabs API client
    private AudioSource audioSource; // Unity AudioSource component to play the audio clip

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component on this GameObject
        api = new ElevenLabsClient(apiKey.ApiKey); // Initialize the API client with the provided API key
    }

    // Method to asynchronously generate a voice clip from the provided text
    public async Task GenerateVoiceClipAsync(string text)
    {
        // Retrieve all available voices from the API
        var allVoice = await api.VoicesEndpoint.GetAllVoicesAsync();

        // Assign the first voice from the list of available voices (you might want to select a specific voice)
        voice = allVoice.First();

        // Get the default voice settings from the API
        defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();

        // Convert the provided text to an audio clip using the selected voice and settings
        var (clipPath, audioClip) = await api.TextToSpeechEndpoint.TextToSpeechAsync(text, voice, defaultVoiceSettings, ElevenLabs.Models.Model.MultiLingualV1);

        // Assign the generated audio clip to the AudioSource and play it
        audioSource.clip = audioClip;
        audioSource.Play();

        // TODO: delete clippath
    }
}
