using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace ClassRoomVR
{
    /// <summary>
    /// Controla las animaciones del escritorio y la silla, así como la interacción con otros escritorios.
    /// </summary>
    public class Desk : MonoBehaviour
    {
        [SerializeField] private Animation _deskAnimation; // Referencia a la animación del escritorio
        [SerializeField] private Animation _chairAnimation; // Referencia a la animación de la silla
        [SerializeField] private NavMeshObstacle _chairObstacle; // Referencia al obstáculo de la silla

        private int _deskId; // Número de identificación del escritorio

        /// <summary>
        /// Propiedad que expone el ID del escritorio.
        /// </summary>
        public int DeskId
        {
            get => _deskId;
            set => _deskId = value;
        }

        /// <summary>
        /// Evento que se invoca cuando ocurre una colisión con otro escritorio.
        /// </summary>
        [HideInInspector] public UnityEngine.Events.UnityEvent OnCollisionChanged { get; private set; }

        private List<string> _deskAnimationClipNames; // Lista con los nombres de los clips de animación del escritorio

        /// <summary>
        /// Método que se ejecuta al despertar el objeto. Inicializa la lista de nombres de clips de animación.
        /// </summary>
        private void Awake()
        {
            // Inicializa la lista de nombres de clips de animación
            _deskAnimationClipNames = new List<string>();
            OnCollisionChanged = new UnityEngine.Events.UnityEvent();
        }

        /// <summary>
        /// Método que se ejecuta al iniciar el objeto. Llena la lista de clips de animación si existe la animación del escritorio.
        /// </summary>
        private void Start()
        {
            // Llena la lista de nombres de clips de animación del escritorio
            if (_deskAnimation != null)
            {
                foreach (AnimationState animationState in _deskAnimation)
                {
                    _deskAnimationClipNames.Add(animationState.name);
                }
            }
        }

        /// <summary>
        /// Obtiene la posición del estudiante que está sentado en el escritorio.
        /// </summary>
        /// <returns>Vector3 con la posición del estudiante.</returns>
        public Vector3 GetStudentPosition() => transform.GetChild(0).position + new Vector3(0, 0, 0.05f);

        /// <summary>
        /// Invoca el evento de colisión si el otro objeto tiene la etiqueta "Desk".
        /// </summary>
        /// <param name="other">El collider del objeto con el que colisiona.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Desk"))
            {
                OnCollisionChanged?.Invoke(); // Invoca el evento de forma segura
            }
        }

        /// <summary>
        /// Reproduce la animación del escritorio según el clip especificado.
        /// </summary>
        /// <param name="anim">Enum que representa el clip de animación a reproducir.</param>
        public void PlayAnimacionMesa(Animaciones anim)
        {
            if (_deskAnimation != null)
            {
                _deskAnimation.Play(_deskAnimationClipNames[(int)anim]);
            }
        }

        /// <summary>
        /// Reproduce la animación de la silla.
        /// </summary>
        public void RockChair()
        {
            if (_chairAnimation != null)
            {
                _chairAnimation.Play();
            }
        }

        /// <summary>
        /// Activa o desactiva el obstáculo de la silla en el NavMesh.
        /// </summary>
        /// <param name="active">Booleano que indica si el obstáculo debe estar activo o no.</param>
        public void SetChairActive(bool active)
        {
            if (_chairObstacle != null)
            {
                _chairObstacle.gameObject.SetActive(active);
            }
        }
    }
}