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

        public List<bool> getList() 
        {
            //List<bool> l = new List<bool>();
            //foreach(Toggle t in toggles) 
            //{
            //    l.Add(t.isOn);
            //}
            //return l;
            return null;
        }


        public void Accept()
        {
            GameManager.Instance.SetDeskFormation(getList());
        }

        private void Awake()
        {
            //toggles = new List<Toggle>();
            list_ = new Dictionary<Toggle, Desk>();
            settings = GameManager.Instance.Settings;
            numDesks.SetValue(settings.numDesks);
            numDesks.onValueChanged.AddListener(ChangeObjects);
            DontDestroyOnLoad(parentDesk);
        }

        
        protected void ChangeObjects(float value)
        {
            settings.numDesks = (int)value;
            Set();
        }

    }
}