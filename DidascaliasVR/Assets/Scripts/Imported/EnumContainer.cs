using System;

public enum OriginInfo
{
    /// <summary>
    /// Persona de origen hispano o latino.
    /// </summary>
    HispanicLatino,

    /// <summary>
    /// Persona de origen asiático o isleño del Pacífico.
    /// </summary>
    AsianPacificIslander,

    /// <summary>
    /// Persona de origen africano o afroamericano.
    /// </summary>
    BlackAfricanAmerican,

    /// <summary>
    /// Persona de origen europeo o caucásico.
    /// </summary>
    WhiteCaucasian,

    /// <summary>
    /// Otro origen étnico no especificado.
    /// </summary>
    Other
}

public enum State
{
    /// <summary>
    /// Estado sentado.
    /// </summary>
    Sitting=0,

    /// <summary>
    /// Lavantandose
    /// </summary>
    Standing=1,

    /// <summary>
    /// De pie
    /// </summary>
    StandUp = 2
}

public enum TalkModex
{
    /// <summary>
    /// Modo de conversación no especificado.
    /// </summary>
    None,

    /// <summary>
    /// Modo de conversación irrespetuoso.
    /// </summary>
    Disrespect,

    /// <summary>
    /// Modo de conversación bueno.
    /// </summary>
    Good,

    /// <summary>
    /// Modo de conversación normal.
    /// </summary>
    Normal
}

public enum Align
{
    /// <summary>
    /// Alineación no especificada.
    /// </summary>
    None,

    /// <summary>
    /// Alineación centrada.
    /// </summary>
    Center,

    /// <summary>
    /// Alineación al frente.
    /// </summary>
    Front
}

public enum GenerateMode
{
    /// <summary>
    /// Modo de generación aleatorio.
    /// </summary>
    Random,

    /// <summary>
    /// Modo de generación personalizado.
    /// </summary>
    Personalized,

    /// <summary>
    /// Modo de generación basado en género.
    /// </summary>
    Gender
}

public enum Age
{
    /// <summary>
    /// Primer grado.
    /// </summary>
    Primero,

    /// <summary>
    /// Segundo grado.
    /// </summary>
    Segundo,

    /// <summary>
    /// Tercer grado.
    /// </summary>
    Tercero
}

public enum StructureMode
{
    /// <summary>
    /// Estructura en fila.
    /// </summary>
    Fila,

    /// <summary>
    /// Estructura en forma de U.
    /// </summary>
    U,

    /// <summary>
    /// Estructura circular.
    /// </summary>
    Circular
}

[Flags]
public enum FieldOfVision
{
    /// <summary>
    /// Visión hacia arriba.
    /// </summary>
    Up = 1,

    /// <summary>
    /// Visión hacia la derecha.
    /// </summary>
    Right = 2,

    /// <summary>
    /// Visión hacia abajo.
    /// </summary>
    Down = 4,

    /// <summary>
    /// Visión hacia la izquierda.
    /// </summary>
    Left = 8,

    /// <summary>
    /// Visión hacia la ventana.
    /// </summary>
    Window = 16,

    /// <summary>
    /// Visión hacia la puerta.
    /// </summary>
    Door = 32,

    /// <summary>
    /// Visión hacia el profesor.
    /// </summary>
    Teacher = 64
}

[Flags]
public enum HandSelector
{
    /// <summary>
    /// Selección basada en la posición.
    /// </summary>
    Posicion = 1,

    /// <summary>
    /// Selección basada en la amplitud.
    /// </summary>
    Amplitud = 2,

    /// <summary>
    /// Selección basada en la distancia recorrida.
    /// </summary>
    DistanciaRecorrida = 4,

    /// <summary>
    /// Selección basada en la velocidad.
    /// </summary>
    Velocidad = 8,

    /// <summary>
    /// Selección basada en la aceleración.
    /// </summary>
    Aceleracion = 16
}

[Flags]
public enum HeadSelector
{
    /// <summary>
    /// Selección basada en la posición.
    /// </summary>
    Posicion = 1,

    /// <summary>
    /// Selección basada en la distancia recorrida.
    /// </summary>
    DistanciaRecorrida = 2,

