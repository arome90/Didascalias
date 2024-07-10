using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Globalization;
using System;
using System.Linq;
using System.Collections;
using MathNet.Numerics.Distributions;
using System.Diagnostics.Eventing.Reader;
using Unity.VisualScripting;

namespace ClassRoomVR
{
    public class StudentsController : MonoBehaviour
    {
        TalkMode mode;
        private Actions res;
        public Actions Resolutions
        {
            get { return res; }
            set { res = value; }
        }

        Dictionary<string, Student> students;
        GameObject player;
        // Serialized fields for defining positions in the classroom
        [SerializeField] Transform frontSide;
        [SerializeField] Transform backCorner;
        [SerializeField] Transform door;

        // Serialized array of disruptive actions
        [SerializeField] DisruptiveAction[] actions;

        // Getter properties for classroom positions
        public Transform FrontSide => frontSide;
        public Transform BackCorner => backCorner;
        public Transform Door => door;
        // Method to set the dictionary of students
        public void SetParameters(GameObject player,Dictionary<string, Student> students)
        {
            this.player = player;
            this.students = students;
        }

        public void ChangeDesk(Student student1, Student student2)
        {
            var position1 = student1.GetDesk();
            var position2 = student2.GetDesk();
           StartCoroutine(student2.ChangeDesk(position1));
           StartCoroutine(student1.ChangeDesk(position2));
        }

        // Search for a student by name, handling diacritics
        public bool TryGetStudent(string name, out Student student)
        {
            student = null;
            if (students.ContainsKey(name))
            {
                student = students[name];
                return true;
            }
            return false;
        }


        // Make non-problematic students exit the classroom
        public void GoOut()
        {
            int i = 0;
            foreach (Student student in students.Values.Where(s => !s.IsProblematicStudent()))
            {
                i++;
                StartCoroutine(WaitAndExit(student, i));
            }
        }

        IEnumerator WaitAndExit(Student student, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            student.MoveTo(door.position,0.5f);
        }

        // Handle sitting actions for students
        public void HandleSit(List<Student> studentList)
        {
            foreach (var student in studentList)
            {
                student.SitBack();
            }
        }

        // Handle moving actions for students
        public void HandleMove(List<Student> studentList, string place = null)
        {

            foreach (var student in studentList)
            {
                Transform position = Place(place);
                if (position != null)
                {
                    student.MoveTo(position.position, 1.5f);
                }
            }
        }

        public void HandleChange(List<Student> studentList)
        {
            if(studentList.Count > 1) 
            {
                ChangeDesk(studentList[0], studentList[1]);
            }
        }

        // Handle postponing situations
        public void HandlePostpone()
        {
            Debug.Log("Postpone situation");
            mode = TalkMode.Good;
        }

        // Handle expelling students
        public void HandleExpel(List<Student> studentList)
        {
            foreach (var student in studentList)
            {
                student.MoveTo(door.position, 0.5f);
            }
        }

        // Handle calling a student's attention
        public void HandleCall(Student student)
        {
            student.PayAttention();
            student.GetNameText().color = Color.blue;
            StartCoroutine(ReturnColor(student));
        }
        IEnumerator ReturnColor(Student student)
        {
            yield return new WaitForSeconds(5);
            student.GetNameText().color = Color.white;
        }
        // Determine a position based on a string description
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
                    position = player.transform;
                    break;
            }
            return position;
        }

        // Get the current talk mode
        public TalkMode GetMode()
        {
            return mode;
        }

        // Set the current talk mode
        public void SetMode(TalkMode value)
        {
            mode = value;
        }

        private GameObject actionObject;
       
        DisruptiveAction actionActual;
        List<Student> studentList;
        // Perform a disruptive action on students
        public void DoSomethingDisruptive(int index)
        {
            if (actionActual == null && actionObject == null)
            {
                actionActual = actions[index];
                res = Actions.None;
                int randomStudentIndex = UnityEngine.Random.Range(0, students.Count);
                Student student = students.ElementAt(randomStudentIndex).Value;
                if (studentList != null) studentList.Clear();
                studentList = new List<Student> { student };
                switch (actionActual.action)
                {
                    case Actions.Insultar:
                        StartCoroutine(ActionsMethod.Insultar(student, actionActual, CreateConflict));
                        break;
                    case Actions.Separados:
                        randomStudentIndex = randomStudentIndex != students.Count - 1 && ((randomStudentIndex + 1) % GameManager.Instance.GetCurrentSettings().Columns != 0) ? randomStudentIndex + 1 : randomStudentIndex - 1;
                        Student secstudent = students.ElementAt(randomStudentIndex).Value;
                        var problem = GetRandomStudentExcluding(student, secstudent);
                        studentList.Add(problem);
                        StartCoroutine(ActionsMethod.SentarseJuntos(problem, student, secstudent, actionActual, CreateConflict));
                        break;
                    case Actions.Levantarse:
                        ActionsMethod.Levantarse(student, actionActual, frontSide.position, CreateConflict);
                        break;

                }

            }
        }
        [SerializeField]
        TMPro.TextMeshProUGUI text;
        private void CreateConflict() 
        {
            actionObject = Instantiate(actionActual.behaviorHolder);
            actionObject.GetComponent<Action>().SetParameters(player, studentList, actionActual, text);
            actionActual = null;
            foreach(var student in studentList) 
            {
                student.SetProblematicStudent();
                student.PayAttention();
            }
        }

        private Student GetRandomStudentExcluding(Student exclude1, Student exclude2)
        {
            List<Student> eligibleStudents = students.Values.Where(s => s != exclude1 && s != exclude2).ToList();
            int randomIndex = UnityEngine.Random.Range(0, eligibleStudents.Count);
            return eligibleStudents[randomIndex];
        }

        //private Student GetRandomStudentSitting() 
        //{
        //    List<Student> eligibleStudents = students.Values.Where(s => s.state != State.Standing).ToList();
        //    int randomIndex = UnityEngine.Random.Range(0, eligibleStudents.Count);
        //    return eligibleStudents[randomIndex];
        //}

        public void PlaySentence(string text)
        {
            int randomStudentIndex = UnityEngine.Random.Range(0, students.Count);
            students.ElementAt(randomStudentIndex).Value.GenerateText(text);
        }

        public void PlayAllSentence(string text)
        {
            for (int i = 0; i < students.Count - 1; i++)
            {
                students.ElementAt(i).Value.GenerateText(text);

            }
        }

        //private void Start()
        //{
        //    Invoke(nameof(doso), 2);
        //}

        //private void doso()
        //{
        //    DoSomethingDisruptive(1);
        //}

    }
}