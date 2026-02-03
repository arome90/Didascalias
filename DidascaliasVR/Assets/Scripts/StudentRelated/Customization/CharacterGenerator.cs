using System.Collections.Generic;
using UnityEngine;

namespace ClassRoomVR
{
    /// <summary>
    /// Generador de personajes para la simulación de aula VR.
    /// </summary>
    public class CharacterGenerator : MonoBehaviour
    {
        [Header("Huesos del Estudiante")]
        [SerializeField] private string[] _extraBonesBody; // Huesos adicionales para el cuerpo

        [Header("Meshes")]
        [SerializeField] private CharacterSkinnedMeshes _characterAssets; // Activos de malla del personaje

        [SerializeField]
        private Transform _rootBone; // Hueso raíz

        [SerializeField] Vector3 _boundCenter;
        [SerializeField] Vector3 _boundExtents;

        [SerializeField] Avatar _animatorAvatar;

        private Student _student; // Referencia al componente Student
        private Transform[] _playerBonesArray; // Array de huesos del jugador
        private Dictionary<string, Transform> _playerBonesDict; // Diccionario de huesos del jugador

        private void Awake()
        {
            InitializePlayerBonesAndDictionary();
        }

        /// <summary>
        /// Inicializa el array y diccionario de huesos del jugador.
        /// </summary>
        private void InitializePlayerBonesAndDictionary()
        {
            if (transform.childCount > 0)
            {
                if(!_rootBone) _rootBone = transform.GetChild(0);
                var bonesList = new List<Transform> { _rootBone };
                PopulateBonesList(_rootBone, bonesList);
                _playerBonesArray = bonesList.ToArray();
                _playerBonesDict = new Dictionary<string, Transform>();

                foreach (var bone in _playerBonesArray)
                {
                    if (!_playerBonesDict.ContainsKey(bone.name))
                    {
                        _playerBonesDict.Add(bone.name, bone);
                    }
                }
            }
        }

        /// <summary>
        /// Rellena la lista de huesos recursivamente.
        /// </summary>
        /// <param name="root">Hueso raíz.</param>
        /// <param name="bonesList">Lista de huesos.</param>
        private void PopulateBonesList(Transform root, List<Transform> bonesList)
        {
            foreach (Transform child in root)
            {
                bonesList.Add(child);
                PopulateBonesList(child, bonesList);
            }
        }

        private void Start()
        {
            _student = GetComponent<Student>();
            // GetComponent<Animator>().avatar = _animatorAvatar;
            SetupCharacterMeshes();
        }

        /// <summary>
        /// Configura los meshes del personaje según el género y los complementos.
        /// </summary>
        private void SetupCharacterMeshes()
        {
            var bodies = _student.Gender == Gender.Girl ? _characterAssets.Bodies.Women : _characterAssets.Bodies.Men;
            var selectedBody = bodies[Random.Range(0, bodies.Length)];

            int hairColor = Random.Range(0, selectedBody.HairMat.Length);
            AttachMesh(selectedBody.Body, selectedBody.BodyMat, selectedBody.HairMat[hairColor], _rootBone);

            if (selectedBody.Hair != null )
            {
                AttachMesh(selectedBody.Hair[Random.Range(0, selectedBody.Hair.Length)], null, selectedBody.HairMat[hairColor], _rootBone);
               // AdjustBonesPosition(selectedBody.Hair, new[] { "Bip001Hair01", "Bip001Hair02", "Bip001Hair03" });
            }

            AdjustBonesPosition(selectedBody.Body, _extraBonesBody);

            foreach (var category in _characterAssets.Categories)
            {
                if (category.Items.Count > 0)
                {
                    var selectedItem = category.Items[Random.Range(0, category.Items.Count)];
                    AttachClothing(selectedItem.Cloth, selectedItem.Materials[Random.Range(0, selectedItem.Materials.Length)], selectedBody.BodyMat, selectedItem.Colors);
                }
            }

            //Modelar cara aleatoriamente 
            RandomFaceModeling(selectedBody.Body, -100, 100);

            SetExpressionBlendShape(selectedBody.Body, Expressions.Smile, 100.0f);
            

            // var complement = selectedBody.HeadBone.Complements[Random.Range(0, selectedBody.HeadBone.Complements.Count)];
            //    int colorIndex = Random.Range(0, complement.color.Length);

            //    CharacterPropsSpawner.TrySpawnComplement(_rootBone, selectedBody.HeadBone, complement, colorIndex, false);
        }

