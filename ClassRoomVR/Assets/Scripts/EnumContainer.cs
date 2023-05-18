public enum OriginInfo { Random, HispanicLatino, AsianPacificIslander, BlackAfricanAmerican, WhiteCaucasian, Other }
public enum BodyInfo { Random, Body1, Body2, Body3, Body4 }
public enum GenderInfo { Random, Women, Men }
public enum Gender { Women, Men };
public enum State { Sitting, Standing };

public enum TalkMode { None, Disrespect, Good };
public enum Allign { None, Centro, Frente };
public enum GenerateMode { Random, Personalizado, Gender }
public enum Age { Primero, Segundo, Tercero }
public enum StructureMode { Fila, U, Circular, UnPasillo, DosPasillos }
[System.Flags]
public enum FieldOfVision
{
    Up = 1, Right = 2,
    Down = 4, Left = 8, Window = 16, Door = 32, Teacher = 64
}