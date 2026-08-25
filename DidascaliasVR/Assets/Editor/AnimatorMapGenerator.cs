#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class AnimatorMapGenerator : EditorWindow
{
    private RuntimeAnimatorController _animatorController;
    private AnimatorStateMap _outputAsset;

    [MenuItem("Tools/Generar Grafo Filtrado por Contexto")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorMapGenerator>("Animator Path Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Generador de Grafo Filtrado (IsReachable)", EditorStyles.boldLabel);

        _animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField(
            "Animator Controller", _animatorController, typeof(RuntimeAnimatorController), false);

        _outputAsset = (AnimatorStateMap)EditorGUILayout.ObjectField(
            "ScriptableObject Destino", _outputAsset, typeof(AnimatorStateMap), false);

        if (GUILayout.Button("Generar Mapeo Filtrado"))
        {
            GenerarMapeoFiltrado();
        }
    }

    private void GenerarMapeoFiltrado()
    {
        if (_animatorController == null || _outputAsset == null)
        {
            Debug.LogError("[MapGenerator] Asigna el Controller y el ScriptableObject.");
            return;
        }

        AnimatorController controller = _animatorController as AnimatorController;
        if (controller == null) return;

        _outputAsset.graph.Clear();

        // 1. Extraer estados que tengan StudentStateContext válido con IsReachable = true
        foreach (var layer in controller.layers)
        {
            ProcesarStateMachineRecursivo(layer.stateMachine);
        }

        // 2. Calcular las rutas navegables entre los estados válidos registrados
        CalcularCaminosCompletos(_outputAsset);

        EditorUtility.SetDirty(_outputAsset);
        AssetDatabase.SaveAssets();
        Debug.Log($"[MapGenerator] Mapeo completado. {_outputAsset.graph.Count} estados alcanzables mapeados.");
    }

    private void ProcesarStateMachineRecursivo(AnimatorStateMachine stateMachine)
    {
        foreach (ChildAnimatorState childState in stateMachine.states)
        {
            AnimatorState state = childState.state;

            // Comprobar si tiene el componente StudentStateContext con IsReachable == true
            if (!EsEstadoAlcanzable(state))
            {
                continue; // Saltar estados no expuestos a la IA
            }

            var stateData = new AnimatorStateMap.StateData { stateName = state.name };

            foreach (AnimatorStateTransition transition in state.transitions)
            {
                // Resolver el destino final (navegando si hay estados intermedios no alcanzables)
                string targetStateName = ResolverEstadoDestinoValido(transition);

                if (!string.IsNullOrEmpty(targetStateName) && targetStateName != state.name)
                {
                    var transData = new AnimatorStateMap.TransitionData { targetState = targetStateName };

                    foreach (AnimatorCondition cond in transition.conditions)
                    {
                        transData.conditions.Add(new AnimatorStateMap.ConditionData
                        {
                            parameterName = cond.parameter,
                            mode = cond.mode.ToString(),
                            threshold = cond.threshold
                        });
                    }

                    stateData.directTransitions.Add(transData);
                }
            }

            _outputAsset.graph.Add(stateData);
        }

        foreach (ChildAnimatorStateMachine subMachine in stateMachine.stateMachines)
        {
            ProcesarStateMachineRecursivo(subMachine.stateMachine);
        }
    }

    // Verifica si un AnimatorState tiene el Behaviour configurado como alcanzable
    private static bool EsEstadoAlcanzable(AnimatorState state)
    {
        if (state == null) return false;

        foreach (var behaviour in state.behaviours)
        {
            if (behaviour is StudentStateContext context && context.IsReachable)
            {
                return true;
            }
        }
        return false;
    }

    // Resuelve el siguiente destino navegando a través de animaciones intermedias sin Contexto
    private string ResolverEstadoDestinoValido(AnimatorStateTransition transition, HashSet<AnimatorState> visited = null)
    {
        visited ??= new HashSet<AnimatorState>();

        AnimatorState targetState = transition.destinationState;

        if (targetState == null && transition.destinationStateMachine != null)
        {
            targetState = transition.destinationStateMachine.defaultState;
        }

        if (targetState == null || visited.Contains(targetState)) return null;
        visited.Add(targetState);

        // Si el estado destino es alcanzable por la IA, devolvemos su nombre directamente
        if (EsEstadoAlcanzable(targetState))
        {
            return targetState.name;
        }

        // Si el estado no es alcanzable, seguimos sus transiciones en cadena hasta encontrar uno que sí lo sea
        foreach (AnimatorStateTransition nextTrans in targetState.transitions)
        {
            string resolved = ResolverEstadoDestinoValido(nextTrans, visited);
            if (!string.IsNullOrEmpty(resolved))
            {
                return resolved;
            }
        }

        return null;
    }

    private static void CalcularCaminosCompletos(AnimatorStateMap mapAsset)
    {
        var rawDict = mapAsset.ToDictionary();

        foreach (var stateData in mapAsset.graph)
        {
            stateData.reachablePaths.Clear();

            var queue = new Queue<(string currentState, List<string> path, AnimatorStateMap.TransitionData firstStep)>();
            var visited = new HashSet<string> { stateData.stateName };

            if (rawDict.TryGetValue(stateData.stateName, out var directTrans))
            {
                foreach (var trans in directTrans)
                {
                    if (string.IsNullOrEmpty(trans.targetState)) continue;

                    var initialPath = new List<string> { stateData.stateName, trans.targetState };
                    queue.Enqueue((trans.targetState, initialPath, trans));
                    visited.Add(trans.targetState);
                }
            }

            while (queue.Count > 0)
            {
                var (curr, path, firstStep) = queue.Dequeue();

                stateData.reachablePaths.Add(new AnimatorStateMap.ReachablePath
                {
                    targetState = curr,
                    totalSteps = path.Count - 1,
                    nextImmediateState = firstStep.targetState,
                    immediateConditions = firstStep.conditions,
                    fullStatePath = new List<string>(path)
                });

                if (rawDict.TryGetValue(curr, out var nextTransitions))
                {
                    foreach (var nextTrans in nextTransitions)
                    {
                        if (!string.IsNullOrEmpty(nextTrans.targetState) && !visited.Contains(nextTrans.targetState))
                        {
                            visited.Add(nextTrans.targetState);
                            var newPath = new List<string>(path) { nextTrans.targetState };
                            queue.Enqueue((nextTrans.targetState, newPath, firstStep));
                        }
                    }
                }
            }
        }
    }
}
#endif