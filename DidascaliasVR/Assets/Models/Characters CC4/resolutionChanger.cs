using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR;

public class resolutionChanger : MonoBehaviour
{

    [SerializeField]
    InputActionReference triggerRight;
    [SerializeField]
    InputActionReference triggerLeft;

    [SerializeField]
    TextMeshProUGUI resText;

    [SerializeField]
    TextMeshProUGUI quaText;

    int idRes = 0;
    int idQua = 0;

    // Start is called before the first frame update
    void Start()
    {
        triggerRight.action.started += ButtonPressed;
        triggerLeft.action.started += ButtonQuality;
        QualitySettings.SetQualityLevel(0);
        resText.text = "Resolucion x1";
        quaText.text = "Calidad: Muy Baja";
        UniversalRenderPipelineAsset urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;
        urp.renderScale = 1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            quality();
        }
    }

    void ButtonPressed(InputAction.CallbackContext context)
    {
        if (idRes == 2) idRes = 0;
        else idRes++;

        UniversalRenderPipelineAsset urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;

        switch (idRes)
        {
            case 0:
                urp.renderScale = 1f;
                resText.text = "Resolucion x1";
                break;
            case 1:
                urp.renderScale = 1.5f;
                resText.text = "Resolucion x1.5";
                break;
            case 2:
                urp.renderScale = 2f;
                resText.text = "Resolucion x2";
                break;
        }
    }

    void ButtonQuality(InputAction.CallbackContext context)
    {
        quality();   
    }

    void quality()
    {
        if (idQua == 3) idQua = 0;
        else idQua++;

        switch (idQua)
        {
            case 0:
                QualitySettings.SetQualityLevel(0);
                quaText.text = "Calidad: Muy Baja";
                break;
            case 1:
                QualitySettings.SetQualityLevel(1);
                quaText.text = "Calidad: Baja";
                break;
            case 2:
                QualitySettings.SetQualityLevel(2);
                quaText.text = "Calidad: Alta";
                break;
            case 3:
                QualitySettings.SetQualityLevel(3);
                quaText.text = "Calidad: Ultra";
                break;
        }
    }
}
