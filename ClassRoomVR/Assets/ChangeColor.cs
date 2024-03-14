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
        gameObject.GetComponent<SkinnedMeshRenderer>().material.color = colors[i];




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
    private SkinnedMeshRenderer skinnedMeshRenderer;
    private void Update()
    {
        int i = Random.Range(0, colors.Count);
        // Accede al componente Skinned Mesh Renderer
        skinnedMeshRenderer = GetComponent<SkinnedMeshRenderer>();

        // Cambia el color del material
        Material material = skinnedMeshRenderer.material;
       // material.color = colors[i];
        material.SetColor("_Color", Color.red);
        // Actualiza el material
        //  skinnedMeshRenderer.SetPropertyBlock(new MaterialPropertyBlock());
       // skinnedMeshRenderer.material.color = colors[i];
    }
}




