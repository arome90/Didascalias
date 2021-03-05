using UnityEngine;

namespace ClassRoomVR {
    public class Scene3 : MonoBehaviour
    {
        // Metodo de la situacion
        public void walkAwayFromGroup()
        {
            MySceneManager sm = GameManager.Instance._sceneManager;
            GameObject[] problematics = sm.getProblematics();
            GameObject schoolClass = sm.getClass();


            bool done = false;

            // Hacer que uno de los alumnos se separe


            sm.setSpecialSituation(done);
        }

        // Metodo path1
        public void sitBack()
        {
            MySceneManager sm = GameManager.Instance._sceneManager;
            bool done = false;

            // Hacer que el alumno vuelva a su sitio


            sm.setSpecialPath(done);
        }

        // Metodo path2
        public void goEndOfclass()
        {
            MySceneManager sm = GameManager.Instance._sceneManager;
            bool done = false;

            // Hacer que el alumno vaya al final de la clase


            sm.setSpecialPath(done);
        }
    }
}
