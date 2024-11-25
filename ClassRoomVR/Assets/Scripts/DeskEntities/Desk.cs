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
        public enum MATERIAL_STATE
        {
            BOOK_OPENED,
            NOTEBOOK_OPENED,
            // esto seguro que se puede hacer con cosas de bits y sumando valores 
            // por ejemplo:
            // BOOK_OPENED = 0b01
            // NOTEBOOK_OPENED = 0b10
            // NONE = 0b00
            // Entonces para comprobar si algo está abierto, se hace con la máscara de bits correspondiente.
            // Se verá.
            BOOK_AND_NOTEBOOK_OPENED,

            NONE
        }

        [SerializeField] private Animation _deskAnimation; // Referencia a la animación del escritorio
        [SerializeField] private Animation _chairAnimation; // Referencia a la animación de la silla
        [SerializeField] private NavMeshObstacle _chairObstacle; // Referencia al obstáculo de la silla

        [SerializeField] private MaterialManager _materialManager;

        private int _deskId; // Número de identificación del escritorio

        /// <summary>
        /// Propiedad que expone el ID del escritorio.
        /// </summary>
        public int DeskId
        {
            get => _deskId;
            set => _deskId = value;
        }

        MATERIAL_STATE _state;

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
            foreach (AnimationState animationState in _deskAnimation)
            {
                _deskAnimationClipNames.Add(animationState.name);
            }

            if(!_materialManager)
            {
                // Esto no es muy eficiente para la carga de escenas, intentaremos que siempre está
                // asignado desde el inspector
                _materialManager = GetComponentInChildren<MaterialManager>();
            }
        }

        public bool IsNotebookOpened()
        {
            return (_state == MATERIAL_STATE.NOTEBOOK_OPENED || _state == MATERIAL_STATE.BOOK_AND_NOTEBOOK_OPENED);
        }
        public MATERIAL_STATE getState() { return _state; }

        /// <summary>
        /// Comienza la animación de abrir el libro que está sobre la mesa.
        /// </summary>
        public void OpenBook()
        {
            _state = MATERIAL_STATE.BOOK_OPENED;
            _materialManager.GetBook().Open();
        }

        public void OpenNoteBook()
        {
            if(_state == MATERIAL_STATE.BOOK_OPENED)
            {
                _state = MATERIAL_STATE.BOOK_AND_NOTEBOOK_OPENED;
            }
            else _state = MATERIAL_STATE.NOTEBOOK_OPENED;
            _materialManager.GetNotebook().Open();
        }
        public void CloseBook()
        {
            if (_state == MATERIAL_STATE.BOOK_AND_NOTEBOOK_OPENED)
            {
                _state = MATERIAL_STATE.NOTEBOOK_OPENED;
            }
            else _state = MATERIAL_STATE.NONE;
            _materialManager.GetBook().Close();
        }
        public void CloseNoteBook()
        {
            if (_state == MATERIAL_STATE.BOOK_AND_NOTEBOOK_OPENED)
            {
                _state = MATERIAL_STATE.BOOK_OPENED;
            }
            else _state = MATERIAL_STATE.NONE;
            _materialManager.GetNotebook().Close();
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
        public void PlayDeskAnimation(Animaciones anim)
        {
            if (_deskAnimation != null)
            {
                _deskAnimation.Play(_deskAnimationClipNames[(int)anim]);
            }
        }

        /// <summary>
        /// Reproduce la animación de la silla.
        /// </summary>
        public void PlayChairAnimation()
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