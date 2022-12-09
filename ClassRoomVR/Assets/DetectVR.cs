using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Management;

namespace ClassRoomVR {
    public class DetectVR : MonoBehaviour
    {
        public GameObject playerVR;
        public GameObject player;
        // Start is called before the first frame update
        void Awake()
        {
            if (GameManager.Instance.getVR())
            {
                var xrset = XRGeneralSettings.Instance;
                if (xrset == null)
                {
                    Debug.Log("XRGeneralSettings is null");
                    return;
                }
                var xrman = xrset.Manager;
                if (xrman == null)
                {
                    Debug.Log("XRManagerSettings is null");
                    return;
                }
                var xrloa = xrman.activeLoader;
                if (xrloa == null)
                {
                    Debug.Log("XRLoader is null");
                    SetPlayerVR(false);
                    return;
                }
                Debug.Log("XRLoader is okay");
                SetPlayerVR(true);

            }
            else SetPlayerVR(false);
        }

        private void SetPlayerVR(bool vr)
        {
            player.SetActive(!vr);
            playerVR.SetActive(vr);
            GameObject pl = vr ? playerVR : player;
            GameManager.Instance.SetPlayer(pl.transform.GetChild(0).gameObject);
        }

    }
}
