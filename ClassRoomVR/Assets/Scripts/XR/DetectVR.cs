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
        private void Awake()
        {
            CheckAndSetPlayerMode();
        }
        private void CheckAndSetPlayerMode()
        {
            bool usingVR = GameManager.Instance.IsUsingVRHardware();

            if (usingVR)
            {
                SetupVRPlayer();
            }
            else
            {
                SetupNonVRPlayer();
            }
        }

        private void SetupVRPlayer()
        {
            var xrSettings = XRGeneralSettings.Instance;
            
            if (xrSettings != null)
            {
                var xrManager = xrSettings.Manager;

                if (xrManager != null)
                {
                    var xrLoader = xrManager.activeLoader;

                    if (xrLoader != null)
                    {
                        Debug.Log("XRLoader is okay");
                        SetPlayerMode(true);
                        return;
                    }
                }
            }

            Debug.Log("Failed to set up VR player");
            SetPlayerMode(false);
        }

        private void SetupNonVRPlayer()
        {
            Debug.Log("Using non-VR mode");
            SetPlayerMode(false);
        }

        private void SetPlayerMode(bool vr)
        {
            GameObject selectedPlayer = vr ? playerVR : player;
            GameManager.Instance.SetPlayer(selectedPlayer.transform.gameObject);
            player.SetActive(!vr);
            playerVR.SetActive(vr);
        }
    }
}
