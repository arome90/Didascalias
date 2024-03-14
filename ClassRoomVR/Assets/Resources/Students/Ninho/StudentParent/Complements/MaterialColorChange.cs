using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialColorChange : MonoBehaviour
{
    private Material material;
    public Color[] colors;

    // Start is called before the first frame update
    void Start()
    {
        material= GetComponent<MeshRenderer>().material;
        material.SetColor("_ColorRedMask", colors[Random.Range(0, colors.Length - 1)]);
        material.SetColor("_ColorGreenMask", colors[Random.Range(0, colors.Length - 1)]);
        material.SetColor("_ColorBlueMask", colors[Random.Range(0, colors.Length - 1)]);
    }

    // Update is called once per frame
    void Update()
    {
        material.SetColor("_ColorRedMask", colors[Random.Range(0, colors.Length - 1)]);
        material.SetColor("_ColorGreenMask", colors[Random.Range(0, colors.Length - 1)]);
        material.SetColor("_ColorBlueMask", colors[Random.Range(0, colors.Length - 1)]);
    }
}
