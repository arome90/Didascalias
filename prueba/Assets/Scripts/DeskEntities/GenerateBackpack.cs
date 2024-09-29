using UnityEngine;



namespace ClassRoomVR
{
    /// <summary>
    /// Clase encargada de gestionar la activación de un objeto hijo aleatorio en la mochila, desactivando o destruyendo los demás.
    /// </summary>
    public class GenerateBackpack : MonoBehaviour
    {
        private void Start()
        {
            ActivateRandomChild();
        }

        /// <summary>
        /// Activa un hijo aleatorio de este GameObject y destruye los otros.
        /// </summary>
        private void ActivateRandomChild()
        {
            int childCount = transform.childCount;

            // Si no hay hijos, salir del método
            if (childCount == 0) return;

            int randomIndex = Random.Range(0, childCount);

            for (int i = 0; i < childCount; i++)
            {
                Transform child = transform.GetChild(i);
                if (i == randomIndex)
                {
                    child.gameObject.SetActive(true); // Activar el hijo seleccionado
                }
                else
                {
                    Destroy(child.gameObject); // Destruir los hijos no seleccionados
                }
            }
        }
    }
}