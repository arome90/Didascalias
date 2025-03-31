using System;

public enum OriginInfo2
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

public enum Gender2
{
    /// <summary>
    /// Género femenino.
    /// </summary>
    Women,

    /// <summary>
    /// Género masculino.
    /// </summary>
    Men
}

public enum State2
{
    /// <summary>
    /// Estado sentado.
    /// </summary>
    Sitting,

    /// <summary>
    /// Estado de pie.
    /// </summary>
    Standing
}

public enum TalkMode2
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

public enum Allign2
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

public enum GenerateMode2
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

public enum Age2
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

public enum StructureMode2
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
public enum FieldOfVision2
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
public enum HandSelector2
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
public enum HeadSelector2
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

public enum Animaciones2
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

public enum AnimacionesStudent2
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

public enum VisualAction2
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
public enum Actions2
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
    Levantarse = 1 << 2  // 4
}

public enum Positions2
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

public enum Expresiones2
{
    /// <summary>
    /// Expresión de llorar.
    /// </summary>
    Llorar,

    /// <summary>
    /// Expresión de estar dormido.
    /// </summary>
    Dormido,

    /// <summary>
    /// Expresión de sonreír.
    /// </summary>
    Sonreir,

    /// <summary>
    /// Expresión de quejarse.
    /// </summary>
    Quejarse,

    /// <summary>
    /// Expresión de estar enfadado.
    /// </summary>
    Enfadado,

    /// <summary>
    /// Expresión de parpadear.
    /// </summary>
    Pestañear
}
