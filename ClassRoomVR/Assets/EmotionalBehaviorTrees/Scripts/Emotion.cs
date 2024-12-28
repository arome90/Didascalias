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
    
    public float AnxietyConfidence { get;  set; }
    public float BoredomFascination { get;  set; }
    public float FrustrationEuphoria { get;  set; }
    public float DispiritedEncouraged { get;  set; }
    public float TerrorEnchantment { get;  set; }


    private Random random;

    public Emotion()
    {
        random = new Random();
    }

    public void InitializeEmotions(float max)
    {
        Joy = (float)random.NextDouble() * max; //NextDouble devuelve un valor aleatorio entre 0 y 1
        Sadness = (float)random.NextDouble() * max;
        Fear = (float)random.NextDouble() * max;
        Anger = (float)random.NextDouble() * max;
        Surprise = (float)random.NextDouble() * max;
        Disgust = (float)random.NextDouble() * max;

        AnxietyConfidence = ((float)random.NextDouble() * 2) - 1; //Se genera un numero aleatorio entre -1 y 1
        BoredomFascination = ((float)random.NextDouble() * 2) - 1;
        FrustrationEuphoria = ((float)random.NextDouble() * 2) - 1;
        DispiritedEncouraged = ((float)random.NextDouble() * 2) - 1;
        TerrorEnchantment = ((float)random.NextDouble() * 2) - 1;
    }
}

