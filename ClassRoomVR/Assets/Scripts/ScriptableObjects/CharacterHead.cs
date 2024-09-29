using UnityEngine;
using static CharacterProps;

/// <summary>
/// Configuración del aspecto del personaje, incluyendo cuerpo, pelo y materiales.
/// </summary>
[CreateAssetMenu(fileName = "CharacterSkin", menuName = "Character Assets/Character Skin", order = 2)]
public class CharacterSkin : ScriptableObject
{
    [Header("Cuerpo")]
    [SerializeField] private SkinnedMeshRenderer _body;       // Mesh con animación para el cuerpo
    [SerializeField] private Material _bodyMat;               // Material del cuerpo

    [Header("Pelo")]
    [SerializeField] private SkinnedMeshRenderer[] _hair;       // Mesh con animación para el pelo
    [SerializeField] private Material[] _hairMat;               // Material del pelo

    [Header("Complementos de la cabeza")]
    [SerializeField] private CharacterProps.ComplementAttachment _headBone;  // Configuración del hueso para los complementos de la cabeza

    public SkinnedMeshRenderer Body => _body;
    public Material BodyMat => _bodyMat;
    public SkinnedMeshRenderer[] Hair => _hair;
    public Material[] HairMat => _hairMat;
    public CharacterProps.ComplementAttachment HeadBone => _headBone;


}

