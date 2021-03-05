using UnityEngine;

namespace ClassRoomVR {
    public class Scene2 : MonoBehaviour
    {
        public void separateProblematics()
        {
            MySceneManager sm = GameManager.Instance._sceneManager;
            GameObject[] problematics = sm.getProblematics();
            GameObject schoolClass = sm.getClass();
            bool[] asientosOcupados = sm.getFreeDesks();

            bool done = false;

            // Hacer que uno de los alumnos se separe


            sm.setSpecialSituation(done);
        }
    }
}