    /// <summary>
    /// Selección basada en la velocidad.
    /// </summary>
    Velocidad = 4
}

public enum Animaciones
{
    /// <summary>
    /// Animación de empujar.
    /// </summary>
    Empujar,

    /// <summary>
    /// Animación de sentarse relajado.
    /// </summary>
    SitRelajado,

    /// <summary>
    /// Animación de sentarse sin ganas.
    /// </summary>
    SitSinGanas
}

public enum EventSittingAnimations
{
    None,
    Terrified,
    RiseHand,
    PlayingPhone,
    ConstantMoving,
    Sleeping,
    Attending,
    Attending2,
    Bored,
    Drawing
   
    

}

public enum NormalSittingAnimations
{
    SitHandsOnDesk,
    SitHandsOnThigh

}

public enum SittingTransitionAnimations
{
    /// <summary>
    /// Animación de sentarse relajado.
    /// </summary>
    SitRelajado,

    /// <summary>
    /// Animación de sentarse sin ganas.
    /// </summary>
    SitSinGanas,

    Levantar
}


public enum AnimacionesStudent
{
    /// <summary>
    /// Animación de levantar.
    /// </summary>
    Levantar,

    /// <summary>
    /// Animación de empujar.
    /// </summary>
    Empujar,

    /// <summary>
    /// Animación de sentarse con ganas.
    /// </summary>
    SitGanas,

    /// <summary>
    /// Animación de sentarse sin ganas.
    /// </summary>
    SitSinGanas
}

public enum VisualAction
{
    /// <summary>
    /// Acción del menú.
    /// </summary>
    Menu,

    /// <summary>
    /// Activar acción.
    /// </summary>
    Activate,

    /// <summary>
    /// Acción de seleccionar.
    /// </summary>
    Select,

    /// <summary>
    /// Acción del botón primario.
    /// </summary>
    PrimaryButton,

    /// <summary>
    /// Acción del botón secundario.
    /// </summary>
    SecondaryButton,

    /// <summary>
    /// Acción del joystick.
    /// </summary>
    JoyStick
}

[Flags]
public enum Actions
{
    /// <summary>
    /// Ninguna acción.
    /// </summary>
    None = 0,

    /// <summary>
    /// Acción de insultar.
    /// </summary>
    Insultar = 1 << 0, // 1

    /// <summary>
    /// Acción de separar.
    /// </summary>
    Separados = 1 << 1, // 2

    /// <summary>
    /// Acción de levantarse.
    /// </summary>
    Levantarse = 1 << 2 , // 4

    RiseHand= 1 << 3
}

public enum Positions
{
    /// <summary>
    /// Sin posición especificada.
    /// </summary>
    None = -1,

    /// <summary>
    /// Posición en el frente.
    /// </summary>
    FrontSide,

    /// <summary>
    /// Posición en la esquina trasera.
    /// </summary>
    BackCorner,

    /// <summary>
    /// Posición cerca de las puertas.
    /// </summary>
    Doors,

    /// <summary>
    /// Posición del estudiante.
    /// </summary>
    Student
}
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

public enum ExternalForces
{
    TeacherTalksNormal,
    TeacherSilentTooLong,
    TeacherTooLoud,
    TeacherTooQuiet
}
public enum EmotionType
{
    AnxietyConfidence,
    BoredomFascination ,
    FrustrationEuphoria,
    DispiritedEncouraged,
    TerrorEnchantment
}
public enum PersonalityType
{
    Extraversion,
    Agreeableness,
    Conscientiousness,
    Neuroticism,
    Openness
}

public enum studentBehaviorParams
{
    extraversionInfluence,
    agreeablenessInfluence,
    conscientiousnessInfluence,
    neuroticismInfluence,
    opennessInfluence,
    attentionAddition,
    attentionSubtraction,
    distanceFactorAddition,
    distanceFactorSubtraction,
    climateInfluence,
    range
}

public enum BehaviorInfluences
{
    Priority,
    Extraversion,
    Agreeableness,
    Conscientiousness,
    Neuroticism,
    Openness,
    AnxietyConfidence,
    BoredomFascination,
    FrustrationEuphoria,
    DispiritedEncouraged,
    TerrorEnchantment,
    Attention
}