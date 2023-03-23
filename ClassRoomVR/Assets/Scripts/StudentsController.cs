using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Globalization;
using System;
using System.Linq;

namespace ClassRoomVR
{
    public class StudentsController : MonoBehaviour
    {
        
        Camera camera;
        public enum TalkMode { None, Disrespect, Good };
        TalkMode mode;
        Dictionary<string, Student> _students;
        HashSet<string> _problematicStudents;


        [SerializeField] Transform frontSide;
        [SerializeField] Transform backCorner;
        [SerializeField] Transform door;


        public Transform FrontSide { get { return frontSide; } }
        public Transform BackCorner { get { return backCorner; } }
        public Transform Door { get { return door; } }
        //TO DO : cambiar por lista y agregar el nombre


        private void Start()
        {
            camera = Camera.main;
            

        }
        public void SetParameters(Dictionary<string, Student> students, HashSet<string> problematicStudents) 
        {
            _students = students;
            _problematicStudents = problematicStudents;
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
            cameraFrustum = GeometryUtility.CalculateFrustumPlanes(camera);

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
            cameraFrustum = GeometryUtility.CalculateFrustumPlanes(camera);
            var bounds = s.GetCollider().bounds;
            bounds.center += new Vector3(0, 1f, 0);
            return GeometryUtility.TestPlanesAABB(cameraFrustum, bounds);
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

        public Transform Place(string place)
        {
            Transform trplace = null;
            switch (place)
            {
                case "fondo":
                    trplace = backCorner;
                    break;
                case "esquina":
                    trplace = frontSide;
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