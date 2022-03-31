using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using EmteqLabs.Models;
using TMPro;


namespace EmteqLabs
{

    public class Pulso : MonoBehaviour
    {


       


        // Start is called before the first frame update



        private void Start()
        {
            if (!EmteqVRManager.IsDeviceConnected())
            {
                EmteqVRManager.OnDeviceConnect += OnEmteqDeviceConnectionSuccess;
                EmteqVRManager.OnDeviceDisconnect += OnEmteqDeviceConnectionError;
              //  _statusText.text = ("<color=blue>Connecting to EmteqVR Device</color>");
            }
            else
            {
                OnEmteqDeviceConnectionSuccess();

            }
        }
        private void OnEmteqDeviceConnectionError()
        {
           // _statusText.text = ("<color=red>Could not connect to EmteqVR Device</color>");
        }

        private void OnEmteqDeviceConnectionSuccess()
        {
            //_statusText.text = ("<color=blue>Detecting Heart Rate...</color>");
            EmteqVRManager.OnHeartRateAverageUpdate += OnHeartRateUpdate;
        }
        private void OnHeartRateUpdate(double hr)
        {
            if (hr > 0d)
            {
                // _statusText.text = ("<color=green>Heart Rate Detected...</color>");
                //_currentHRText.text = hr.ToString("F");
            }
                ClassRoomVR.GameManager.Instance._sceneManager.uiManager.ChangeRate(hr);
        }

        //called from button in Unity
        public void CalculateBaseline()
        {
           
            EmteqVRManager.StartHeartRateBaselineCalibration();
        }

        //called from a button in Unity
        public void ShowBaselineResult()
        {
            BaselineHeartRateData baselineHeartRateData = EmteqVRManager.EndHeartRateBaselineCalibration();
          
        }


        private void OnDestroy()
        {
            EmteqVRManager.OnHeartRateAverageUpdate -= OnHeartRateUpdate;

            EmteqVRManager.OnDeviceConnect -= OnEmteqDeviceConnectionSuccess;
            EmteqVRManager.OnDeviceDisconnect -= OnEmteqDeviceConnectionError;
        }
    }

}