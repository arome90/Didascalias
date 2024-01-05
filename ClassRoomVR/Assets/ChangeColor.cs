using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeColor : MonoBehaviour
{
    // Start is called before the first frame update

    [SerializeField] List<Color> colors;
    void Start()
    {
        int i = Random.Range(0, colors.Count);
        gameObject.GetComponent<Renderer>().material.color = colors[i];

    }
}
