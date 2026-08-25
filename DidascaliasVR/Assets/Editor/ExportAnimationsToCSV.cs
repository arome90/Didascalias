#if UNITY_EDITOR
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

        csv = new StringBuilder();
        // 3. Crear el contenido CSV con encabezado
        // Usamos ';' como separador por defecto para Excel en español
        csv.AppendLine("Estado Animator;Nombre;Descripción;Behaviours");

    }

    private static void SaveFile(StringBuilder csv)
    {
        // 4. Ventana para guardar el archivo
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

        // Recorrer todas las capas del Animator
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

        // Recorrer todas las capas del Animator
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

                    if (onlyAppendIfHasContext)
                    {
                        csv.AppendLine($"{animatorStateName};{cleanName};{cleanDescription};{cleanBehaviours}");
                        Debug.Log("Estado con contexto exportado");
                    }
                }
            }
            // Fila del CSV para el Estado
            if (!onlyAppendIfHasContext)
            {
                csv.AppendLine($"{animatorStateName};{cleanName};{cleanDescription};{cleanBehaviours}");
                Debug.Log("Estado exportado");
            }

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

                    if (onlyAppendIfHasContext)
                    {
                        csv.AppendLine($"{subMachineName};{cleanName};{cleanDescription};{cleanBehaviours}");
                        Debug.Log("Estado con contexto exportado");
                    }
                }
            }

            // Fila del CSV para el Estado
            if (!onlyAppendIfHasContext)
            {
                csv.AppendLine($"{subMachineName};{cleanName};{cleanDescription};{cleanBehaviours}");
                Debug.Log("Estado exportado");
            }

            // Llamada recursiva para procesar los estados y sub-máquinas que estén DENTRO de esta SubMachine
            ProcessStateMachine(subMachine, csv, onlyAppendIfHasContext);
        }
    }
}

public class ImportAnimatorFromExcel
{
    [MenuItem("Tools/Importar Estados de Animator desde CSV (Excel)")]
    public static void ImportStates()
    {
        // 1. Validar la selección del GameObject
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

        // 2. Seleccionar el archivo CSV
        string path = EditorUtility.OpenFilePanel("Seleccionar archivo CSV de Estados", "", "csv");
        if (string.IsNullOrEmpty(path)) return;

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length <= 1)
        {
            EditorUtility.DisplayDialog("Error", "El archivo CSV está vacío o no contiene datos.", "OK");
            return;
        }

        // 3. Procesar las líneas del CSV (omitimos la cabecera)
        int updatedCount = 0;
        int createdCount = 0;

        // Registrar el AnimatorController en el sistema de deshacer (Undo)
        Undo.RegisterCompleteObjectUndo(controller, "Importar Contexto Animator desde CSV");

        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (string.IsNullOrEmpty(line)) continue;

            // Separar por punto y coma (;)
            string[] parts = line.Split(';');
            if (parts.Length < 3) continue;

            string rawAnimatorStateName = parts[0].Trim();
            string newName = parts[1].Trim();
            string newDescription = parts[2].Trim();

            // Quitar la marca de SubMachine si la tuviera del exportador
            string targetName = rawAnimatorStateName.Trim();

            bool wasCreated = false;
            bool found = false;

            // Recorrer capas para buscar y modificar el estado/submáquina
            foreach (var layer in controller.layers)
            {
                if (ProcessAndApplyToStateMachine(layer.stateMachine, targetName, newName, newDescription, ref wasCreated))
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

        // Guardar cambios en el asset del AnimatorController
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();

        EditorUtility.DisplayDialog("Éxito", $"Importación completada:\n- Actualizados: {updatedCount}\n- Creados desde cero: {createdCount}", "OK");
    }

    private static bool ProcessAndApplyToStateMachine(
        AnimatorStateMachine stateMachine,
        string targetName,
        string newName,
        string newDescription,
        ref bool wasCreated)
    {
        // 1. Buscar entre los estados normales de esta máquina
        foreach (var childState in stateMachine.states)
        {
            var state = childState.state;
            if (state.name == targetName)
            {
                ApplyContextToBehaviours(
                    state.behaviours,
                    newName,
                    newDescription,
                    behaviourToAdd => state.AddStateMachineBehaviour(behaviourToAdd),
                    ref wasCreated
                );
                return true;
            }
        }

        // 2. Comprobar si el nombre coincide con la propia Sub-State Machine actual
        if (stateMachine.name == targetName)
        {
            ApplyContextToBehaviours(
                stateMachine.behaviours,
                newName,
                newDescription,
                behaviourToAdd => stateMachine.AddStateMachineBehaviour(behaviourToAdd),
                ref wasCreated
            );
            return true;
        }

        // 3. Recorrer las submáquinas hijo (recursividad para descendientes)
        foreach (var childSubMachine in stateMachine.stateMachines)
        {
            var subMachine = childSubMachine.stateMachine;

            // Llamada recursiva para buscar tanto dentro de la submáquina como en ella misma
            if (ProcessAndApplyToStateMachine(subMachine, targetName, newName, newDescription, ref wasCreated))
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
        System.Func<System.Type, StateMachineBehaviour> addBehaviourFunc,
        ref bool wasCreated)
    {
        StudentStateContext targetContext = null;

        // 1. Buscar si ya existe el componente
        foreach (var behaviour in existingBehaviours)
        {
            if (behaviour is StudentStateContext context)
            {
                targetContext = context;
                break;
            }
        }

        // 2. Si no existe, crearlo dinámicamente
        if (targetContext == null)
        {
            targetContext = (StudentStateContext)addBehaviourFunc(typeof(StudentStateContext));
            wasCreated = true;
        }
        else
        {
            wasCreated = false;
        }

        // 3. Modificar los valores e indicar cambios
        Undo.RecordObject(targetContext, "Modificar StudentStateContext");
        targetContext.stateName = newName;
        targetContext.stateDescription = newDescription;
        EditorUtility.SetDirty(targetContext);
    }
}
#endif
