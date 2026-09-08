#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class ExportAnimatorToCSV
{
    private static void Initialize(out StringBuilder csv, out AnimatorController controller)
    {
        csv = null; controller = null;

        // 1. Obtener el GameObject seleccionado en la jerarquía
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null)
        {
            EditorUtility.DisplayDialog("Error", "Por favor, selecciona en la Jerarquía el GameObject que tiene el Animator.", "OK");
            return;
        }

        Animator animator = selectedObj.GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            EditorUtility.DisplayDialog("Error", "El GameObject seleccionado no tiene un componente Animator o no tiene asignado un Animator Controller.", "OK");
            return;
        }

        // 2. Extraer el AnimatorController
        controller = animator.runtimeAnimatorController as AnimatorController;
        if (controller == null)
        {
            AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideController != null)
            {
                controller = overrideController.runtimeAnimatorController as AnimatorController;
            }
        }

        if (controller == null)
        {
            EditorUtility.DisplayDialog("Error", "No se pudo acceder a la estructura interna del AnimatorController.", "OK");
            return;
        }

        csv = new StringBuilder();
        // 3. Encabezado CSV con la nueva columna para las acciones de ExposeToLLM
        csv.AppendLine("Estado Animator;Nombre;Descripción;Behaviours;Acciones Disponibles (ExposeToLLM)");
    }

    private static void SaveFile(StringBuilder csv)
    {
        string path = EditorUtility.SaveFilePanel("Guardar Estados para Excel", "", "Estados_Estudiante.csv", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, csv.ToString(), Encoding.UTF8);
            EditorUtility.DisplayDialog("Éxito", $"Archivo CSV generado correctamente en:\n{path}", "OK");
        }
    }

    [MenuItem("Tools/Exportar Estados de Animator a CSV (Excel)")]
    public static void ExportStates()
    {
        Initialize(out StringBuilder csv, out AnimatorController controller);
        if (csv == null || controller == null) return;

        foreach (var layer in controller.layers)
        {
            ProcessStateMachine(layer.stateMachine, csv, false);
        }

        SaveFile(csv);
    }

    [MenuItem("Tools/Exportar Estados con Contexto a CSV (Excel)")]
    public static void ExportContextualStates()
    {
        Initialize(out StringBuilder csv, out AnimatorController controller);
        if (csv == null || controller == null) return;

        foreach (var layer in controller.layers)
        {
            ProcessStateMachine(layer.stateMachine, csv, true);
        }

        SaveFile(csv);
    }

    private static void ProcessStateMachine(AnimatorStateMachine stateMachine, StringBuilder csv, bool onlyAppendIfHasContext = false)
    {
        // 1. Recorrer los estados directos de esta máquina
        foreach (var childState in stateMachine.states)
        {
            var state = childState.state;
            string animatorStateName = state.name;

            string allBehaviours = string.Join(", ", state.behaviours.Select(b => b.GetType().Name));
            string cleanBehaviours = allBehaviours.Replace(";", " ");
            string cleanDescription = "";
            string cleanName = "";
            string cleanMethods = "";

            foreach (var behaviour in state.behaviours)
            {
                if (behaviour is StudentStateContext context)
                {
                    cleanDescription = context.stateDescription
                        .Replace("\r", "")
                        .Replace("\n", " ")
                        .Replace(";", ",");

                    cleanName = context.stateName.Replace(";", ",");

                    if (context.availableMethods != null && context.availableMethods.Count > 0)
                    {
                        cleanMethods = string.Join("|", context.availableMethods)
                            .Replace("\r", "")
                            .Replace("\n", " ")
                            .Replace(";", ",");
                    }

                    if (onlyAppendIfHasContext)
                    {
                        csv.AppendLine($"{animatorStateName};{cleanName};{cleanDescription};{cleanBehaviours};{cleanMethods}");
                        Debug.Log("Estado con contexto exportado");
                    }
                }
            }

            if (!onlyAppendIfHasContext)
            {
                csv.AppendLine($"{animatorStateName};{cleanName};{cleanDescription};{cleanBehaviours};{cleanMethods}");
                Debug.Log("Estado exportado");
            }
        }

        // 2. Recorrer las Sub-State Machines
        foreach (var childSubMachine in stateMachine.stateMachines)
        {
            var subMachine = childSubMachine.stateMachine;
            string subMachineName = subMachine.name;

            string allBehaviours = string.Join(", ", subMachine.behaviours.Select(b => b.GetType().Name));
            string cleanBehaviours = allBehaviours.Replace(";", " ");
            string cleanDescription = "";
            string cleanName = "";
            string cleanMethods = "";

            foreach (var behaviour in subMachine.behaviours)
            {
                if (behaviour is StudentStateContext context)
                {
                    cleanDescription = context.stateDescription
                        .Replace("\r", "")
                        .Replace("\n", " ")
                        .Replace(";", ",");

                    cleanName = context.stateName.Replace(";", ",");

                    if (context.availableMethods != null && context.availableMethods.Count > 0)
                    {
                        cleanMethods = string.Join("|", context.availableMethods)
                            .Replace("\r", "")
                            .Replace("\n", " ")
                            .Replace(";", ",");
                    }

                    if (onlyAppendIfHasContext)
                    {
                        csv.AppendLine($"{subMachineName};{cleanName};{cleanDescription};{cleanBehaviours};{cleanMethods}");
                        Debug.Log("Estado con contexto exportado");
                    }
                }
            }

            if (!onlyAppendIfHasContext)
            {
                csv.AppendLine($"{subMachineName};{cleanName};{cleanDescription};{cleanBehaviours};{cleanMethods}");
                Debug.Log("Estado exportado");
            }

            ProcessStateMachine(subMachine, csv, onlyAppendIfHasContext);
        }
    }
}

