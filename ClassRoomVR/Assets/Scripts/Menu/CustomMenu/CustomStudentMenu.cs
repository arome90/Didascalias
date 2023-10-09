using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR
{

    public class CustomStudentMenu : MonoBehaviour
    {
        public Transform togglesParent;
        public Customizable model;
        public List<CustomVariables> customVariables;
        int currentIndex;
        public Toggle togglePrefab;
        public TextMeshProUGUI nameText;
        public Button random;
        //public TextMeshProUGUI characteristicsText;

        private List<GameObject> pageStudents;
        public Button next;
        public Button prev;
        private int currentPage;

        Dictionary<Toggle, int> toggles;

        [SerializeField] List<CustomButtons> botones;
        void Start()
        {
            toggles = new Dictionary<Toggle, int>();
            pageStudents = new List<GameObject>();
            currentPage = 0;
            currentIndex = 0;
            nameText.text = customVariables[currentIndex].name;
            model.SetList(customVariables[currentIndex].list);

            random.onClick.AddListener(() =>
            {
                model.Randomize();
                var list = model.GetList();
                for (int i = 0; i < list.Count; i++)
                {
                    customVariables[currentIndex].list[i] = list[i].GetIndex();
                }

            });
            ToggleGroup toggleContainer = null;
            for (int i = 0; i < customVariables.Count; i++)
            {
                if (i % 10 == 0)
                {
                    string name = "Container" + i % 10;
                    GameObject gm = new GameObject(name, typeof(ToggleGroup), typeof(VerticalLayoutGroup));
                    gm.transform.SetParent(togglesParent, false);
                    toggleContainer = gm.GetComponent<ToggleGroup>();
                    pageStudents.Add(gm);
                    gm.SetActive(false);
                }
                Toggle toggle = Instantiate(togglePrefab, toggleContainer.transform);
                toggle.group = toggleContainer;
                toggle.transform.GetChild(0).GetComponent<TextMeshProUGUI>().SetText(customVariables[i].name);
                toggle.onValueChanged.AddListener(isOn => OnToggleValueChanged(isOn, toggle));
                toggles.Add(toggle, i);

            }
            pageStudents[0].SetActive(true);
            if (pageStudents.Count > 1) { next.gameObject.SetActive(true); }

            next.onClick.AddListener(() =>
            {
                pageStudents[currentPage].SetActive(false);
                currentPage++;
                pageStudents[currentPage].SetActive(true);
                if (currentPage == pageStudents.Count - 1) { next.gameObject.SetActive(false); }
                prev.gameObject.SetActive(true);
            });

            prev.onClick.AddListener(() =>
            {
                pageStudents[currentPage].SetActive(false);
                currentPage--;
                pageStudents[currentPage].SetActive(true);
                if (currentPage == 0) { prev.gameObject.SetActive(false); next.gameObject.SetActive(true); }
                next.gameObject.SetActive(true);

            });


            model.onValueChanged.AddListener(() => {
                var list = model.GetList();
                for (int i = 0; i < list.Count; i++)
                {
                    customVariables[currentIndex].list[i] = list[i].GetIndex();
                    Debug.Log(list[i].GetIndex());
                }
            });

            for (int i = 0; i < botones.Count; i++)
            {
                var bt = botones[i];
                botones[i].SetValue(i);
                botones[i].PrevButton.onClick.AddListener(() => { OnButtonPrevClicked(bt); });
                botones[i].NextButton.onClick.AddListener(() => { OnButtonNextClicked(bt); });

            }

        }


        void OnToggleValueChanged(bool isOn, Toggle toggle)
        {
            if (isOn)
            {
                currentIndex = toggles[toggle];
                nameText.text = customVariables[currentIndex].name;
                model.SetList(customVariables[currentIndex].list);

            }
        }

        void OnButtonNextClicked(CustomButtons but)
        {
            model.SetIndex(but.GetValue());
            model.CurrentCustomization.Next();
            customVariables[currentIndex].list[but.GetValue()] = model.CurrentCustomization.GetIndex();

        }
        void OnButtonPrevClicked(CustomButtons but)
        {
            model.SetIndex(but.GetValue());
            model.CurrentCustomization.Previous();
            customVariables[currentIndex].list[but.GetValue()] = model.CurrentCustomization.GetIndex();

        }






    }

}