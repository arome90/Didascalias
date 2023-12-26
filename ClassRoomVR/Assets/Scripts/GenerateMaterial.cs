using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMaterial : MonoBehaviour
{
    [SerializeField] List<GameObject> cases;
    [SerializeField] List<GameObject> books;
    // Start is called before the first frame update
    void Start()
    {
        SelectObject(cases);
        SelectObject(books);
    }

    void SelectObject(List<GameObject>list) 
    {
        int j = Random.Range(0, list.Count);
        int i = 0;
        foreach (GameObject obj in list)
        {
            if (j != i) { Destroy(obj.gameObject); }
            else obj.gameObject.SetActive(true);
            i++;
        }
    }
}
