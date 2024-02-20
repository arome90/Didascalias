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

        // Method to handle changing desks for students
        public void SendChangeDesk(string[] values)
        {
            Student s1, s2;
            if (TryGetStudent(values[0], out s1) && TryGetStudent(values[1], out s2))
                ChangeDesk(s1, s2);
        }

        private void ChangeDesk(Student student1, Student student2)
        {
            var position1 = student1.GetDesk();
            var position2 = student2.GetDesk();
            student1.ChangeDesk(position2);
            student2.ChangeDesk(position1);
        }

        //// Remove diacritics (accent marks) from a string
        //private string RemoveDiacritics(string text)
        //{
        //    string normalized = text.Normalize(NormalizationForm.FormD);
        //    StringBuilder stringBuilder = new StringBuilder();
        //    foreach (char c in normalized)
        //    {
        //        if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
        //            stringBuilder.Append(c);
        //    }
        //    return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        //}

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
        public void HandleSit(Student student)
        {
            if (student != null) { student.SitBack(); }
        }

        // Handle moving actions for students
        public void HandleMove(Student student, string place)
        {
            if (student != null)
            {
                Transform position = Place(place);
                if (position != null)
                {
                    student.MoveTo(position.position,1.5f);
                }
            }
        }

        // Handle postponing situations
        public void HandlePostpone()
        {
            Debug.Log("Postpone situation");
            mode = TalkMode.Good;
        }

        // Handle expelling students
        public void HandleExpel(Student student)
        {
            if (student != null)
            {
                student.MoveTo(door.position, 0.5f);
            }
        }

        // Handle disrespectful behavior
        public void HandleDisrespect()
        {
            Debug.Log("You have shown disrespect");
            mode = TalkMode.Disrespect;
        }

        // Handle calming situations
        public void HandleCalm()
        {
            Debug.Log("You have spoken well");
            mode = TalkMode.Good;
        }

        public void HandleNormal()
        {
            Debug.Log("You have spoken");
            mode = TalkMode.Normal;
        }


        // Handle calling a student's attention
        public void HandleCall(Student student)
        {
            student.PayAttention();
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
        [SerializeField]
        TMPro.TextMeshProUGUI text;
        // Perform a disruptive action on students
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
                    student.MoveTo(frontSide.position, 1f);
                randomStudentIndex++;
                if (randomStudentIndex >= students.Count)
                    randomStudentIndex -= 2;
                studentList.Add(student);
            }
            if (student != null)
            {
                actionObject = Instantiate(action.behaviorHolder);
                actionObject.GetComponent<Action>().SetParameters(player,studentList, action,text);
            }
            ClassManager.Instance.DisruptiveSituation = true;
        }




        public void SeparateStudent() 
        {
            int randomStudentIndex = UnityEngine.Random.Range(0, students.Count);
            Student student = students.ElementAt(randomStudentIndex).Value;
            Student obj = null;//  ESTUDIANTE OBJETIVO AL QUE LA VA A QUITAR EL SITIO ;
            Student cOMPAÑE = null;// COMPAÑERO DE TRASTADAS ;
            //el student va hasta el sitio del objetivo cuando llega le dice al objetivo me quiero sentar con el compañero de al lado 
            // el otro estudainte se va al sitio del otro y estos dos al sentarse dicen ale ya estamos juntos 
        }
        public void PlaySentence(string text)
        {
            students.ElementAt(0).Value.GenerateText(text);
        }

        private void Update()
        {
            if (GameManager.Instance.IsPause) return;
            int index = 0;
            while (index < actions.Length && !ClassManager.Instance.DisruptiveSituation)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + index))
                {
                    DoSomethingDisruptive(index);
                }
                index++;
            }
        }
        
    }
}
        // Get a list of student names that are in the camera's field of vision
        //public List<string> StudentsOnVision()
        //{
        //    Plane[] cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
        //    return students.Values.Where(student =>
        //    {
        //        Bounds bounds = student.GetCollider().bounds;
        //        bounds.center += new Vector3(0, 1f, 0);
        //        return GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
        //    }).Select(student => student.GetStudentName()).ToList();
        //}