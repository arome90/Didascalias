using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "AnimatorStateMap", menuName = "AI/Animator State Map")]
public class AnimatorStateMap : ScriptableObject
{
    [Serializable]
    public class ConditionData
    {
        public string parameterName;
        public string mode; // Ej: "If", "IfNot", "Greater", "Less"
        public float threshold;
    }

    [Serializable]
    public class TransitionData
    {
        public string targetState;
        public List<ConditionData> conditions = new List<ConditionData>();
    }

    [Serializable]
    public class ReachablePath
    {
        public string targetState;              // Estado final alcanzable
        public int totalSteps;                 // Cantidad de transiciones interlineales
        public string nextImmediateState;       // Siguiente paso inmediato para llegar
        public List<ConditionData> immediateConditions; // Parámetros para dar el PRIMER paso
        public List<string> fullStatePath;      // Secuencia completa de estados [Origen, ..., Destino]
    }

    [Serializable]
    public class StateData
    {
        public string stateName;
        public List<TransitionData> directTransitions = new List<TransitionData>();
        public List<ReachablePath> reachablePaths = new List<ReachablePath>(); // Rutas a TODOS los estados alcanzables
    }

    public List<StateData> graph = new List<StateData>();

    public Dictionary<string, List<TransitionData>> ToDictionary()
    {
        var dict = new Dictionary<string, List<TransitionData>>();
        foreach (var item in graph)
        {
            if (!dict.ContainsKey(item.stateName))
            {
                dict.Add(item.stateName, item.directTransitions);
            }
        }
        return dict;
    }
}