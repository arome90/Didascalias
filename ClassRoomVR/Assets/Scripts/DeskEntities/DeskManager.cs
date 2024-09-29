using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Clase que gestiona la creación y organización de los escritorios en el aula.
    /// Permite la creación de distribuciones regulares o circulares.
    /// </summary>
    public class DeskManager : GenericSingleton<DeskManager>
    {
        [SerializeField] private Desk _deskPrefab; // Prefab de escritorio
        private List<Vector2> _deskPositions; // Lista de posiciones de los escritorios
        private List<Desk> _desks; // Lista de instancias de los escritorios

        [SerializeField] private float _deskOffsetX; // Offset de X de los escritorios
        [SerializeField] private float _deskOffsetZ; // Offset de Z de los escritorios
        [SerializeField] private float _deskOffsetO; // Otro offset (no especificado)
        public float DeskOffsetX=> _deskOffsetX;
        public float DeskOffsetZ=> _deskOffsetZ;
        public float DeskOffsetO => _deskOffsetZ;
        /// <summary>
        /// Propiedad para obtener la lista de posiciones de los escritorios.
        /// </summary>
        public List<Vector2> DeskPositions => _deskPositions;

        /// <summary>
        /// Propiedad para obtener la lista de escritorios instanciados.
        /// </summary>
        public List<Desk> Desks => _desks;

        public override void Awake()
        {
            base.Awake();
            _deskPositions = new List<Vector2>();
            _desks = new List<Desk>();
        }

        /// <summary>
        /// Obtiene el índice de un escritorio libre.
        /// </summary>
        /// <param name="deskPosition">Referencia al índice del escritorio que se desea comprobar.</param>
        public void GetFreeDesk(ref int deskPosition)
        {
            for (int i = deskPosition; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).gameObject.activeSelf)
                {
                    deskPosition = i;
                    return;
                }
            }
        }

        /// <summary>
        /// Crea un layout regular de escritorios en una cuadrícula.
        /// </summary>
        /// <param name="numDesks">Número de escritorios a crear.</param>
        /// <param name="numRows">Número de filas.</param>
        /// <param name="numColumns">Número de columnas.</param>
        public void CreateRegularLayout(int numDesks, int numRows, int numColumns)
        {
            _deskPositions.Clear();
            _desks.Clear();
            DestroyChildren();

            int iDesk = 0;
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numColumns; j++)
                {
                    if (numDesks == iDesk)
                    {
                        return;
                    }

                    float xPos = j - (numColumns - 1) / 2f;
                    float zPos = -i + (numRows - 1) / 2f;
                    _deskPositions.Add(new Vector2(xPos, zPos));

                    Desk desk = Instantiate(_deskPrefab, new Vector3(transform.position.x + xPos * _deskOffsetX, transform.position.y, transform.position.z + zPos * _deskOffsetZ), Quaternion.identity, transform);
                    desk.DeskId = iDesk;
                    _desks.Add(desk);
                    iDesk++;
                }
            }
        }

        /// <summary>
        /// Crea un layout circular de escritorios.
        /// </summary>
        /// <param name="numDesks">Número de escritorios a crear.</param>
        /// <param name="radius">Radio del círculo.</param>
        /// <param name="degrees">Ángulo de los escritorios en el círculo (por defecto es 180 grados).</param>
        public void CreateCircle(int numDesks, float radius, float degrees = 180f)
        {
            _deskPositions.Clear();
            _desks.Clear();
            DestroyChildren();

            float angle = degrees / (numDesks - (degrees > 180 ? 0 : 1));
            for (int i = 0; i < numDesks; i++)
            {
                float xPos = Mathf.Cos(Mathf.Deg2Rad * angle * i) * radius;
                float zPos = Mathf.Sin(Mathf.Deg2Rad * angle * i) * radius;
                _deskPositions.Add(new Vector2(xPos, zPos));

                Vector3 position = new Vector3(xPos, 0, -zPos) + transform.position;
                Desk desk = Instantiate(_deskPrefab, position, Quaternion.identity, transform);

                if (degrees > 180)
                {
                    desk.transform.LookAt(transform.position); // Gira el escritorio hacia el centro si es necesario
                }

                desk.DeskId = i;
                _desks.Add(desk);
            }
        }

        /// <summary>
        /// Destruye todos los hijos del transform, eliminando todos los escritorios.
        /// </summary>
        public void DestroyChildren()
        {
            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Destruye los hijos inactivos del transform.
        /// </summary>
        public void DestroyInactiveChildObjects()
        {
            foreach (Transform child in transform)
            {
                if (!child.gameObject.activeSelf)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Obtiene el Renderer del colisionador del escritorio (para detectar colisiones).
        /// </summary>
        /// <returns>Renderer del colisionador del escritorio.</returns>
        public Renderer GetDeskCollider()
        {
            return _deskPrefab.transform.GetChild(1).GetChild(0).GetComponent<SkinnedMeshRenderer>();
        }
    }
}
