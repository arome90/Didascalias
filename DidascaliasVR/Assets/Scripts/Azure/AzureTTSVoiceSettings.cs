using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AzureTTSVoiceConfig", menuName = "ScriptableObjects/AzureTTSVoiceConfig")]
public class AzureTTSVoiceConfig : ScriptableObject
{
    [Serializable]
    public struct VoiceParameters 
    {
        public string name;

        public float minPitch;
        public float maxPitch;
        public float minRate;
        public float maxRate;

        public float finalPitch;
        public float finalRate;
    }

    [SerializeField]
    private string _languagePrefix;

    [SerializeField] 
    private VoiceParameters[] girlVoices;

    [SerializeField]
    private VoiceParameters[] boyVoices;

    public VoiceParameters GetVoice(Gender gender)
    {
        VoiceParameters parameters = (gender == Gender.Girl ?
            girlVoices.Next() :
            boyVoices.Next());

        parameters.finalPitch = UnityEngine.Random.Range(parameters.minPitch, parameters.maxPitch);
        parameters.finalRate = UnityEngine.Random.Range(parameters.minRate, parameters.maxRate);

        parameters.name = _languagePrefix + "-" + parameters.name;
        return parameters;
    }
}