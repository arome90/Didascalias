using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.WitAi.TTS.Utilities;
using UnityEngine.Windows;
using Meta.WitAi.TTS.Interfaces;
using System;

public class ResponseStudent : MonoBehaviour
{
    TTSSpeaker _speaker;
    [SerializeField] private string _dateId = "[DATE]";
     private AudioClip _asyncClip;

    // Start is called before the first frame update
    void Start()
    {
        _asyncClip = transform.GetChild(0).GetComponent<AudioClip>();
        _speaker = GetComponent<TTSSpeaker>();
        //_speaker.customWitVoiceSettings.voice
        Invoke(nameof(TTS), 3);
    }

    void TTS() 
    {
        
        // Speak phrase
        string phrase = FormatText("Hola buenas tardes, que tal estas profesor ?");

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

    // Update is called once per frame
    void Update()
    {
        
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
