using System.Collections;
using System.Collections.Generic;
using System;
public class Emotion
{
    public float Joy { get;  set; }
    public float Sadness { get;  set; }
    public float Fear { get;  set; }
    public float Anger { get;  set; }
    public float Surprise { get;  set; }
    public float Disgust { get;  set; }

    private Random random;

    public Emotion()
    {
        random = new Random();
    }

    public void InitializeEmotions(float max)
    {
        Joy = (float)random.NextDouble() * max;
        Sadness = (float)random.NextDouble() * max;
        Fear = (float)random.NextDouble() * max;
        Anger = (float)random.NextDouble() * max;
        Surprise = (float)random.NextDouble() * max;
        Disgust = (float)random.NextDouble() * max;
    }
}

