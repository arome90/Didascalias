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




        // i = 0;
        //Transform rootParent = transform.root.transform;
        //GameObject target;
        //Transform[] bodyBones = null;
        //var skinnedMesh = transform.parent.GetComponentInChildren<SkinnedMeshRenderer>();
        //if (skinnedMesh != null)
        //{
        //    bodyBones = skinnedMesh.bones;
        //}

        //if (bodyBones == null)
        //{
        //    Debug.LogError("Wrong parent body.");
        //    return;
        //}

        //GameObject Attachment;

        //for (i = 0; i < transform.childCount; i++)
        //{
        //    Attachment = transform.GetChild(i).gameObject;
        //    if (Attachment.GetComponent<SkinnedMeshRenderer>() != null)
        //    {
        //        Attachment.GetComponent<SkinnedMeshRenderer>().bones = bodyBones;
        //    }

        //}
    }
}




