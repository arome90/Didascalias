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
using ClassRoomVR;
using Newtonsoft.Json.Linq;

public class ResponseStudent : MonoBehaviour
{
   // TTSWit TTSWit;
    TTSVoiceSettings voiceSettings;
    TTSSpeaker _speaker;
    [SerializeField] private string _dateId = "[DATE]";
     private AudioClip _asyncClip;
    Student student;
    private bool speak;
    public bool Speak { get { return speak; } }
    // Start is called before the first frame update
    void Start()
    {
        student= GetComponent<Student>();
        _asyncClip = GetComponent<AudioClip>();
        _speaker = GetComponent<TTSSpeaker>();
        _speaker.VoiceID = student.GetGender()== Gender.Men ? "WIT$CAM": "MARIA";       
        _speaker.Events.OnComplete.AddListener(a);
    }

    public void TTS(string text) 
    {
        // Speak phrase
        student.GetNameText().color = Color.green;
        string phrase = FormatText(text);
        // Speak async
        _speaker.Speak(phrase);
        speak = true;
        student.MoveJaw();

    }

    private void a(TTSSpeaker s, TTSClipData data)
    {
        student.GetNameText().color = Color.white;
        speak = false;

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
