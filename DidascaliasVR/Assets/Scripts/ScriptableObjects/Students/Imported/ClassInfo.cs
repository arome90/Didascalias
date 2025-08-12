using UnityEngine;

namespace ClassRoomVR
{
    [CreateAssetMenu(fileName = "ClassInfo", menuName = "ScriptableObject/ClassInfo", order = 3)]
    public class ClassInfo : ScriptableObject
    {
        [Header("Class Information Used to Generate Scenes")]

        [Tooltip("Names of male students")]
        [SerializeField] private string[] _maleStudentNames; // Prefijo "_" para variables privadas
        public string[] MaleStudentNames => _maleStudentNames;

        [Tooltip("Names of female students")]
        [SerializeField] private string[] _femaleStudentNames; // Prefijo "_" para variables privadas
        public string[] FemaleStudentNames => _femaleStudentNames;

        /// <summary>
        /// Devuelve la lista combinada de todos los nombres de estudiantes.
        /// </summary>
        public string[] AllStudentNames => CombineStudentNames();

        /// <summary>
        /// Combina las listas de nombres de estudiantes masculinos y femeninos.
        /// </summary>
        private string[] CombineStudentNames()
        {
            string[] allNames = new string[_maleStudentNames.Length + _femaleStudentNames.Length];
            _maleStudentNames.CopyTo(allNames, 0);
            _femaleStudentNames.CopyTo(allNames, _maleStudentNames.Length);
            return allNames;
        }

        /// <summary>
        /// Devuelve un nombre aleatorio de la lista combinada de estudiantes.
        /// </summary>
        public string GetRandomStudentName()
        {
            string[] allNames = AllStudentNames;
            return allNames[Random.Range(0, allNames.Length)];
        }
    }
}
