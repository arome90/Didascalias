using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

namespace ClassRoomVR
{
    public class DetectVR : MonoBehaviour
    {
        [SerializeField] GameObject playerVR;
        [SerializeField] GameObject player;

        // If VR glasses can be initialized correctly, choose the player in VR mode
        private void Start()
        {
            if (GameManager.Instance.IsUsingVRHardware())
            {
                var xrSettings = XRGeneralSettings.Instance;
                if (xrSettings == null)
                {
                    Debug.Log("XRGeneralSettings is null");
                    return;
                }
                var xrManager = xrSettings.Manager;
                if (xrManager == null)
                {
                    Debug.Log("XRManagerSettings is null");
                    return;
                }
                var xrLoader = xrManager.activeLoader;
                if (xrLoader == null)
                {
                    Debug.Log("XRLoader is null");
                    SetPlayerVR(false);
                    return;
                }
                Debug.Log("XRLoader is okay");
                SetPlayerVR(true);
            }
            else
            {
                SetPlayerVR(false);
            }
        }

        private void SetPlayerVR(bool vr)
        {
            player.SetActive(!vr);
            playerVR.SetActive(vr);
            GameObject selectedPlayer = vr ? playerVR : player;
            GameManager.Instance.SetPlayer(selectedPlayer.transform.GetChild(0).gameObject);
        }
    }
}
