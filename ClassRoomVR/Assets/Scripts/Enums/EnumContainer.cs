using System;

public enum OriginInfo { Random, HispanicLatino, AsianPacificIslander, BlackAfricanAmerican, WhiteCaucasian, Other }
public enum GenderInfo { Random, Women, Men }
public enum Gender { Women, Men };
public enum State { Sitting ,Standing };

public enum TalkMode { None, Disrespect, Good, Normal };
public enum Allign { None, Centro, Frente };
public enum GenerateMode { Random, Personalizado, Gender }
public enum Age { Primero, Segundo, Tercero }
public enum StructureMode { Fila, U, Circular }

[System.Flags]
public enum FieldOfVision
{
    Up = 1, Right = 2,
    Down = 4, Left = 8, Window = 16, Door = 32, Teacher = 64
}

[System.Flags]
public enum HandSelector
{
    Posicion = 1, Amplitud = 2,
    DistanciaRecorrida = 4, Velocidad = 8, Aceleracion = 16
}


[System.Flags]
public enum HeadSelector
{
    Posicion = 1,
    DistanciaRecorrida = 2, Velocidad = 4
}

public enum Animaciones
{
   Empujar, SitRelajado, SitSinGanas
}


public enum AnimacionesStudent
{
    Levantar, Empujar, SitGanas, SitSinGanas
}

public enum VisualAction { Menu, Activate, Select, PrimaryButton, SecondaryButton, JoyStick };

[Flags]
public enum Actions
{
    None = 0,
    Insultar = 1 << 0, // 1
    Separados = 1 << 1, // 2
    Levantarse = 1 << 2  // 4
}

// Always update according to the list of positions where students can move
public enum Positions
{
    None = -1,
    FrontSide,
    BackCorner,
    Doors,
    Student
}

