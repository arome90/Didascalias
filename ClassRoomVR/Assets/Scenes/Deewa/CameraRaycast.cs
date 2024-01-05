using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    Camera cam;
    public LayerMask mask;
    public Vector3 si;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePose = Input.mousePosition;
        mousePose.z = 100f;
        mousePose = cam.ScreenToWorldPoint(mousePose);
        Debug.DrawRay(transform.position, mousePose - transform.position, Color.red);

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100, mask))
            {
                Debug.Log(hit.transform.name);
                hit.transform.GetComponent<Renderer>().material.color = Color.red;
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(si, 100);
            }
        }
    }
}
