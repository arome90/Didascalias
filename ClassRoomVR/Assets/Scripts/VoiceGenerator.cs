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
    [SerializeField] ElevenLabsConfiguration apiKey;
    // public string text;
    [SerializeField] Voice voice;
    [SerializeField] VoiceSettings defaultVoiceSettings;

    private ElevenLabsClient api;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        api = new ElevenLabsClient(apiKey.ApiKey);
    }

    public async Task GenerateVoiceClipAsync(string text) 
    {
        var allVoice = await api.VoicesEndpoint.GetAllVoicesAsync();
        voice = allVoice.First();
        defaultVoiceSettings = await api.VoicesEndpoint.GetDefaultVoiceSettingsAsync();
        
        var (clipPath, audioClip) = await api.TextToSpeechEndpoint.TextToSpeechAsync("hola", voice, defaultVoiceSettings,ElevenLabs.Models.Model.MultiLingualV1);
        audioSource.clip = audioClip;
        audioSource.Play();

        //Borrar clipath
    }


}
