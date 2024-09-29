using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona el menú del juego.
    /// </summary>
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] private GameObject _positionPlayer; // Objeto que define la posición de destino del jugador
        [SerializeField] private GameObject _player; // Objeto del jugador
        private Vector3 _playerInit; // Posición inicial del jugador

        private void Start()
        {
            if (_player != null)
            {
                _playerInit = _player.transform.position;
            }
        }

        /// <summary>
        /// Mueve al jugador a la posición de destino.
        /// </summary>
        public void PlayButton()
        {
            if (_player != null)
            {
                SetPlayerPosition(_positionPlayer.transform.position + Vector3.down / 2.0f, Quaternion.identity);
            }
        }

        /// <summary>
        /// Regresa al jugador a la posición inicial y le aplica una rotación.
        /// </summary>
        public void ReturnButton()
        {
            if (_player != null)
            {
                SetPlayerPosition(_playerInit, Quaternion.Euler(Vector3.down * 90.0f));
            }
        }

        /// <summary>
        /// Sale de la aplicación.
        /// </summary>
        public void QuitButton()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Establece la posición y rotación del jugador.
        /// </summary>
        /// <param name="position">La nueva posición del jugador.</param>
        /// <param name="rotation">La nueva rotación del jugador.</param>
        private void SetPlayerPosition(Vector3 position, Quaternion rotation)
        {
            if (_player != null)
            {
                _player.transform.position = position;
                _player.transform.rotation = rotation;
            }
        }
    }
}