public class ImportAnimatorFromExcel
{
    [MenuItem("Tools/Importar Estados de Animator desde CSV (Excel)")]
    public static void ImportStates()
    {
        GameObject selectedObj = Selection.activeGameObject;
        if (selectedObj == null)
        {
            EditorUtility.DisplayDialog("Error", "Selecciona en la Jerarquía el GameObject que tiene el Animator.", "OK");
            return;
        }

        Animator animator = selectedObj.GetComponent<Animator>();
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            EditorUtility.DisplayDialog("Error", "El GameObject no tiene un componente Animator o AnimatorController válido.", "OK");
            return;
        }

        AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
        if (controller == null)
        {
            AnimatorOverrideController overrideController = animator.runtimeAnimatorController as AnimatorOverrideController;
            if (overrideController != null)
                controller = overrideController.runtimeAnimatorController as AnimatorController;
        }

        if (controller == null)
        {
            EditorUtility.DisplayDialog("Error", "No se pudo acceder a la estructura del AnimatorController.", "OK");
            return;
        }

        string path = EditorUtility.OpenFilePanel("Seleccionar archivo CSV de Estados", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length <= 1)
        {
            EditorUtility.DisplayDialog("Error", "El archivo CSV está vacío o no contiene datos.", "OK");
            return;
        }

        int updatedCount = 0;
        int createdCount = 0;

        Undo.RegisterCompleteObjectUndo(controller, "Importar Contexto Animator desde CSV");

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            string[] parts = line.Split(';');
            if (parts.Length < 3) continue;

            string rawAnimatorStateName = parts[0].Trim();
            string newName = parts[1].Trim();
            string newDescription = parts[2].Trim();

            // Leer la quinta columna con las acciones (si existe)
            string rawMethods = parts.Length >= 5 ? parts[4].Trim() : "";

            string targetName = rawAnimatorStateName.Trim();

            bool wasCreated = false;
            bool found = false;

            foreach (var layer in controller.layers)
            {
                if (ProcessAndApplyToStateMachine(layer.stateMachine, targetName, newName, newDescription, rawMethods, ref wasCreated))
                {
                    found = true;
                    if (wasCreated) createdCount++;
                    else updatedCount++;
                    break;
                }
            }

            if (!found)
            {
                Debug.LogWarning($"[Importador] No se encontró el estado o sub-máquina '{targetName}' en el Animator.");
            }
        }

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Éxito", $"Importación completada:\n- Actualizados: {updatedCount}\n- Creados desde cero: {createdCount}", "OK");
    }

    private static bool ProcessAndApplyToStateMachine(
        AnimatorStateMachine stateMachine,
        string targetName,
        string newName,
        string newDescription,
        string rawMethods,
        ref bool wasCreated)
    {
        foreach (var childState in stateMachine.states)
        {
            var state = childState.state;
            if (state.name == targetName)
            {
                ApplyContextToBehaviours(
                    state.behaviours,
                    newName,
                    newDescription,
                    rawMethods,
                    behaviourToAdd => state.AddStateMachineBehaviour(behaviourToAdd),
                    ref wasCreated
                );
                return true;
            }
        }

        if (stateMachine.name == targetName)
        {
            ApplyContextToBehaviours(
                stateMachine.behaviours,
                newName,
                newDescription,
                rawMethods,
                behaviourToAdd => stateMachine.AddStateMachineBehaviour(behaviourToAdd),
                ref wasCreated
            );
            return true;
        }

        foreach (var childSubMachine in stateMachine.stateMachines)
        {
            var subMachine = childSubMachine.stateMachine;

            if (ProcessAndApplyToStateMachine(subMachine, targetName, newName, newDescription, rawMethods, ref wasCreated))
            {
                return true;
            }
        }

        return false;
    }

    private static void ApplyContextToBehaviours(
        StateMachineBehaviour[] existingBehaviours,
        string newName,
        string newDescription,
        string rawMethods,
        System.Func<System.Type, StateMachineBehaviour> addBehaviourFunc,
        ref bool wasCreated)
    {
        StudentStateContext targetContext = null;

        foreach (var behaviour in existingBehaviours)
        {
            if (behaviour is StudentStateContext context)
            {
                targetContext = context;
                break;
            }
        }

        if (targetContext == null)
        {
            targetContext = (StudentStateContext)addBehaviourFunc(typeof(StudentStateContext));
            wasCreated = true;
        }
        else
        {
            wasCreated = false;
        }

        Undo.RecordObject(targetContext, "Modificar StudentStateContext");
        targetContext.stateName = newName;
        targetContext.stateDescription = newDescription;

        // Deserializar la lista de acciones separadas por '|'
        if (targetContext.availableMethods == null)
            targetContext.availableMethods = new List<string>();
        else
            targetContext.availableMethods.Clear();

        if (!string.IsNullOrEmpty(rawMethods))
        {
            string[] parsedMethods = rawMethods.Split('|');
            foreach (var method in parsedMethods)
            {
                string trimmed = method.Trim();
                if (!string.IsNullOrEmpty(trimmed))
                {
                    targetContext.availableMethods.Add(trimmed);
                }
            }
        }

        EditorUtility.SetDirty(targetContext);
    }
}
#endif