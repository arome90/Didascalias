using System;
using UnityEngine;
using UnityEngine.UI;

namespace ClassRoomVR {
    public class MySceneManager : MonoBehaviour {
        // Publics
        //public GameManager _gm;

        // Privates
        private GameObject teacher;
        private Transform studentsPositions;

        //-------------
        // Cosas del gm
        [Tooltip("Esto se lo pasa el gm en funcion del nivel elegido")]
        public ScenePackage sceneInfo; // En realidad es private
        [Tooltip("Esto lo coge del gm")]
        public ClassInfo classInfo; // En realidad es private
        
        public Canvas canvas; // En realidad lo coge del gm, no existe aqui

        // Start is called before the first frame update
        void Start() {
            // Generacion de la clase
            Instantiate(classInfo.clase, transform);
            // Temporales, deberiamos poner un punto de aparicion para el teacher en el prefab de la clase
            teacher = Instantiate(classInfo.teacher, transform);
            teacher.transform.position = classInfo.clase.GetComponentInChildren<Transform>().Find("TeacherDesk").position + new Vector3(0, 1.5f, 1);
            teacher.transform.Rotate(new Vector3(0, 180, 0));

            // Generamos los chavales
            studentsPositions = classInfo.clase.GetComponentInChildren<Transform>().Find("Desks").GetComponentInChildren<Transform>().Find("DeskPositions");
            generateChilds();

            // Mostramos el texto descriptivo de la escena (ademas del boton obvio :P)
            // Obviamente esto no es asi :), 
            // Tendriamos una referencia al canvas y de ahi cogemos el texto que sea el bueno 
            Text iniText = gameObject.AddComponent<Text>(); 
            iniText.text = sceneInfo.iniMessage;
        }

        // Update is called once per frame
        void Update() {}



        //-------------------PRIVATES-------------------------
        private void generateChilds() {
            if (sceneInfo.nGroups > 1)
            {
                // Colocar a los chavales en grupos
                int nPupitres = studentsPositions.childCount;
            }

            if (sceneInfo.nStudents > 30) sceneInfo.nStudents = 30;

            for (int i = 0; i < sceneInfo.nStudents; i++) {
                // Instanciamos los alumnos en sus posiciones de manera aleatoria.

                // Elegimos el sexo del estudiante
                GameObject pickedStudent;
                int sex = UnityEngine.Random.Range(0, 2);
                if (sex == 0) {
                    pickedStudent = Instantiate(classInfo.girlsPrefabs[UnityEngine.Random.Range(0, classInfo.girlsPrefabs.Length)], transform);
                    pickedStudent.name = classInfo.girlsNames[UnityEngine.Random.Range(0, classInfo.girlsNames.Length)];
                }
                else
                {
                    pickedStudent = Instantiate(classInfo.boysPrefabs[UnityEngine.Random.Range(0, classInfo.boysPrefabs.Length)], transform);
                    pickedStudent.name = classInfo.boysNames[UnityEngine.Random.Range(0, classInfo.boysNames.Length)];
                }

                // Le ponemos el nombre
                pickedStudent.GetComponentInChildren<Transform>().Find("Name").GetComponent<TextMesh>().text = pickedStudent.name;

                // TODO: falta el ordenamiento por grupos ;)
                // Lo colocamos en su pupitre
                Transform pos = studentsPositions.GetComponent<Transform>().GetChild(i);
                pickedStudent.transform.SetPositionAndRotation(pos.position + new Vector3(0, -0.4f, 0), pos.rotation);
            }
        }
    }
}