using System;
using TMPro;
using UnityEngine;

namespace Didascalia.Connection
{
    [RequireComponent(typeof(TMP_Dropdown))]
    internal class DropdownServer : MonoBehaviour
    {
        public enum ServerType
        {
            Dev,
            Prod
        }
        const uint ServerTypeCount = 2;

        [SerializeField]
        private TextMeshProUGUI label;
        private TMP_Dropdown dropdown;
        [SerializeField]
        private string[] serverUrls = new string[ServerTypeCount];

        private void Awake()
        {
            Utils.Error.DebugbreakFailUnless(label != null, "Label reference is not set", this);
            EnsureServerUrls();

            dropdown = GetComponent<TMP_Dropdown>();
            Utils.Error.DebugbreakFailUnless(dropdown != null, "TMP_Dropdown component is missing", this);
            EnsureDropdownValues(dropdown);

            dropdown.onValueChanged.AddListener(OnValueChanged);
        }
        private void OnDestroy()
        {
            dropdown.onValueChanged.RemoveListener(OnValueChanged);
        }
        private void EnsureDropdownValues(TMP_Dropdown dropdown)
        {
            Utils.Error.DebugbreakFailUnless(
                (uint)ServerType.Dev < ServerTypeCount,
                $"ServerType.Dev should be less than {ServerTypeCount}, but is {ServerType.Dev}",
                this
            );
            Utils.Error.DebugbreakFailUnless(
                (uint)ServerType.Prod < ServerTypeCount,
                $"ServerType.Prod should be less than {ServerTypeCount}, but is {ServerType.Prod}",
                this
            );
            Utils.Error.DebugbreakFailUnless(
                dropdown.options.Count == ServerTypeCount,
                $"Dropdown options count should be {ServerTypeCount}, but is {dropdown.options.Count}",
                this
            );

            Utils.Error.DebugbreakFailUnless(
                string.Equals(dropdown.options[(int)ServerType.Dev].text, ServerType.Dev.ToString(), StringComparison.OrdinalIgnoreCase),
                $"Dropdown option for Dev should be {ServerType.Dev}, but is {dropdown.options[(int)ServerType.Dev].text}",
                this
            );
            Utils.Error.DebugbreakFailUnless(
                string.Equals(dropdown.options[(int)ServerType.Prod].text, ServerType.Prod.ToString(), StringComparison.OrdinalIgnoreCase),
                $"Dropdown option for Prod should be {ServerType.Prod}, but is {dropdown.options[(int)ServerType.Prod].text}",
                this
            );
        }
        private void EnsureServerUrls()
        {
            Utils.Error.DebugbreakFailUnless(
                serverUrls.Length == ServerTypeCount,
                $"Server URLs count should be {ServerTypeCount}, but is {serverUrls.Length}",
                this
            );
            Utils.Error.DebugbreakFailUnless(
                !string.IsNullOrEmpty(serverUrls[(int)ServerType.Dev]),
                $"Server URL for Dev should not be null or empty",
                this
            );
            Utils.Error.DebugbreakFailUnless(
                !string.IsNullOrEmpty(serverUrls[(int)ServerType.Prod]),
                $"Server URL for Prod should not be null or empty",
                this
            );
        }

        private void OnValueChanged(int arg0)
        {
            Utils.Error.DebugbreakFailUnless(
                arg0 >= 0 && arg0 < ServerTypeCount,
                $"Selected index should be between 0 and {ServerTypeCount - 1}, but is {arg0}",
                this
            );
            string selectedServerUrl = serverUrls[arg0];
            label.text = selectedServerUrl;

            ConnectionManager.Instance.ResetConnection(selectedServerUrl);
            ConnectionManager.Instance.StartConnection();
        }
    }
}