using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ClassRoomVR;

public class LostConnectionMenu : MonoBehaviour
{

    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("Seting loading bar txt: " + gameObject);
        GameManager.Instance.SetLoadingTxt(gameObject); // Configura la barra de carga en el GameManager

    }
}
