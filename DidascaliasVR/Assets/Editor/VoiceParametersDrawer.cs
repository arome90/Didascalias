using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(AzureTTSVoiceConfig.VoiceParameters))]
public class VoiceParametersDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        // Obtener propiedades serializadas
        SerializedProperty nameProp = property.FindPropertyRelative("name");
        SerializedProperty minPitchProp = property.FindPropertyRelative("minPitch");
        SerializedProperty maxPitchProp = property.FindPropertyRelative("maxPitch");
        SerializedProperty minRateProp = property.FindPropertyRelative("minRate");
        SerializedProperty maxRateProp = property.FindPropertyRelative("maxRate");

        // --- VERIFICACIÓN Y AUTO-INICIALIZACIÓN ---
        // Si min y max son iguales (ej. 0 y 0), asignamos un rango inicial válido por defecto
        if (Mathf.Approximately(minPitchProp.floatValue, maxPitchProp.floatValue))
        {
            minPitchProp.floatValue = -15f;
            maxPitchProp.floatValue = 15f;
        }

        if (Mathf.Approximately(minRateProp.floatValue, maxRateProp.floatValue))
        {
            minRateProp.floatValue = -15f;
            maxRateProp.floatValue = 15f;
        }

        // Dibujar foldout / título de la propiedad
        position.height = EditorGUIUtility.singleLineHeight;
        property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, label, true);

        if (property.isExpanded)
        {
            EditorGUI.indentLevel++;
            Rect currentRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

            // 1. Campo para el Nombre de la Voz
            EditorGUI.PropertyField(currentRect, nameProp);
            currentRect.y += EditorGUIUtility.singleLineHeight + 2;

            // 2. Slider Min/Max para Pitch (-30% a +30%)
            float minPitch = minPitchProp.floatValue;
            float maxPitch = maxPitchProp.floatValue;
            EditorGUI.MinMaxSlider(currentRect, new GUIContent($"Pitch: [{minPitch:F0}% a {maxPitch:F0}%]"), ref minPitch, ref maxPitch, -30f, 30f);
            minPitchProp.floatValue = minPitch;
            maxPitchProp.floatValue = maxPitch;

            currentRect.y += EditorGUIUtility.singleLineHeight + 2;

            // 3. Slider Min/Max para Rate (-30% a +30%)
            float minRate = minRateProp.floatValue;
            float maxRate = maxRateProp.floatValue;
            EditorGUI.MinMaxSlider(currentRect, new GUIContent($"Rate: [{minRate:F0}% a {maxRate:F0}%]"), ref minRate, ref maxRate, -30f, 30f);
            minRateProp.floatValue = minRate;
            maxRateProp.floatValue = maxRate;

            EditorGUI.indentLevel--;
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (!property.isExpanded)
            return EditorGUIUtility.singleLineHeight;

        // Título + 3 campos (Name, Pitch Slider, Rate Slider) + espaciados
        return (EditorGUIUtility.singleLineHeight * 4) + 6;
    }
}