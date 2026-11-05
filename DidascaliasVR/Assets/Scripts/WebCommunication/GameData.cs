using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Didascalia
{
    [Serializable]
    public class BaseData
    {
        public string Time;

        public BaseData()
        {
            Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
    }

    [Serializable]
    public class VoiceData : BaseData
    {
        public float Volume, Pitch;
        public VoiceData(float v, float p)
        {
            Volume = v;
            Pitch = p;
        }
    }

    [Serializable]
    public class VolumeData : BaseData
    {
        public float volume;
        public VolumeData(float v)
        {
            volume = v;
        }
    }

    [Serializable]
    public class SpeechData : BaseData
    {
        public string TranscribedText;
        public string RecognitionStatus;

        public SpeechData(string text, string status = "Success")
        {
            TranscribedText = text;
            RecognitionStatus = status;
        }
    }

    [Serializable]
    public class PlayerData : BaseData
    {
        public float HeadVelocity;
        public float LeftHandVelocity;
        public float RightHandVelocity;

        public PlayerData(float headVelocity, float leftHandVelocity, float rightHandVelocity)
        {
            HeadVelocity = headVelocity;
            LeftHandVelocity = leftHandVelocity;
            RightHandVelocity = rightHandVelocity;
        }
    }

    [Serializable]
    public class EventData : BaseData
    {
        public string ActionInfo;
        public string EventType;
        public List<string> Alumnxs;

        public EventData(string action, string type, List<string> alumnxs)
        {
            ActionInfo = action;
            EventType = type;
            Alumnxs = alumnxs;
        }
    }

    [Serializable]
    public class TurnData : BaseData
    {
        public string Speaker;          // "Teacher" o "Student"
        public float TurnDuration;      // Duración del turno en segundos
        public float LatencyFromPrior;  // Wait Time respecto al turno anterior (en segundos)
        public bool WasInterruption;    // ¿Interrumpió al otro hablante?

        public TurnData(string speaker, float duration, float latency, bool wasInterruption)
        {
            Speaker = speaker;
            TurnDuration = duration;
            LatencyFromPrior = latency;
            WasInterruption = wasInterruption;
        }
    }

    [Serializable]
    public class GameData
    {
        public Dictionary<string, List<BaseData>> datas;
        public string Session;
        public GameData()
        {
            datas = new Dictionary<string, List<BaseData>>();
        }


        public string ToJson()
        {
            return JsonConvert.SerializeObject(datas, Formatting.Indented);
        }
    }
}

