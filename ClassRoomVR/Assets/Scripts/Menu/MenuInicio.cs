using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class MenuInicio : MonoBehaviour
    {
        [SerializeField] private Button enterButton;
        [SerializeField] private Button enter2Button;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject nextScreen;
        [SerializeField] private Vector3 playerDestination;
        [SerializeField] private Vector3 playerInitialPosition;
        [SerializeField] private Transform player;
        [SerializeField] List<GameObject> maps;
        [SerializeField] RectTransform rect;
        [SerializeField] RectTransform rect2;
        [SerializeField] GameObject canvas;

        private void Start()
        {
            enterButton.onClick.AddListener(() =>
            {
                PlayButton();
                GoNextScreen();
                maps[0].SetActive(true);
                maps[1].SetActive(false);
                var a =canvas.GetComponent<RectTransform>();
                MatchOther(a, rect);
                GameManager.Instance.SetScene(1);
            });
            enter2Button.onClick.AddListener(() =>
            {
                PlayButton();
                GoNextScreen();
                maps[1].SetActive(true);
                maps[0].SetActive(false);
                var a = canvas.GetComponent<RectTransform>();
                MatchOther(a, rect2);
                GameManager.Instance.SetScene(2);
            });
            quitButton.onClick.AddListener(QuitButton);
            // Remove in the future
            DeskManager.Instance.DestroyChildren();
            DontDestroyOnLoad(GameObject.Find("DeskManager"));
        }

        public void MatchOther( RectTransform rt, RectTransform other)
        {
            Vector2 myPrevPivot = rt.pivot;
            myPrevPivot = other.pivot;
            rt.position = other.position;

            rt.localScale = other.localScale;

            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, other.rect.width);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, other.rect.height);
            //rectTransf.ForceUpdateRectTransforms(); - needed before we adjust pivot a second time?
            rt.pivot = myPrevPivot;
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
            SetPlayerPositionAndRotation(playerInitialPosition, Quaternion.Euler(Vector3.down * 90.0f));
            gameObject.SetActive(true);
            maps[0].SetActive(true);
            maps[1].SetActive(false);

        }

        private void OnEnable()
        {
            ReturnButton();
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

