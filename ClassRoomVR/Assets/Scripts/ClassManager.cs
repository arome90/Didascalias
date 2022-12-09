using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace ClassRoomVR
{
    public class ClassManager : MonoBehaviour
    {
        [SerializeField]
        Transform studentsPositions ;

        [SerializeField]
        private Student prefabStudent;

        private Student[] _students;
        private bool[] _asientosOcupados;
        int[] _problematicStudents;

        public Transform frontSide;
        public Transform backCorner;

        private void Start()
        {
            generateChilds();
            StartScene();
        }
        private void generateChilds()
        {
            ScenePackage sceneInfo = GameManager.Instance.getPack();
            ClassInfo classInfo = GameManager.Instance.getClass();

            List<List<string>> names = new List<List<string>>();
            names.Add(classInfo.girlsNames.ToList());
            names.Add(classInfo.boysNames.ToList());
            List<GameObject[]> prefabBodys = new List<GameObject[]>();
            prefabBodys.Add(classInfo.girlsPrefabs); 
            prefabBodys.Add(classInfo.boysPrefabs);


            if (sceneInfo.nStudents > 30) sceneInfo.nStudents = 30;
            _asientosOcupados = new bool[studentsPositions.childCount];

            _students = new Student[sceneInfo.nStudents];
            int[] _studentsSex = new int[sceneInfo.nStudents];

            int deskPos = 0;

            // Instanciamos los alumnos en sus posiciones de manera aleatoria (el prefab).
            for (int i = 0; i < sceneInfo.nStudents && deskPos < 30; i++)
            {
                // Elegimos el sexo del estudiante
                Student pickedStudent;
                int sex = UnityEngine.Random.Range(0, 2); // 0 mujer, 1 hombre
                int indexName = UnityEngine.Random.Range(0, names[sex].Count);
                string name = names[sex][indexName];
                names[sex].RemoveAt(indexName);
                pickedStudent = Instantiate(prefabStudent, transform);
                pickedStudent.SetParameters(name, sex);
                pickedStudent.CreateBody(prefabBodys[sex][UnityEngine.Random.Range(0, prefabBodys[sex].Length)]);
              

                // Ordenamiento por grupos
                if (sceneInfo.nGroups > 1)
                {
                    if (deskPos == 2 || deskPos == 7 || deskPos == 12 || deskPos == 17 || deskPos == 22 || deskPos == 27)
                        deskPos++;
                    if (deskPos == 10 || deskPos == 11 || deskPos == 12 || deskPos == 13 || deskPos == 14)
                        deskPos = 15;
                }

                // Lo colocamos en su pupitre
                Transform pos = studentsPositions.GetChild(deskPos);
                Vector3 position = pos.position + new Vector3(0, -0.4f, 0);
                pickedStudent.transform.SetPositionAndRotation(position, pos.rotation);
                pickedStudent.SetDesk(position);

                // Lo añadimos al array de estudiantes
                _students[i] = pickedStudent;
                _asientosOcupados[deskPos] = true;
                _studentsSex[i] = sex;
                deskPos++;
            }

            // Estudiantes problematicos
             _problematicStudents = new int[sceneInfo.problematicStudents];
            int problematic = -1;
            for (int i = 0; i < sceneInfo.problematicStudents; i++)
            {
                // En caso de que se tengan que sentar juntos
                if (sceneInfo.problematicTogether)
                {
                    // Condicion inicial para el primero
                    if (problematic == -1)
                    {
                        problematic = UnityEngine.Random.Range(0, sceneInfo.nStudents);

                        // FEISIMO (para evitar errores de generacion)
                        if (problematic == 4) problematic -= 1;
                    }
                    // Colocacion de los demas alrededor del anterior problematico
                    else
                    {
                        int a = -1;
                        do
                        {
                            a = UnityEngine.Random.Range(0, 4);
                            switch (a)
                            {
                                case 0:
                                    a = 1;
                                    break;
                                case 1:
                                    a = -1;
                                    break;
                                case 2:
                                    a = 5;
                                    break;
                                case 3:
                                    a = -5;
                                    break;
                                default:
                                    break;
                            }
                        } while (problematic + a > sceneInfo.nStudents - 1 || problematic + a < 0);

                        problematic += a;
                    }
                }
                else
                {
                    problematic = UnityEngine.Random.Range(0, sceneInfo.nStudents);
                }
                _problematicStudents[i] = problematic;
                _students[problematic].SetProblematicStudent();
            }   // end estudiantes problematicos

            //probIniPos = _students[_problematicStudents[0]].gameObject.transform.position;

            // Ejecutamos animaciones con distinto timing
            PlayAnimationsAtDifferentTimeClass(classInfo.idleAnim.name);
        }

        private void PlayAnimationsAtDifferentTimeClass(string animName)
        {
            // Play animations at different time
            float time = 0.0f;
            foreach (Student s in _students)
            {
                time = time + 1f / 8;
                s.transform.GetChild(s.transform.childCount-1).GetComponent<Animator>().Play(animName, 0, time);
            }
        }

        public void StartScene() 
        {
            string alumsName = "";
            // Mostramos el texto descriptivo de la escena
            for (int i = 0; i < _problematicStudents.Length; i++)
            {
                if (i > 0 && i != _problematicStudents.Length - 1) alumsName += ", ";
                else if (i > 0 && i == _problematicStudents.Length - 1) alumsName += " y ";
                alumsName += _students[_problematicStudents[i]].name;
                if (i > 1 && i == _problematicStudents.Length - 1) alumsName += ";";
            }
            GameObject player = GameManager.Instance.GetPlayer();
            ScenePackage sceneInfo = GameManager.Instance.getPack();
            player.GetComponent<AudioSource>().clip = sceneInfo.contextClip;
            player.GetComponent<AudioSource>().Play();
            string t = sceneInfo.iniMessage.Replace("alum", alumsName);
            Debug.Log(t);
            //uiManager.panelContexto(t);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B)) 
            {
                _students[_problematicStudents[0]].MoveTo(frontSide.position);
            }
            else if (Input.GetKeyDown(KeyCode.N)) 
            {
                _students[_problematicStudents[0]].SitBack();
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                _students[_problematicStudents[0]].MoveTo(backCorner.position);
            }
            else if (Input.GetKeyDown(KeyCode.K))
            {
                Vector3 pos1= _students[_problematicStudents[0]].GetDesk();
                Vector3 pos2= _students[_problematicStudents[1]].GetDesk();
                _students[_problematicStudents[0]].ChangeDesk(pos2);
                _students[_problematicStudents[1]].ChangeDesk(pos1);
               
            }
        }
        public Student[] GetStudents() { return _students; }
    }
}
