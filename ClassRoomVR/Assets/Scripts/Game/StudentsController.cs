using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Globalization;
using System;
using System.Linq;
using System.Collections;

namespace ClassRoomVR
{
    public class StudentsController : MonoBehaviour
    {

        TalkMode mode;
        Dictionary<string, Student> students;

        [SerializeField] Transform frontSide;
        [SerializeField] Transform backCorner;
        [SerializeField] Transform door;

        [SerializeField] DisruptiveAction[] actions;

        public Transform FrontSide => frontSide;
        public Transform BackCorner => backCorner;
        public Transform Door => door;

        public void SetParameters(Dictionary<string, Student> students)
        {
            this.students = students;
        }

        public void SendChangeDesk(string[] values)
        {
            Student student1 = SearchName(values[0]);
            Student student2 = SearchName(values[1]);
            if (student1 != null && student2 != null)
                ChangeDesk(student1, student2);
        }

        private void ChangeDesk(Student student1, Student student2)
        {
            var position1 = student1.GetDesk();
            var position2 = student2.GetDesk();
            student1.ChangeDesk(position2);
            student2.ChangeDesk(position1);
        }

        private Student SearchName(string name)
        {
            string normalized = name.Normalize(NormalizationForm.FormD);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            string noTildesName = stringBuilder.ToString().Normalize(NormalizationForm.FormC);
            if (students.ContainsKey(noTildesName))
                return students[noTildesName];
            return null;
        }

        public List<string> StudentsOnVision()
        {
            Plane[] cameraFrustum;
            List<string> names = new List<string>();
            cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);

            foreach (Student student in students.Values)
            {
                Bounds bounds = student.GetCollider().bounds;
                bounds.center += new Vector3(0, 1f, 0);
                if (GeometryUtility.TestPlanesAABB(cameraFrustum, bounds))
                {
                    names.Add(student.GetStudentName());
                }
            }
            return names;
        }

        public bool IsStudentInFieldOfVision(Student student)
        {
            Plane[] cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            Bounds bounds = student.GetCollider().bounds;
            bounds.center += new Vector3(0, 1f, 0);
            return GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
        }

        public void GoOut()
        {
            int i = 0;
            foreach (Student student in students.Values)
            {
                i++;
                if (!student.IsProblematicStudent())
                    StartCoroutine(WaitAndExit(student, i));
            }
        }

        IEnumerator WaitAndExit(Student student, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            student.MoveTo(door.position);
        }

        public void HandleSit(string[] students)
        {
            if (students.Length == 1)
            {
                Student student = SearchName(students[0]);
                if (student != null)
                    student.SitBack();
            }
        }

        public void HandleMove(string[] students, string place)
        {
            if (students.Length > 1)
            {
                SendChangeDesk(students);
            }
            else if (students.Length == 1)
            {
                Transform position = Place(place);
                if (position != null)
                {
                    Student student = SearchName(students[0]);
                    if (student != null)
                        student.MoveTo(position.position);
                }
            }
        }
        public void HandlePostpone()
        {
            Debug.Log("Posponer situación");
            mode = TalkMode.Good;
        }

        public void HandleExpel(string[] students)
        {
            if (students.Length == 1)
            {
                Student student = SearchName(students[0]);
                if (student != null)
                    student.MoveTo(door.position);
            }
        }

        public void HandleDisrespect()
        {
            Debug.Log("Has faltado el respeto");
            mode = TalkMode.Disrespect;
        }

        public void HandleCalm()
        {
            Debug.Log("Has hablado bien");
            mode = TalkMode.Good;
        }

        public void HandleCall(string[] students)
        {
            if (students.Length == 1)
            {
                Student student = SearchName(students[0]);
                if (student != null)
                    student.PayAttention();
            }
        }

        public Transform Place(string place)
        {
            Transform position = null;
            switch (place)
            {
                case "Fondo":
                    position = backCorner;
                    break;
                case "esquina":
                    position = frontSide;
                    break;
                case "Fuera":
                    position = door;
                    break;
                case "Aquí":
                    position = GameManager.Instance.GetPlayer().transform;
                    break;
            }
            return position;
        }

        public TalkMode GetMode()
        {
            return mode;
        }

        public void SetMode(TalkMode value)
        {
            mode = value;
        }

        private GameObject actionObject;

        public void DoSomethingDisruptive(int index)
        {
            DisruptiveAction action = actions[index];
            int randomStudentIndex = UnityEngine.Random.Range(0, students.Count);
            Student student = null;
            List<Student> studentList = new List<Student>();
            for (int i = 0; i < action.numStudents; i++)
            {
                student = students.ElementAt(randomStudentIndex).Value;
                AudioClip clip = student.GetGender() == Gender.Women ? action.situationAudioFeminine : action.situationAudioMasculine;
                student.SetProblematicStudent();
                student.PayAttention();
                student.PlayDisruptiveAction(action.problematicsAnimation.name, clip);
                if (action.position == Positions.FrontSide)
                    student.MoveTo(frontSide.position);
                randomStudentIndex++;
                if (randomStudentIndex >= students.Count)
                    randomStudentIndex -= 2;
                studentList.Add(student);
            }
            if (student != null)
            {
                actionObject = Instantiate(action.behaviorHolder);
                actionObject.GetComponent<Action>().SetParameters(studentList, action);
            }
            GameManager.Instance.GetClassManager().DisruptiveSituation = true;
        }


        float a = 0;

        private void Start()
        {
            var b=gameObject.AddComponent<VariableMeasurement>();
            b.del+=(()=>{ return a; });
            b.Set(3f);
            
        }
        private void Update()
        {
            if (GameManager.Instance.isPause) return;
            int index = 0;
            while (index < actions.Length && !GameManager.Instance.GetClassManager().DisruptiveSituation)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + index))
                {
                    DoSomethingDisruptive(index);
                }
                index++;
            }

            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                // students.First().Value.GenerateText();
                a++;
                Debug.Log(a);
            }
        }
    }
}

public static class StringExtensions
{
    public static string SinTildes(this string texto)
    {
        string normalized = texto.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();
        foreach (char c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }
        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}
