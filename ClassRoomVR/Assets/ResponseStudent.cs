using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi.TTS.Utilities;
using UnityEngine.Windows;
using Meta.WitAi.TTS.Interfaces;
using System;
using Unity.Mathematics;
using Meta.WitAi.TTS.Data;
using Meta.WitAi.TTS.Integrations;

public class ResponseStudent : MonoBehaviour
{
   // TTSWit TTSWit;
    TTSVoiceSettings voiceSettings;
    TTSSpeaker _speaker;
    [SerializeField] private string _dateId = "[DATE]";
     private AudioClip _asyncClip;

    // Start is called before the first frame update
    void Start()
    {
        _asyncClip = GetComponent<AudioClip>();
        _speaker = GetComponent<TTSSpeaker>();
        //_speaker.VoiceID = TTSWit.PresetVoiceSettings[0].SettingsId;
        //_speaker.customWitVoiceSettings.voice
    }

    public void TTS(string text) 
    {
        // Speak phrase
        string phrase = FormatText(text);
        // Speak async
        _speaker.Speak(phrase);
    }

    // Speak async
    private IEnumerator SpeakAsync(string phrase, bool queued)
    {
        // Queue
        if (queued)
        {
            yield return _speaker.SpeakQueuedAsync(new string[] { phrase });
        }
        // Default
        else
        {
            yield return _speaker.SpeakAsync(phrase);
        }

        // Play complete clip
        if (_asyncClip != null)
        {
            _speaker.AudioSource.PlayOneShot(_asyncClip);
        }
    }

    private string FormatText(string text)
    {
        string result = text;
        if (result.Contains(_dateId))
        {
            DateTime now = DateTime.Now;
            string dateString = $"{now.ToLongDateString()} at {now.ToLongTimeString()}";
            result = text.Replace(_dateId, dateString);
        }
        return result;
    }
}
