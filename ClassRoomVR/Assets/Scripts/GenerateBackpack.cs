using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateBackpack : MonoBehaviour
{
    [System.Serializable]
    struct BackPack
    {
        public Vector3 position;
        public Vector3 rotation;
    }
    [SerializeField] List<BackPack> backpackPosition;
    // Start is called before the first frame update
    void Start()
    {
        int i = Random.Range(0, backpackPosition.Count);
        transform.localPosition = backpackPosition[i].position;
        transform.rotation = Quaternion.Euler(backpackPosition[i].rotation);

        int j = Random.Range(0, transform.childCount);
        i = 0;
        foreach (Transform child in transform)
        {
            if (j != i) { Destroy(child.gameObject); }
            else child.gameObject.SetActive(true);
            i++;
        }


    }

}
