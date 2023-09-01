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
            UpdateStructureVisibility();
            applyButton.onClick.AddListener(GoBackScreen);
        }

        private void UpdateStructureVisibility()
        {
            bool isCircular = settings.StructureMode == StructureMode.Circular
                || settings.StructureMode == StructureMode.U;

            SetStructureVisibility(circularStructure, isCircular);
            SetStructureVisibility(filaStructure, !isCircular);

            currentStructure = isCircular ? circularStructure : filaStructure;
        }
        private void SetStructureVisibility(Structure structure, bool isVisible)
        {
            if (structure != null)
            {
                structure.gameObject.SetActive(isVisible);
            }
        }
    }
}
