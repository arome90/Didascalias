using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using ClassRoomVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReconnectUI : MonoBehaviour
{

    [SerializeField] Image loadingBar;
    [SerializeField] float fillSpeed = 0.5f;

    private void Start()
    {
        GameManager.Instance.SetLoadingBar(this);
        gameObject.SetActive(false);
    }
    private void Update()
    {
        //// Simula un progreso de carga
        //loadingBar.fillAmount += fillSpeed * Time.deltaTime;

        //// Restringe el valor de fillAmount entre 0 y 1
        //loadingBar.fillAmount = Mathf.Clamp01(loadingBar.fillAmount);

        //// Verifica si el fillAmount ha llegado a uno y reinicia si es necesario
        //if (loadingBar.fillAmount == 1f)
        //{
        //    // Reinicia el fillAmount a cero
        //    loadingBar.fillAmount = 0f;
        //}
        loadingBar.transform.Rotate(new Vector3(0, 0, -fillSpeed) * Time.deltaTime);
        //loadingBar.transform.eulerAngles = new Vector3(0, 0, Mathf.Clamp(loadingBar.transform.eulerAngles.z, 0f, 360f));

    }
}
