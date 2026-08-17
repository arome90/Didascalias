#if UNITY_EDITOR
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class ExportAnimatorToCSV
{
    [MenuItem("Tools/Exportar Estados de Animator a CSV (Excel)")]
    public static void ExportStates()
    {
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
        AnimatorController controller = animator.runtimeAnimatorController as AnimatorController;
        if (controller == null)
        {
            // Soporte para AnimatorOverrideController si se usara
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

        // 3. Crear el contenido CSV con encabezado
        StringBuilder csv = new StringBuilder();
        // Usamos ';' como separador por defecto para Excel en español
        csv.AppendLine("Estado Animator;Nombre;Descripción;Behaviours");

        // Recorrer todas las capas del Animator
        foreach (var layer in controller.layers)
        {
            ProcessStateMachine(layer.stateMachine, csv);
        }

        // 4. Ventana para guardar el archivo
        string path = EditorUtility.SaveFilePanel("Guardar Estados para Excel", "", "Estados_Estudiante.csv", "csv");
        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, csv.ToString(), Encoding.UTF8);
            EditorUtility.DisplayDialog("Éxito", $"Archivo CSV generado correctamente en:\n{path}", "OK");
        }
    }

    private static void ProcessStateMachine(AnimatorStateMachine stateMachine, StringBuilder csv)
    {
        // 1. Recorrer los estados directos de esta máquina
        foreach (var childState in stateMachine.states)
        {
            var state = childState.state;
            string animatorStateName = state.name;

            // 1. Obtener la lista con todos los nombres de los Behaviours adjuntos a este estado
            string allBehaviours = string.Join(", ", state.behaviours.Select(b => b.GetType().Name));

            // Opcional: Limpiar caracteres que puedan romper la estructura del CSV (como comas o punto y coma)
            string cleanBehaviours = allBehaviours.Replace(";", " ");
            string cleanDescription = "";
            string cleanName = "";

            // Buscar si el estado tiene adjunto nuestro script 'StudentStateContext'
            foreach (var behaviour in state.behaviours)
            {
                if (behaviour is StudentStateContext context)
                {
                    cleanDescription = context.stateDescription
                        .Replace("\r", "")
                        .Replace("\n", " ")
                        .Replace(";", ",");

                    cleanName = context.stateName.Replace(";", ",");

                }
            }
            // Fila del CSV para el Estado
            csv.AppendLine($"{animatorStateName};{cleanName};{cleanDescription};{cleanBehaviours}");

        }

        // 2. Recorrer las Sub-State Machines
        foreach (var childSubMachine in stateMachine.stateMachines)
        {
            var subMachine = childSubMachine.stateMachine;
            string subMachineName = subMachine.name;

            // 1. Obtener la lista con todos los nombres de los Behaviours adjuntos a este estado
            string allBehaviours = string.Join(", ", subMachine.behaviours.Select(b => b.GetType().Name));

            // Opcional: Limpiar caracteres que puedan romper la estructura del CSV (como comas o punto y coma)
            string cleanBehaviours = allBehaviours.Replace(";", " ");
            string cleanDescription = "";
            string cleanName = "";

            // Buscar si el estado tiene adjunto nuestro script 'StudentStateContext'
            foreach (var behaviour in subMachine.behaviours)
            {
                if (behaviour is StudentStateContext context)
                {
                    cleanDescription = context.stateDescription
                        .Replace("\r", "")
                        .Replace("\n", " ")
                        .Replace(";", ",");

                    cleanName = context.stateName.Replace(";", ",");

                }
            }
            csv.AppendLine($"{subMachineName};{cleanName};{cleanDescription};{cleanBehaviours}");

            // Llamada recursiva para procesar los estados y sub-máquinas que estén DENTRO de esta SubMachine
            ProcessStateMachine(subMachine, csv);
        }
    }
}
#endif
