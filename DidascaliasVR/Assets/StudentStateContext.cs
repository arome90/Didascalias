using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class StudentStateContext : StateMachineBehaviour
{
    [SerializeField, Tooltip("Variable que nos dice si el LLM puede viajar a este estado")]
    private bool _isReachable = true;

    public bool IsReachable => _isReachable;

    [Header("Información del Estado para el LLM")]
    public string stateName;
    [TextArea(2, 5)]
    public string stateDescription;

    [HideInInspector]
    public List<string> availableMethods = new List<string>();

    // Se ejecuta automáticamente al entrar en esta animación
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Buscamos el gestor de contexto en el GameObject que tiene el Animator
        var contextManager = animator.GetComponentInParent<Student>();

        if (contextManager != null)
        {
            contextManager.SetStateContext(stateName, stateDescription, availableMethods);
        }
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(StudentStateContext))]
public class StudentStateContextEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();

        StudentStateContext script = (StudentStateContext)target;

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Acciones Disponibles para el LLM", EditorStyles.boldLabel);

        // Extraer los métodos de StudentBehaviour marcados con [ExposeToLLM]
        Type behaviourType = typeof(StudentBehaviour);
        var markedMethods = behaviourType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .Select(m => new {
                Method = m,
                Attr = m.GetCustomAttribute<ExposeToLLMAttribute>()
            })
            .Where(x => x.Attr != null)
            .ToList();

        if (script.availableMethods == null)
            script.availableMethods = new List<string>();

        // Construir el conjunto de etiquetas válidas (Nombre + Argumentos + Descripción)
        HashSet<string> allowedLabels = new HashSet<string>();

        foreach (var item in markedMethods)
        {
            string methodName = item.Method.Name;
            string descriptionText = !string.IsNullOrEmpty(item.Attr.Description) ? $" - {item.Attr.Description}" : "";

            string fullLabel = $"{methodName}{descriptionText}";
            allowedLabels.Add(fullLabel);
        }

        // LIMPIEZA AUTOMÁTICA: Eliminar cualquier valor en availableMethods que no coincida con las etiquetas permitidas
        int removedCount = script.availableMethods.RemoveAll(savedItem => !allowedLabels.Contains(savedItem));
        if (removedCount > 0)
        {
            Undo.RecordObject(script, "Limpiar acciones no permitidas u obsoletas");
            EditorUtility.SetDirty(script);
        }

        if (markedMethods.Count == 0)
        {
            EditorGUILayout.HelpBox("No se encontraron métodos marcados con [ExposeToLLM] en StudentBehaviour.", MessageType.Info);
        }
        else
        {
            foreach (var item in markedMethods)
            {
                string methodName = item.Method.Name;
                string descriptionText = !string.IsNullOrEmpty(item.Attr.Description) ? $" - {item.Attr.Description}" : "";

                string fullLabel = $"{methodName}{descriptionText}";

                bool isSelected = script.availableMethods.Contains(fullLabel);
                bool toggleResult = EditorGUILayout.ToggleLeft(fullLabel, isSelected);

                if (toggleResult && !isSelected)
                {
                    Undo.RecordObject(script, "Añadir método a la lista");
                    script.availableMethods.Add(fullLabel);
                    EditorUtility.SetDirty(script);
                }
                else if (!toggleResult && isSelected)
                {
                    Undo.RecordObject(script, "Remover método de la lista");
                    script.availableMethods.Remove(fullLabel);
                    EditorUtility.SetDirty(script);
                }
            }
        }

        serializedObject.ApplyModifiedProperties();
    }

    private static string GetFriendlyTypeName(Type type)
    {
        if (type == typeof(bool)) return "bool";
        if (type == typeof(int)) return "int";
        if (type == typeof(float)) return "float";
        if (type == typeof(string)) return "string";
        return type.Name;
    }
}
#endif