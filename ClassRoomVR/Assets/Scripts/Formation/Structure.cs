using System.Collections;
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
        //protected List<Desk> desks_;
        //protected List<Toggle> toggles;

        protected Dictionary<Toggle, Desk> list_;
        protected ClassSettings settings;
        protected  GameObject parent;

        public abstract void Set();

        private void Awake()
        {
            //toggles = new List<Toggle>();
            list_ = new Dictionary<Toggle, Desk>();
            settings = GameManager.Instance.Settings;
            settings.numDesks = settings.NumStu;
            numDesks.SetValue(settings.numDesks);
            numDesks.SetMin(settings.NumStu);
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
            settings.numDesks = (int)value;
            Set();
        }

    }
}