using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class MenuInicio : MonoBehaviour
    {
        [SerializeField] private Button enterButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject nextScreen;
        [SerializeField] private Vector3 playerDestination;
        [SerializeField] private Vector3 playerInitialPosition;
        [SerializeField] private Transform player;

        private void Start()
        {
            enterButton.onClick.AddListener(() =>
            {
                PlayButton();
                GoNextScreen();
            });
            quitButton.onClick.AddListener(QuitButton);
            // Remove in the future
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
            if (player != null)
            {
                player.position = playerInitialPosition;
                player.rotation = Quaternion.Euler(Vector3.down * 90.0f);
            }
        }

        private void OnEnable()
        {
            ReturnButton();
        }
    }
}