        /// <summary>
        /// Adjunta un mesh al hueso objetivo.
        /// </summary>
        /// <param name="mesh">Mesh a adjuntar.</param>
        /// <param name="bodyMat">Material del cuerpo.</param>
        /// <param name="hairMat">Material del pelo.</param>
        /// <param name="targetBone">Hueso objetivo.</param>
        private void AttachMesh(SkinnedMeshRenderer mesh, Material bodyMat, Material hairMat, Transform targetBone)
        {
            var newMesh = Instantiate(mesh, targetBone.position, Quaternion.identity);
            newMesh.bones = SetupBones(mesh.bones);
            newMesh.rootBone = targetBone;
            newMesh.transform.SetParent(transform, false);

            if (bodyMat != null && hairMat != null)
            {
                newMesh.materials = new[] { bodyMat, hairMat };
            }
            else if (hairMat != null)
                newMesh.materials = new[] { hairMat };

            newMesh.localBounds = new Bounds(_boundCenter, _boundExtents);
        }

        /// <summary>
        /// Adjunta una prenda de ropa al personaje.
        /// </summary>
        /// <param name="mesh">Mesh de la ropa.</param>
        /// <param name="bodyMat">Material del cuerpo.</param>
        /// <param name="colors">Colores disponibles.</param>
        private void AttachClothing(SkinnedMeshRenderer mesh, Material clothMat, Material bodyMat, Color[] colors)
        {
            var newMesh = Instantiate(mesh);
            newMesh.bones = SetupBones(mesh.bones);
            newMesh.rootBone = _rootBone;
            newMesh.transform.SetParent(_rootBone.parent, false);

            int colorIndex = Random.Range(0, colors.Length);
            newMesh.materials = new[] { clothMat, bodyMat };
            newMesh.materials[0].SetColor("_Color", colors[colorIndex]);

            newMesh.localBounds = new Bounds(_boundCenter, _boundExtents);
        }

        /// <summary>
        /// Configura los huesos según el array de huesos proporcionado.
        /// </summary>
        /// <param name="bones">Array de huesos.</param>
        /// <returns>Array de huesos configurados.</returns>
        private Transform[] SetupBones(Transform[] bones)
        {
            var newBones = new Transform[bones.Length];

            for (int i = 0; i < bones.Length; i++)
            {
                _playerBonesDict.TryGetValue(bones[i].name, out newBones[i]);
            }

            return newBones;
        }

        /// <summary>
        /// Ajusta la posición y rotación de los huesos especificados.
        /// </summary>
        /// <param name="mesh">Mesh utilizado.</param>
        /// <param name="boneNames">Nombres de los huesos a ajustar.</param>
        private void AdjustBonesPosition(SkinnedMeshRenderer mesh, string[] boneNames)
        {
            foreach (var boneName in boneNames)
            {
                if (_playerBonesDict.TryGetValue(boneName, out var targetBone))
                {
                    var sourceBone = FindBoneByName(mesh, boneName);

                    if (sourceBone != null)
                    {
                        targetBone.localPosition = sourceBone.localPosition;
                        targetBone.localRotation = sourceBone.localRotation;
                    }
                }
            }
        }

        /// <summary>
        /// Busca un hueso por nombre dentro del mesh.
        /// </summary>
        /// <param name="mesh">Mesh a buscar.</param>
        /// <param name="boneName">Nombre del hueso a encontrar.</param>
        /// <returns>Transform del hueso encontrado, o null si no se encuentra.</returns>
        private Transform FindBoneByName(SkinnedMeshRenderer mesh, string boneName)
        {
            foreach (var bone in mesh.bones)
            {
                if (bone.name == boneName)
                {
                    return bone;
                }
            }
            return null;
        }
        public void RandomFaceModeling(SkinnedMeshRenderer mesh, float minValue, float maxValue)
        {
            for (int i = (int)Expressions.EXPRESSIONS_SIZE; i < (int)ModelingProperties.MODELING_PROPERTIES_SIZE; i++)
            {
                SetModelBlendShape(mesh, (ModelingProperties)i, Random.Range(minValue, maxValue));
            }
        }
        public void SetModelBlendShape(SkinnedMeshRenderer mesh, ModelingProperties prop, float value)
        {
            mesh.SetBlendShapeWeight((int)prop, value);
        }

        public void SetExpressionBlendShape(SkinnedMeshRenderer mesh, Expressions expression, float value)
        {
            mesh.SetBlendShapeWeight((int)expression, value);
        }
    }
}
