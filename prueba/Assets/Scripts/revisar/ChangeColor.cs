using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Changes the color of the SkinnedMeshRenderer component at runtime.
/// </summary>
public class ChangeColor : MonoBehaviour
{
    [SerializeField] private List<Color> _colors;

    private SkinnedMeshRenderer _skinnedMeshRenderer;
    private Material _material;

    private void Awake()
    {
        // Cache the SkinnedMeshRenderer and its material
        _skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();
        if (_skinnedMeshRenderer == null)
        {
            Debug.LogError("SkinnedMeshRenderer component is missing.");
            return;
        }
        _material = _skinnedMeshRenderer.material;
        if (_material == null)
        {
            Debug.LogError("Material is missing on the SkinnedMeshRenderer.");
        }
    }

    private void Start()
    {
        ApplyRandomColor();
    }

    private void Update()
    {
        if (_material != null)
        {
            ApplyRandomColor();
        }
    }

    /// <summary>
    /// Applies a random color from the list to the material.
    /// </summary>
    private void ApplyRandomColor()
    {
        if (_colors.Count == 0) return;

        int randomIndex = Random.Range(0, _colors.Count);
        _material.SetColor("_Color", _colors[randomIndex]);
    }
}
