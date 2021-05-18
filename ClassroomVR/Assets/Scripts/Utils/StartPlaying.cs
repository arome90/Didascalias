using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassRoomVR
{
    public class StartPlaying : MonoBehaviour
    {
        public AudioSourceScript component;
        public AudioSourceScript componentVR;

        private AudioSourceScript _chosenComp;
        // Start is called before the first frame update
        void Start()
        {
            if (GameManager.Instance.getVR())
                _chosenComp = componentVR;
            else
                _chosenComp = component;
        }

        // Update is called once per frame
        void Update()
        {
            if (!_chosenComp.isPlaying()) {
                GameManager.Instance._sceneManager.startplaying();
                this.gameObject.SetActive(false);
            }
        }
    }
}
