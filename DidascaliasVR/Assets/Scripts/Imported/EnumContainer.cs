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