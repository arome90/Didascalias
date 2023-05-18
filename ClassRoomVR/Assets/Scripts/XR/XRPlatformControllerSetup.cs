using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.XR.Management;
#else
using UnityEngine.XR.Management;
#endif

namespace Unity.Template.VR
{
    // Script to control the initialization of VR headsets
    internal class XRPlatformControllerSetup : MonoBehaviour
    {
        [SerializeField] private GameObject leftController;
        [SerializeField] private GameObject rightController;
        [SerializeField] private GameObject leftControllerOculusPackage;
        [SerializeField] private GameObject rightControllerOculusPackage;

        private void Start()
        {
#if UNITY_EDITOR
            var loaders = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone).Manager.activeLoaders;
#else
            var loaders = XRGeneralSettings.Instance.Manager.activeLoaders;
#endif

            foreach (var loader in loaders)
            {
                if (loader.name.Equals("Oculus Loader"))
                {
                    rightController.SetActive(false);
                    leftController.SetActive(false);
                    rightControllerOculusPackage.SetActive(true);
                    leftControllerOculusPackage.SetActive(true);
                }
            }
        }
    }
}
