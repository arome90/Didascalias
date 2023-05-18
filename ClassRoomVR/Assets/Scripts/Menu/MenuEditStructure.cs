using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class MenuEditStructure : MonoBehaviour
    {
        private ClassSettings settings;

        [SerializeField] private Button backButton;
        [SerializeField] private Button applyButton;
        [SerializeField] private GameObject nextScreen;
        [SerializeField] private Structure circularStructure;
        [SerializeField] private Structure filaStructure;

        private Structure currentStructure;

        private void Awake()
        {
            settings = GameManager.Instance.GetCurrentSettings();
            backButton.onClick.AddListener(GoBackScreen);
        }

        private void GoBackScreen()
        {
            nextScreen.SetActive(true);
            gameObject.SetActive(false);
        }

        // Temporary
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                GameManager.Instance.LoadMainScene();
            }
        }

        private void OnEnable()
        {
            bool isCircular = settings.StructureMode == StructureMode.Circular
                || settings.StructureMode == StructureMode.U;
            circularStructure.gameObject.SetActive(isCircular);
            filaStructure.gameObject.SetActive(!isCircular);
            currentStructure = isCircular ? circularStructure : filaStructure;
            applyButton.onClick.AddListener(GoBackScreen);
        }
    }
}
