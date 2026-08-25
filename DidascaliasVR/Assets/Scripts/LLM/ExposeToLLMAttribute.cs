using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

// Con AllowMultiple = false evitamos duplicados y solo permitimos usarlo en métodos
[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ExposeToLLMAttribute : Attribute
{
    public string Description { get; }

    // Permite añadir opcionalmente una descripción a la función
    public ExposeToLLMAttribute(string description = "")
    {
        Description = description;
    }
}

public static class LLMActionExporter
{
    /// <summary>
    /// Genera la lista de funciones con sus descripciones y parámetros formateados para el LLM
    /// </summary>
    public static string ExportMethodsForPrompt(List<string> methodNames)
    {
        if (methodNames == null || methodNames.Count == 0)
            return "Ninguna";

        StringBuilder sb = new StringBuilder();
        Type behaviourType = typeof(StudentBehaviour);

        foreach (string methodName in methodNames)
        {
            MethodInfo method = behaviourType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null) continue;

            var attr = method.GetCustomAttribute<ExposeToLLMAttribute>();
            string description = attr != null ? attr.Description : "Sin descripción";

            // Extraer los parámetros del método vía Reflexión
            ParameterInfo[] parameters = method.GetParameters();

            sb.Append($"- {methodName}(");

            List<string> paramStrings = new List<string>();
            foreach (var p in parameters)
            {
                // Formato: nombreParametro: tipo (ej: isAngry: bool)
                string typeName = GetFriendlyTypeName(p.ParameterType);
                paramStrings.Add($"{p.Name}: {typeName}");
            }

            sb.Append(string.Join(", ", paramStrings));
            sb.AppendLine($"): {description}");
        }

        return sb.ToString();
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