using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace ClassRoomVR
{
    public class AudioSourceScript : MonoBehaviour
    {
        public AudioSource component;

        public void setClip(AudioClip _clip) {
            component.clip = _clip;
        }
        public void playClip() {
            component.Play();
        }
        public bool isPlaying()
        {
            return component.isPlaying;
        }
    }
}