using UnityEngine;
using System.Collections.Generic;



namespace ClassRoomVR
{
    /// <summary>
    /// Clase encargada de gestionar la activación de un objeto hijo aleatorio en la mochila, desactivando o destruyendo los demás.
    /// </summary>
    public class GenerateBackpack : MonoBehaviour
    {
        [SerializeField] private List<GameObject> children;
        private void Start()
        {
            ActivateRandomChild();
        }

        /// <summary>
        /// Activa un hijo aleatorio de este GameObject y destruye los otros.
        /// </summary>
        public void ActivateRandomChild()
        {
            if (children == null || children.Count == 0)
            {
                Debug.LogWarning("No hay hijos asignados en la lista.");
                return;
            }

            int randomIndex = Random.Range(0, children.Count);

            for (int i = 0; i < children.Count; i++)
            {
                if (i == randomIndex)
                {
                    children[i].SetActive(true);
                }
                else
                {
                    Destroy(children[i]);
                }
            }
        }
    }
}