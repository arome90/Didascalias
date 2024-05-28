using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public abstract class Structure : MonoBehaviour
    {
        [SerializeField] protected ToggleDesks toggleDesks;
        [SerializeField] protected Option numDesks; // UI option for setting the number of desks
        protected ClassSettings settings; // Settings for the classroom
        public abstract void Set(); // Abstract method to be implemented by derived classes
        public abstract int MaxDesk();
        private void OnDisable()
        {
           DeskManager.Instance.DestroyInactiveChildObjects(); // Clean up inactive child objects under parentDesk
        }

        protected void ChangeObjects(float value)
        {
            settings.NumDesks = (int)value; // Update the number of desks in settings
            Set(); // Call the Set() method to arrange desks based on updated settings
        }
    }
}
