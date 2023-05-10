using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class MenuInicio : MonoBehaviour
    {
        [SerializeField] Button entrar;
        [SerializeField] Button salir;
        [SerializeField] GameObject nextScreen;
        // Use this for initialization
        [SerializeField] Vector3 playerDest;
        [SerializeField] Vector3 playerInit;
        [SerializeField] Transform player;

        void Start()
        {
            
            entrar.onClick.AddListener(() =>
            {
                PlayButton();
                GoNextScreen();
            });
            salir.onClick.AddListener(QuitButton);
        }

        void GoNextScreen()
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
                player.position = playerDest ;

            }
        }

        public void ReturnButton()
        {
            if (player != null)
            {

                player.position = playerInit;
                player.rotation = Quaternion.Euler(Vector3.down * 90.0f);

            }
        }

        private void OnEnable()
        {
            ReturnButton();
        }
    }
}