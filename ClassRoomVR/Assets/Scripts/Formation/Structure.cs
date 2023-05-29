using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public abstract class Structure : MonoBehaviour
    {
        [SerializeField] protected Desk desk;
        [SerializeField] protected GameObject parentDesk;
        [SerializeField] protected Option numDesks;
        [SerializeField] protected Toggle prefab;

        protected Dictionary<Toggle, Desk> list_;
        protected ClassSettings settings;
        protected GameObject parent;

        protected int lastOptionClicked=0;
        public abstract void Set();

        private void Awake()
        {
            list_ = new Dictionary<Toggle, Desk>();
            settings = GameManager.Instance.GetCurrentSettings();
            numDesks.onValueChanged.AddListener(ChangeObjects);
            DontDestroyOnLoad(parentDesk);
        }

        private void OnDisable()
        {
            foreach (Transform child in parentDesk.transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        protected void ChangeObjects(float value)
        {
            lastOptionClicked = 0;
            settings.NumDesks = (int)value;
            Set();
        }
    }
}
