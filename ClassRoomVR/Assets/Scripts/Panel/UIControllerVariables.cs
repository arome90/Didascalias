using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    public class UIControllerVariables : MonoBehaviour
    {
        [SerializeField] List<UIVariable> list;
        [SerializeField] ClassManager stu;
        [SerializeField] UIVariable prefab;
        private void Start()
        {
            CreatePanel(stu.GetStudents());
        }

        private void Update()
        {
            UpdatePanel(stu.GetStudents());
        }

        public void CreatePanel(Student[] students)
        {
            for (int i = 0; i < students.Length; i++)
            {
                var a = Instantiate(prefab, transform);
                a.SetStatusText(students[i].name);
                list.Add(a);
            }
        }


        public void UpdatePanel(Student[]students)
        {
            for (int i = 0; i < students.Length; i++)
            {
                var att = students[i].GetBehavior().AttentionLevel;
                list[i].SetStatus(att);
                //list[i].SetStatusPerText(att.ToString("0.##"));
                list[i].SetStatusPerText(students[i].GetBehavior().resta.ToString("0.##"));
            }
        }
    }
}