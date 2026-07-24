using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public enum PlayerResolutionToConflict
{
    Positive,
    Neutral,
    Negative,
    None
}

public class Player : Singleton<Player>
{
    public UnityEvent<PlayerResolutionToConflict> OnPlayerResolution = new UnityEvent<PlayerResolutionToConflict>();

    // placeholder
    bool _hasResolved = false;

    PlayerResolutionToConflict _currentResolution;

    public static void StartListeningForPlayerResolution()
    {
        Instance._hasResolved = false;
        Instance.StartCoroutine(Instance.ListeningForPlayerResolution());
    }

    IEnumerator ListeningForPlayerResolution()
    {
        yield return new WaitUntil(() => _hasResolved);
        OnPlayerResolution.Invoke(_currentResolution);
    }

    public void PositiveResolution()
    {
        _currentResolution = PlayerResolutionToConflict.Positive;
        _hasResolved = true;
    }

    public void NeutralResolution()
    {
        _currentResolution = PlayerResolutionToConflict.Neutral;
        _hasResolved = true;
    }

    public void NegativeResolution()
    {
        _currentResolution = PlayerResolutionToConflict.Negative;
        _hasResolved = true;
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(Player))]
public class PlayerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Player script = (Player)target;

        // 2. Botones para disparar las funciones pasando el parámetro
        if (GUILayout.Button("Positive Resolution"))
        {
            script.PositiveResolution();
        }
        if (GUILayout.Button("Neutral Resolution"))
        {
            script.NeutralResolution();
        }
        if (GUILayout.Button("Negative Resolution"))
        {
            script.NegativeResolution();
        }

        // Dibuja el resto de variables públicas por defecto si las hubiera
        DrawDefaultInspector();
    }
}
#endif