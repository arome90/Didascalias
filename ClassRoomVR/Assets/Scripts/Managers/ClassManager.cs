using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ClassRoomVR
{
    public class ClassManager : MonoBehaviour
    {
        private Transform studentsPositions;
        [SerializeField] private Student prefabStudent;
        private bool[] asientosOcupados;
        private HashSet<string> problematicStudents;
        private Dictionary<string, Student> students;
        private StudentsController studentsController;

        [SerializeField] private Transform[] targetsHead;

        private bool disruptiveSituation;
        public bool DisruptiveSituation { get => disruptiveSituation; set => disruptiveSituation = value; }

        private int clima;
        private ClassInfo classInfo;
        private List<List<string>> names;
        private List<GameObject[]> prefabBodys;
        private ClassSettings settings;

        [SerializeField] private AudioClip beforeClassBell;
        [SerializeField] private AudioClip afterClassBell;

        private void Awake()
        {
            GameManager.Instance.SetClassManager(this);
            settings = GameManager.Instance.GetCurrentSettings();
            studentsController = GetComponent<StudentsController>();
            studentsPositions = DeskManager.Instance.gameObject.transform;

            if (studentsPositions.childCount == 0)
            {
                if (settings.StructureMode == StructureMode.Circular)
                    DeskManager.Instance.CreateCircle();
                else if (settings.StructureMode == StructureMode.U)
                    DeskManager.Instance.CreateUShape();
                else
                    DeskManager.Instance.CreateDesks();
            }

            asientosOcupados = new bool[studentsPositions.childCount];
            students = new Dictionary<string, Student>();
            problematicStudents = new HashSet<string>();
            classInfo = GameManager.Instance.GetCurrentClassInfo();
            names = new List<List<string>>();
            names.Add(classInfo.girlsNames.ToList());
            names.Add(classInfo.boysNames.ToList());
            prefabBodys = new List<GameObject[]>();
            prefabBodys.Add(classInfo.girlsPrefabs);
            prefabBodys.Add(classInfo.boysPrefabs);
            GenerateChilds();

            studentsController.SetParameters(students);

            GetComponent<AudioSource>().clip = beforeClassBell;
            GetComponent<AudioSource>().Play();
            AudioRecorder.StartRecording();
        }

        private void OnApplicationQuit()
        {
            AudioRecorder.SaveRecording();
        }

        private void GenerateChilds()
        {
            int deskPos = 0;
            if (settings.Mode == GenerateMode.Gender)
            {
                GeneratePersonalizedChildWithGender(ref deskPos, (int)Gender.Women, settings.NumWomen);
                GeneratePersonalizedChildWithGender(ref deskPos, (int)Gender.Men, settings.NumMen);
            }
            else
            {
                int randomStudents = settings.NumStudents;

                if (settings.Mode == GenerateMode.Personalizado)
                {
                    GeneratePersonalizedChild(ref deskPos);
                    randomStudents -= deskPos;
                }

                for (int i = 0; i < randomStudents && deskPos < 30; i++)
                {
                    int gender = Random.Range(0, 2);
                    int indexName = Random.Range(0, names[gender].Count);
                    Student pickedStudent = CreateStudent(prefabBodys[gender][Random.Range(0, prefabBodys[gender].Length)], names[gender][indexName], (Gender)gender);
                    names[gender].RemoveAt(indexName);
                    PlaceStudent(ref deskPos, pickedStudent, 1);
                    deskPos++;
                }
            }

            PlayAnimationsAtDifferentTimeClass(classInfo.idleAnim.name);
        }

        private Student CreateStudent(GameObject body, string name, Gender gender)
        {
            Student pickedStudent = Instantiate(prefabStudent, transform);
            pickedStudent.SetParameters(name, gender);
            pickedStudent.CreateBody(body);
            students.Add(name, pickedStudent);
            return pickedStudent;
        }
        private void PlaceStudent(ref int deskPos, Student pickedStudent, int nGruops)
        {
            DeskManager.Instance.GetFreeDesk(ref deskPos, nGruops);
            Desk desk = studentsPositions.GetChild(deskPos).GetComponent<Desk>();
            Transform pos = desk.transform.GetChild(0);
            pickedStudent.transform.SetPositionAndRotation(pos.position, pos.rotation);
            pickedStudent.SetDesk(desk);
            pickedStudent.SetTargets(targetsHead);

            asientosOcupados[deskPos] = true;
        }

        private void GeneratePersonalizedChildWithGender(ref int deskPos, int gender, int n)
        {
            for (int i = 0; i < n; i++)
            {
                int indexName = Random.Range(0, names[gender].Count);
                Student pickedStudent = CreateStudent(prefabBodys[gender][Random.Range(0, prefabBodys[gender].Length)], names[gender][indexName], (Gender)gender);
                names[gender].RemoveAt(indexName);
                PlaceStudent(ref deskPos, pickedStudent, 1);
                deskPos++;
            }
        }

        private void GeneratePersonalizedChild(ref int deskPos)
        {
            var list = settings.Students;

            for (int i = 0; i < list.Length; i++)
            {
                StudentInfo info = list[i];
                Gender gen = (Gender)GetEnumValue<GenderInfo>((int)info.Gender);
                int nBody = GetEnumValue<BodyInfo>((int)info.Body);
                GameObject body = gen == Gender.Men ? classInfo.boysPrefabs[nBody] : classInfo.girlsPrefabs[nBody];
                Student pickedStudent = CreateStudent(body, info.Name, gen);
                PlaceStudent(ref deskPos, pickedStudent, 1);
                deskPos++;
            }
        }

        private int GetEnumValue<T>(int value)
        {
            int length = System.Enum.GetNames(typeof(T)).Length - 1;
            return value == 0 ? Random.Range(0, length) : value - 1;
        }

        private void PlayAnimationsAtDifferentTimeClass(string animName)
        {
            float time = 0.0f;
            foreach (Student s in students.Values)
            {
                time = time + 1.0f / 8.0f;
                s.transform.GetChild(s.transform.childCount - 1).GetComponent<Animator>().Play(animName, 0, time);
            }
        }

        private void StartScene()
        {
            string alumsName = "";

            for (int i = 0; i < problematicStudents.Count; i++)
            {
                if (i > 0 && i != problematicStudents.Count - 1)
                    alumsName += ", ";
                else if (i > 0 && i == problematicStudents.Count - 1)
                    alumsName += " y ";

                alumsName += problematicStudents.ElementAt(i);

                if (i > 1 && i == problematicStudents.Count - 1)
                    alumsName += ";";
            }

            GameObject player = GameManager.Instance.GetPlayer();
            ScenePackage sceneInfo = GameManager.Instance.GetChosenPackage();
            player.GetComponent<AudioSource>().clip = sceneInfo.contextClip;
            player.GetComponent<AudioSource>().Play();
            string t = sceneInfo.initialMessage.Replace("alum", alumsName);
            Debug.Log(t);
            studentsController.SetParameters(students);
        }

        public Student[] GetStudents()
        {
            return students.Values.ToArray();
        }

        public StudentsController GetStudentsController()
        {
            return studentsController;
        }
    }

}

