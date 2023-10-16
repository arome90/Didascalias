using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public abstract class Structure : MonoBehaviour
    {
        [SerializeField] protected Desk desk; // Prefab of a desk
        [SerializeField] protected GameObject parentDesk; // Parent object for desks
        [SerializeField] protected Option numDesks; // UI option for setting the number of desks
        [SerializeField] protected Toggle prefab; // Toggle prefab for controlling desk visibility
        [SerializeField] protected BoxCollider aula;

        protected Dictionary<Toggle, Desk> toggleToDeskMap; // Map to associate toggles with desks
        protected ClassSettings settings; // Settings for the classroom
        protected GameObject parent; // Parent object for toggles

        protected int lastOptionClicked = 0; // Identifier for the last UI option clicked

        public abstract void Set(); // Abstract method to be implemented by derived classes
        public abstract void MaxDesk();
        private void Awake()
        {
            InitializeComponents();
            numDesks.onValueChanged.AddListener(ChangeObjects);
            DontDestroyOnLoad(parentDesk); // Prevent parentDesk from being destroyed on scene load
            MaxDesk();
        }

        private void OnDisable()
        {
            DestroyInactiveChildObjects(); // Clean up inactive child objects under parentDesk
        }

        protected void ChangeObjects(float value)
        {
            lastOptionClicked = 0;
            settings.NumDesks = (int)value; // Update the number of desks in settings
            Set(); // Call the Set() method to arrange desks based on updated settings
        }

        private void InitializeComponents()
        {
            toggleToDeskMap = new Dictionary<Toggle, Desk>(); // Initialize the dictionary
            settings = GameManager.Instance.GetCurrentSettings(); // Get the current classroom settings
        }

        private void DestroyInactiveChildObjects()
        {
            foreach (Transform child in parentDesk.transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    Destroy(child.gameObject); // Destroy inactive child objects under parentDesk
                }
            }
        }
    }
}
