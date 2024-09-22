using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudentProperties : MonoBehaviour
{
    //Indice de las expresiones tiene que coincidir con el indice del blendShape correspondiente del personaje
    public enum Expressions
    {
        CloseEyes,
        Sleep,
        Smile,
        Angry,
        Bored,
        Cry,
        EXPRESSIONS_SIZE
    }
    //Indice de las propiedades tiene que coincidir con el indice del blendShape correspondiente del personaje
    public enum ModelingProperties
    {
        EyeBrownsWidth = Expressions.EXPRESSIONS_SIZE,
        EyebrowsThickness,
        EyebrowsRotation,
        EyesCorner,
        EyeHeight,
        EyeLength,
        NoseWidth,
        NoseBridgeHeight,
        NoseRotation,
        MouthSize,
        FaceWidth,
        FaceLength,
        MODELING_PROPERTIES_SIZE
    }
}
