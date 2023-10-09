using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Customizable : MonoBehaviour
{
    public Skin skin;
    public List<Customization> Customs;
    private List<ICustomizable> Customizations;
   public int _currentCustomizationIndex;
    public ICustomizable CurrentCustomization { get; private set; }

    [HideInInspector] public UnityEvent onValueChanged;



    [ContextMenu("Randomize All")]
    public  void Randomize()
    {
        skin.Randomize();
        foreach (var ele in Customs)
        {
            ele.Randomize();
        }
    }

    public List<ICustomizable> GetList() { return Customizations; }

    public void SetIndex(int i)
    {
        _currentCustomizationIndex = i;
        CurrentCustomization = Customizations[_currentCustomizationIndex];

    }

    public void SetList(List<int> list) 
    {
        for(int i=0;i<list.Count;i++) 
        {
            Customizations[i].SetIndex(list[i]);
            Customizations[i].UpdateCustom();
        }
    }

    void Awake()
    {
        Customizations = new List<ICustomizable>();
        Customizations.Add(skin);
        skin.UpdateCustom();
        foreach (var customization in Customs)
        {
            Customizations.Add(customization);
            customization.UpdateCustom();
        }
    }

    void Update()
    {
        if (transform.gameObject.activeSelf)
        {
            SelectCustomizationWithUpDownArrows();
            bool action = false;
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                CurrentCustomization.Next();
                action = true;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                CurrentCustomization.Previous();
                action = true;
            }
            if (action) onValueChanged.Invoke();
        }

    }
    void SelectCustomizationWithUpDownArrows()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
            _currentCustomizationIndex++;
        if (Input.GetKeyDown(KeyCode.UpArrow))
            _currentCustomizationIndex--;
        if (_currentCustomizationIndex < 0)
            _currentCustomizationIndex = Customizations.Count - 1;
        if (_currentCustomizationIndex >= Customizations.Count)
            _currentCustomizationIndex = 0;
        CurrentCustomization = Customizations[_currentCustomizationIndex];
    }

}


public interface ICustomizable
{
    void Next();
    void Previous();
    void UpdateCustom();
    void Randomize();
    void SetIndex(int i);
    int GetIndex();
}



[System.Serializable]
public class Customization : ICustomizable
{
    public string DisplayName;
    public List<Renderer> Renderers;
    public List<Material> Materials;
    public List<GameObject> SubObjects;
    public int _materialIndex; 
    int _subObjectIndex;

    public void Next()
    {
        _materialIndex++;
        if (_materialIndex >= Materials.Count)
            _materialIndex = 0;

        _subObjectIndex++;
        if (_subObjectIndex >= SubObjects.Count)
            _subObjectIndex = 0;

        UpdateCustom();
    }

    public void Previous()
    {
        _materialIndex--;
        if (_materialIndex < 0)
            _materialIndex = Materials.Count - 1;

        _subObjectIndex--;
        if (_subObjectIndex < 0)
            _subObjectIndex = SubObjects.Count - 1;

        UpdateCustom();
    }

    public void UpdateCustom()
    {
        for (var i = 0; i < SubObjects.Count; i++)
            if (SubObjects[i])
                SubObjects[i].SetActive(i == _subObjectIndex);

        foreach (var renderer in Renderers)
            if (renderer)
                renderer.material = Materials[_materialIndex];
    }
    public void Randomize()
    {
        _subObjectIndex = Random.Range(0, SubObjects.Count);
        _materialIndex = Random.Range(0, Materials.Count);
        UpdateCustom();
    }


    public void SetIndex(int i)
    {
        _subObjectIndex = i;
        _materialIndex = i;
    }

    public int GetIndex() 
    {
        return  Materials.Count==0? _subObjectIndex : _materialIndex ;
    }
    //public List<SkinnedMeshRenderer> SkinnedMeshRenderers;

    //public List<Mesh> Meshes;
    //int _meshIndex;

    //public void NextMesh()
    //{
    //    _meshIndex++;
    //    if (_meshIndex >= Meshes.Count)
    //        _meshIndex = 0;

    //    UpdateSubObjects();
    //}

    //public void UpdateMeshes()
    //{
    //    foreach (var renderer in Renderers)
    //        if (renderer)
    //            renderer.sharedMesh = Meshes[_meshIndex];

    //}

    //Cambiar color de texturas por si no hay skins
}



[System.Serializable]
public class Skin: ICustomizable
{

    public List<Texture2D> textures;
    public Material skin;
   
    int _textureIndex;

    public void Next()
    {
        _textureIndex++;
        if (_textureIndex >= textures.Count)
            _textureIndex = 0;

        UpdateCustom();
    }


    public void Previous()
    {
        _textureIndex--;
        if (_textureIndex < 0)
            _textureIndex = textures.Count - 1;

        UpdateCustom();
    }


    public void UpdateCustom()
    {

        skin.mainTexture = textures[_textureIndex];
    }


    public void Randomize()
    {
        _textureIndex = Random.Range(0, textures.Count);
        UpdateCustom();
    }

    public void SetIndex(int i)
    {
        _textureIndex = i;
    }

    public int GetIndex()
    {
        return _textureIndex;
    }
}