using System.Collections.Generic;
using UnityEngine;
using System.Linq;


namespace ClassRoomVR
{
    public class ClassManager : MonoBehaviour
    {
        [SerializeField]
        Transform studentsPositions;
        [SerializeField]
        private Student prefabStudent;
        bool[] _asientosOcupados;
        HashSet<string> _problematicStudents;
        Dictionary<string, Student> _students;
        StudentsController studentsController;

        [SerializeField] DesksManager desksManager;

        [SerializeField] Transform[] targetsHead;

        [SerializeField]StudentsSettings settings;

        bool disruptiveSituation;

        public bool DisruptiveSituation { get => disruptiveSituation; set => disruptiveSituation = value; }


        int clima;

        private void Start()
        {
            GameManager.Instance.setClass(this);
            studentsController = GetComponent<StudentsController>();

            _asientosOcupados = new bool[studentsPositions.childCount];
            _students = new Dictionary<string, Student>();

            generateChilds();
            StartScene();
        }

        //Genera los alumnos de la clase en sus posiciones
        private void generateChilds()
        {
            ScenePackage sceneInfo = GameManager.Instance.getPack();
            Instantiate(sceneInfo.scene);
            ClassInfo classInfo = GameManager.Instance.getClass();
            //Se usan listas para tener una lista auxiliar de la que se eliminan sus componentes
            //De esta manera no se repite ningun nombre ni body hasta que se agoten
            List<List<string>> names = new List<List<string>>();
            names.Add(classInfo.girlsNames.ToList());
            names.Add(classInfo.boysNames.ToList());
            List<GameObject[]> prefabBodys = new List<GameObject[]>();
            prefabBodys.Add(classInfo.girlsPrefabs);
            prefabBodys.Add(classInfo.boysPrefabs);

            
            int deskPos = 0;
            int randomStudents = settings.NumStu;

            if (settings.Mode == StudentsSettings.GenerateMode.Personalizado)
            {
                generatePersonalizedChild(ref deskPos, classInfo, sceneInfo);
                randomStudents -= deskPos;
            }
            // Instanciamos los alumnos en sus posiciones de manera aleatoria (el prefab).
            for (int i = 0; i <= randomStudents && deskPos < 30; i++)
            {
                int gender = Random.Range(0, 2); // 0 mujer, 1 hombre
                int indexName = Random.Range(0, names[gender].Count);
                Student pickedStudent = CreateStudent(prefabBodys[gender][Random.Range(0, prefabBodys[gender].Length)], names[gender][indexName], (Student.Gender)gender);
                names[gender].RemoveAt(indexName);
                PlaceStudent(ref deskPos, pickedStudent, sceneInfo.nGroups);
                deskPos++;
            }

            SetProblematicStudents(sceneInfo);
            // Ejecutamos animaciones con distinto timing
            PlayAnimationsAtDifferentTimeClass(classInfo.idleAnim.name);
        }

        private Student CreateStudent(GameObject body, string name, Student.Gender gender)
        {
            // Elegimos el sexo del estudiante
            Student pickedStudent;
            pickedStudent = Instantiate(prefabStudent, transform);
            pickedStudent.SetParameters(name, gender);
            pickedStudent.CreateBody(body);
            pickedStudent.SetTargets(targetsHead);
            _students.Add(name, pickedStudent);
            return pickedStudent;
        }

        private void PlaceStudent( ref int deskPos,Student pickedStudent, int nGruops)
        {
            desksManager.getFreeDesk(ref deskPos, nGruops);
            // Lo colocamos en su pupitre
            Transform pos = studentsPositions.GetChild(deskPos);
            Vector3 position = pos.position + new Vector3(0, -0.4f, 0);
            pickedStudent.transform.SetPositionAndRotation(position, pos.rotation);
            pickedStudent.SetDesk(position);
            _asientosOcupados[deskPos] = true;

        }


        private void generatePersonalizedChild(ref int deskPos, ClassInfo classInfo,ScenePackage sceneInfo)
        {
 
            var list = settings.Students;
            // Instanciamos los alumnos en sus posiciones .
            for (int i = 0; i < list.Length ; i++)
            {
                StudentInfo info = list[i];
                Student.Gender gen = (Student.Gender) GetEnumValue<StudentInfo.GenderInfo>((int)info.Gender);
                int nBody =GetEnumValue<StudentInfo.BodyInfo>((int)info.Body);
                GameObject body = gen == Student.Gender.Men ? classInfo.boysPrefabs[nBody] : classInfo.girlsPrefabs[nBody];
                Student pickedStudent = CreateStudent(body, info.Name, gen);
                PlaceStudent(ref deskPos, pickedStudent, sceneInfo.nGroups);
                deskPos++;
               
            }
          
        }
        /// <summary>
        /// Todos los Enums tienen un identificador random al principio. Si el valor es cero se devuelve un valor random entre los demas valores.
        /// Si el valor no es cero se devuelve ese valor directamente . Se resta uno en ambos lados para normalizar sin random 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private int GetEnumValue<T>(int value) 
        {
            int lenght = System.Enum.GetNames(typeof(T)).Length - 1;
            return value == 0 ? Random.Range(0,lenght): value -1;
        }

        //public T PickRandom<T>(IList<T> options)
        //{
        //    int index = Random.Range(0, options.Count);
        //    return options[index];
        //}


        private void SetProblematicStudents(ScenePackage scene ) 
        {
            // Estudiantes problematicos
            _problematicStudents = new HashSet<string>();
            int problematic = UnityEngine.Random.Range(0, settings.NumStu);
            _problematicStudents.Add(_students.ElementAt(problematic).Key);
            _students.ElementAt(problematic).Value.SetProblematicStudent();
            if (scene.problematicTogether)
            {
                problematic = desksManager.GetNearDeskRandom(problematic, settings.NumStu);
                _problematicStudents.Add(_students.ElementAt(problematic).Key);
                _students.ElementAt(problematic).Value.SetProblematicStudent();
            }
           
        }

        private void PlayAnimationsAtDifferentTimeClass(string animName)
        {
            // Play animations at different time
            float time = 0.0f;
            foreach (Student s in _students.Values)
            {
                time = time + 1.0f / 8.0f;
                s.transform.GetChild(s.transform.childCount - 1).GetComponent<Animator>().Play(animName, 0, time);
            }
        }

        private void StartScene()
        {
            string alumsName = "";
            // Mostramos el texto descriptivo de la escena
            for (int i = 0; i < _problematicStudents.Count; i++)
            {
                if (i > 0 && i != _problematicStudents.Count - 1) alumsName += ", ";
                else if (i > 0 && i == _problematicStudents.Count - 1) alumsName += " y ";
                alumsName += _problematicStudents.ElementAt(i);
                if (i > 1 && i == _problematicStudents.Count - 1) alumsName += ";";
            }
            GameObject player = GameManager.Instance.GetPlayer();
            ScenePackage sceneInfo = GameManager.Instance.getPack();
            player.GetComponent<AudioSource>().clip = sceneInfo.contextClip;
            player.GetComponent<AudioSource>().Play();
            string t = sceneInfo.iniMessage.Replace("alum", alumsName);
            Debug.Log(t);

            studentsController.SetParameters(_students, _problematicStudents);
            //uiManager.panelContexto(t);
        }

        
        public Student[] GetStudents() { return _students.Values.ToArray(); }

       public StudentsController GetStudentsController() { return studentsController; }

    }
}
      
