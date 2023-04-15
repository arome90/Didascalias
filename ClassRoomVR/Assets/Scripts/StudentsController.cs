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
        
        public enum TalkMode { None, Disrespect, Good };
        TalkMode mode;
        Dictionary<string, Student> _students;
        
        //En un futuro tener un set con los estudiantes que han sido o son problematicos
        //HashSet<string> _problematicStudents;


        [SerializeField] Transform frontSide;
        [SerializeField] Transform backCorner;
        [SerializeField] Transform door;

        [SerializeField] DisruptiveAction[] actions;

        public Transform FrontSide { get { return frontSide; } }
        public Transform BackCorner { get { return backCorner; } }
        public Transform Door { get { return door; } }
        //TO DO : cambiar por lista y agregar el nombre


       
        //public void SetParameters(Dictionary<string, Student> students, HashSet<string> problematicStudents)
        //{
        //    _students = students;
        //    _problematicStudents = problematicStudents;
        //}
        public void SetParameters(Dictionary<string, Student> students)
        {
            _students = students;
            
        }

        /// <summary>
        /// Orden de mandar a cambiar de sitio
        /// </summary>
        /// <param name="values"></param>
        public void SendChangeDesk(string[] values)
        {
            Student stu1 = SearchName(values[0]);
            Student stu2 = SearchName(values[1]);
            if (stu1 != null && stu2 != null) ChangeDesk(stu1, stu2);
        }

        /// <summary>
        /// Cambio de sitio entre dos alumnos
        /// </summary>
        /// <param name="stu1"></param>
        /// <param name="stu2"></param>
        private void ChangeDesk(Student stu1, Student stu2)
        {
            Vector3 pos1 = stu1.GetDesk();
            Vector3 pos2 = stu2.GetDesk();
            stu1.ChangeDesk(pos2);
            stu2.ChangeDesk(pos1);
        }

        /// <summary>
        /// Busca un estudiante por su nombre 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="numSearch"></param>
        /// <returns> devuelve el estudiante. NUll si no lo encuentra
        private Student SearchName(string name)
        {
            string n = StringExtensions.SinTildes(name);
            if (_students.ContainsKey(n))
            {
                return _students[n];
            }
            return null;
        }


        /// <summary>
        /// Devuelve la lista de todos los alumnos que estan en la vision del profe
        /// </summary>
        /// <returns></returns>
        public List<string> StudentsOnVision()
        {
            Plane[] cameraFrustum;
            List<string> names;
            names = new List<string>();
            cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);

            foreach (Student s in _students.Values)
            {
                var bounds = s.GetCollider().bounds;
                bounds.center += new Vector3(0, 1f, 0);
                if (GeometryUtility.TestPlanesAABB(cameraFrustum, bounds))
                {
                    names.Add(s.GetName());

                }
            }
            return names;
        }

        /// <summary>
        /// Detecta si un alumno esta en el campo de vision del profesor
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public bool IsStudentOnVision(Student s)
        {
            Plane[] cameraFrustum;
            cameraFrustum = GeometryUtility.CalculateFrustumPlanes(Camera.main);
            var bounds = s.GetCollider().bounds;
            bounds.center += new Vector3(0, 1f, 0);
            return GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
        }

        public void GoOut()
        {
            int i = 0;
            foreach( Student st in _students.Values)
            {
                i++;
                if (!st.GetProblematicStudent()) StartCoroutine(WaitAndExit(st, i));
            }
        }

        IEnumerator WaitAndExit(Student st, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            st.MoveTo(door.position);
        }


        public void HandleSit(string[] alumnos)
        {
            if (alumnos.Length == 1)
            {
                Student stu = SearchName(alumnos[0]);
                if (stu)
                {
                    stu.SitBack();
                }
            }
        }
        public void HandleMove(string[] alumnos, string place)
        {
            if (alumnos.Length > 1)
            {
                SendChangeDesk(alumnos);
            }
            else if( alumnos.Length ==1)
            {
                var pos = Place(place);

                if (pos != null)
                {
                    Student stu = SearchName(alumnos[0]);
                    if (stu)
                    {
                        stu.MoveTo(pos.position);
                    }
                }
            }
        }

        public void HandlePostpone()
        {
            Debug.Log("Posponer situación");
            mode = TalkMode.Good;
        }
        public void HandleExpel(string[] alumnos)
        {
            if (alumnos.Length == 1)
            {
                Student stu = SearchName(alumnos[0]);
                if (stu)
                {
                    stu.MoveTo(door.position);
                }
            }
        }


        public void HandleDisrespect()
        {
            Debug.Log("Has faltado el respeto");
            mode = TalkMode.Disrespect;
        }

        public void HandleCalm() 
        {
            Debug.Log("Has hablado bien ");
            mode = TalkMode.Good;
        }


        public void HandleCall(string[] alumnos)
        {
            if (alumnos.Length == 1)
            {
                Student stu = SearchName(alumnos[0]);
                if (stu)
                {
                    stu.PayAttention();
                }
            }
        }

        public Transform Place(string place)
        {
            Transform trplace = null;
            switch (place)
            {
                case "Fondo":
                    trplace = backCorner;
                    break;
                case "esquina":
                    trplace = frontSide;
                    break;
                case "Fuera":
                    trplace = door;
                    break;
                case "Aquí":
                    trplace = GameManager.Instance.GetPlayer().transform;
                    break;
            }
            return trplace;
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
        public void DoSomethingDisruptive(int i)
        {

            DisruptiveAction a = actions[i];
            int nStu = UnityEngine.Random.Range(0, _students.Count);
            Student stu = null;
            List<Student> list = new List<Student>();
            for (int j = 0; j < a.numStudents; j++)
            {
                stu = _students.ElementAt(nStu).Value;
                AudioClip clip = stu.GetSex() == Student.Gender.Women ? a.audioSituationFemenino : a.audioSituationMasculino;
                stu.SetProblematicStudent();
                stu.PayAttention();
                stu.PlayDisruptiveAction(a.problematicsAnimation.name, clip);
                if (a.pos == Positions.FrontSide) stu.MoveTo(frontSide.position);
                //ia.GetComponent<Action>().SetParameters(stu, a);
                //ia.SetActive(true);
                nStu++;
                if (nStu >= _students.Count)
                {
                    nStu -= 2;
                }
                list.Add(stu);
            }
            if (stu != null)
            {
                actionObject = Instantiate(a.bh);
                actionObject.GetComponent<Action>().SetParameters(list, a);
            }
            GameManager.Instance.GetClassManager().DisruptiveSituation = true;
            //AÑADIR VARIOS ALUMNOS Y ACCIONES QUE SIGNIFICAN UN METODO 
        }

        private void Update()
        {
            int i = 0;
            while (i < actions.Length && !GameManager.Instance.GetClassManager().DisruptiveSituation)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    DoSomethingDisruptive(i);
                }
                i++;
            }


        }

        
    }


    public static class StringExtensions
    {
        public static string SinTildes(this string texto) =>
    new String(
       texto.Normalize(NormalizationForm.FormD)
       .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
       .ToArray()
    )
    .Normalize(NormalizationForm.FormC);
    }
}