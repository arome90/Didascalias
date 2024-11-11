using System.Collections;
using System.Collections.Generic;
using System;

// Based on the big 5 personality traits model
public class Personality
{

    public float Extraversion { get; private set; }
    public float Agreeableness { get; private set; }
    public float Conscientiousness { get; private set; }
    public float Neuroticism { get; private set; }
    public float Openness { get; private set; }

    private Random random;

    public Personality()
    {
        random = new Random();
        InitializePersonality();
    }

    private void InitializePersonality()
    {
        Extraversion = (float)random.NextDouble();
        Agreeableness = (float)random.NextDouble();
        Conscientiousness = (float)random.NextDouble();
        Neuroticism = (float)random.NextDouble();
        Openness = (float)random.NextDouble();
    }

    public void InfluenceEmotions(Emotion emotion)
    {
        emotion.Joy = Extraversion * 0.9f + Agreeableness * 0.6f + Conscientiousness * 0.6f + (1 - Neuroticism) * 0.8f + Openness * 0.6f;
        emotion.Sadness = Neuroticism * 0.8f + (1 - Extraversion) * 0.3f;
        emotion.Fear = Neuroticism * 0.8f + (1 - Extraversion) * 0.3f;
        emotion.Anger = Neuroticism * 0.7f + (1 - Agreeableness) * 0.3f;
        emotion.Surprise = Extraversion * 0.8f + Openness * 0.8f;
        emotion.Disgust = Neuroticism * 0.7f + (1 - Agreeableness) * 0.3f;
    }


}
