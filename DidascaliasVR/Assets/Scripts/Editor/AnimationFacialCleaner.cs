using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// Herramienta para eliminar curvas de posición de huesos faciales
/// de clips .anim ya importados.
/// </summary>
public static class AnimationFacialCleaner
{
    /// <summary>
    /// Lista de huesos sobre los que queremos actuar.
    /// Solo se eliminarán curvas de estos huesos.
    /// </summary>
    private static readonly HashSet<string> BonesToClean = new()
    {
        "CC_Base_JawRoot",
        "CC_Base_Teeth02",
        "CC_Base_Tongue01",
        "CC_Base_Tongue02",
        "CC_Base_Tongue03",
        "CC_Base_L_Eye",
        "CC_Base_R_Eye"
    };

    /// <summary>
    /// Añade una opción al menú superior de Unity:
    ///
    /// Tools
    ///  └─ CC
    ///      └─ Clean Facial Position Curves
    ///
    /// El script actuará sobre la carpeta seleccionada.
    /// </summary>
    [MenuItem("Tools/CC/Clean Facial Position Curves")]
    public static void CleanSelectedFolder()
    {
        // Obtener el objeto seleccionado en el Project Window
        Object selected = Selection.activeObject;

        // Validar que haya algo seleccionado
        if (selected == null)
        {
            Debug.LogError("Selecciona una carpeta en el Project Window.");
            return;
        }

        // Obtener la ruta de la carpeta seleccionada
        string folderPath = AssetDatabase.GetAssetPath(selected);

        // Buscar todos los AnimationClip dentro de esa carpeta
        string[] guids = AssetDatabase.FindAssets(
            "t:AnimationClip",
            new[] { folderPath }
        );

        int modifiedClips = 0;

        // Recorrer todos los clips encontrados
        foreach (string guid in guids)
        {
            // Convertir GUID a ruta real
            string path = AssetDatabase.GUIDToAssetPath(guid);

            // Cargar el clip
            AnimationClip clip =
                AssetDatabase.LoadAssetAtPath<AnimationClip>(path);

            if (clip == null)
                continue;

            bool modified = false;

            // Obtener todas las curvas animadas del clip
            var bindings = AnimationUtility.GetCurveBindings(clip);

            // Recorrer cada curva
            foreach (var binding in bindings)
            {
                bool targetBone = false;

                // Comprobar si la curva pertenece
                // a uno de los huesos que queremos limpiar
                foreach (string bone in BonesToClean)
                {
                    if (binding.path.EndsWith(bone))
                    {
                        targetBone = true;
                        break;
                    }
                }

                // Si no pertenece a un hueso objetivo,
                // pasamos a la siguiente curva
                if (!targetBone)
                    continue;

                // Comprobar si la curva es de posición local
                //
                // Ejemplos:
                // m_LocalPosition.x
                // m_LocalPosition.y
                // m_LocalPosition.z
                //
                // Queremos eliminarlas para que Unity
                // conserve la posición original del hueso.
                if (binding.propertyName.StartsWith("m_LocalPosition"))
                {
                    // Eliminar la curva
                    AnimationUtility.SetEditorCurve(
                        clip,
                        binding,
                        null
                    );

                    modified = true;

                    Debug.Log(
                        $"Eliminada posición: " +
                        $"{binding.path} -> {binding.propertyName}"
                    );
                }
            }

            // Si se modificó el clip,
            // marcarlo como cambiado
            if (modified)
            {
                EditorUtility.SetDirty(clip);
                modifiedClips++;
            }
        }

        // Guardar todos los cambios realizados
        AssetDatabase.SaveAssets();

        // Refrescar el Project Window
        AssetDatabase.Refresh();

        Debug.Log(
            $"Proceso completado. Clips modificados: {modifiedClips}"
        );
    }
}