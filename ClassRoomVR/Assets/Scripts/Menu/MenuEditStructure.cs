using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{
    public class MenuEditStructure : MonoBehaviour
    {
        private ClassSettings settings;

        //[SerializeField] private Button backButton;
        [SerializeField] private Button applyButton;
        //[SerializeField] private GameObject nextScreen;
        [SerializeField] private Structure circularStructure;
        [SerializeField] private Structure filaStructure;


        private void Awake()
        {
            settings = GameManager.Instance.GetCurrentSettings();
            applyButton.onClick.AddListener(GoBackScreen);

            //  backButton.onClick.AddListener(GoBackScreen);
        }

        private void GoBackScreen()
        {
            MenuTransition.Instance.GoBackScreen();
            MenuTransition.Instance.MovePizarra();
        }


        private void OnEnable()
        {
            UpdateStructureVisibility();
        }

        private void UpdateStructureVisibility()
        {
            bool isCircular = settings.StructureMode == StructureMode.Circular
                || settings.StructureMode == StructureMode.U;

            SetStructureVisibility(circularStructure, isCircular);
            SetStructureVisibility(filaStructure, !isCircular);

           // currentStructure = isCircular ? circularStructure : filaStructure;
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
