using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class MenuInicio : MonoBehaviour
    {
        [SerializeField] private Button enter;
        [SerializeField] private Button tutorial;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject nextScreen;
        [SerializeField] private Vector3 playerDestination;
        [SerializeField] private Vector3 playerInitialPosition;
        [SerializeField] private Transform player;
        
        private void Start()
        {
            enter.onClick.AddListener(() =>
            {
                PlayButton();
                GoNextScreen();
            });
            tutorial.onClick.AddListener(() =>
            {
                tutorial.interactable = false;
                GameManager.Instance.LoadTutorial();
            });

            quitButton.onClick.AddListener(QuitButton);
            // Remove in the future
            DeskManager.Instance.DestroyChildren();
            DontDestroyOnLoad(GameObject.Find("DeskManager"));
        }

        private void GoNextScreen()
        {
            nextScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        public void QuitButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void PlayButton()
        {
            if (player != null)
            {
                player.rotation = Quaternion.Euler(Vector3.zero);
                player.position = playerDestination;
            }
        }


        public void ReturnButton()
        {
            SetPlayerPositionAndRotation(playerInitialPosition, Quaternion.Euler(Vector3.up * 90.0f));
            gameObject.SetActive(true);
        }

        private void OnEnable()
        {
            ReturnButton();
            nextScreen.SetActive(false);
        }

        private void SetPlayerPositionAndRotation(Vector3 position, Quaternion rotation)
        {
            if (player != null)
            {
                player.position = position;
                player.rotation = rotation;
            }
        }



        //TO DO ANIMACION
        IEnumerator ScaleOverTime(GameObject button, float scaleFactor)
        {
            Vector3 originalScale = button.transform.localScale;
            Vector3 destinationScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);

            float currentTime = 0.0f;

            do
            {
                button.transform.localScale = Vector3.Lerp(originalScale, destinationScale, currentTime / 0.5f);
                currentTime += Time.deltaTime;
                yield return null;
            }
            while (currentTime <= 1f);
        }
    }
}

