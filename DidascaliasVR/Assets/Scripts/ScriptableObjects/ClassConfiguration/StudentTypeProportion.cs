using System;
using UnityEngine;

[CreateAssetMenu(fileName = "StudentTypeProportion", menuName ="ScriptableObjects/StudentTypeProportion")]
public class StudentTypeProportion : ScriptableObject
{
    public int NonParticipative =   90;
    public int Participative =      90;
    public int Talkative =          90;
    public int Problematic =        90;
}