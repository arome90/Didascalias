using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

[System.Serializable]
public struct SetData
{
    public GameObject obj;
    public string name;
    public string pol;
    public string mat;
}
public class CharacterTest : MonoBehaviour
{
    [SerializeField]
    SetData[] characterSets;
    List<GameObject>[] characterSetsList;

    [SerializeField]
    int setId = 0;
    int actualId;

    [SerializeField]
    InputActionReference buttonPrimary;
    [SerializeField]
    InputActionReference buttonSecondary;
    [SerializeField]
    InputActionReference buttonTertiary;

    [SerializeField]
    TextMeshProUGUI countText;
    [SerializeField]
    TextMeshProUGUI nameText;
    [SerializeField]
    TextMeshProUGUI polText;
    [SerializeField]
    TextMeshProUGUI matText;

    // Start is called before the first frame update
    void Start()
    {
        buttonPrimary.action.started += ButtonPressed;
        buttonSecondary.action.started += ButtonReleased;
        buttonTertiary.action.started += ButtonChange;

        // Creamos un array de listas de gameobjects (para gestionar los personajes de cada set)
        characterSetsList = new List<GameObject>[characterSets.Length];

        // Recorremos cada set creando una lista y aniadiendo los personajes 
        for (int i = 0; i < characterSets.Length; i++)
        {
            {
                List<GameObject> childs = new List<GameObject>();

                foreach (Transform child in characterSets[i].obj.transform)
                {
                    childs.Add(child.gameObject);
                    Debug.Log(child.gameObject.name);
                }

                characterSetsList[i] = childs;
            }
        }

        actualId = characterSetsList[setId].Count-1;
        updateCanvas();
        //Debug.Log("AAAAAAAAAAA");
        //for(int i = 0;i < characterSetsList[setId].Count; i++)
        //{
        //    Debug.Log(characterSetsList[setId][i].name);
        //}
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("PRE SUB");
            hideCharacter();
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            Debug.Log("PRE ADD");
            showCharacter();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            Debug.Log("PRE ADD");
            changeSet();
        }
    }

    void showCharacter()
    {
        if (actualId < characterSetsList[setId].Count - 1)
            actualId++;

        for (int i = 0; i < characterSetsList.Length; i++)
        {
            characterSetsList[i][actualId].SetActive(true);
        }
        Debug.Log("POSTADD");
        //characterSetsList[setId][actualId].SetActive(true);
        updateCount();

    }

    void hideCharacter()
    {
        for(int i = 0; i < characterSetsList.Length; i++)
        {
            characterSetsList[i][actualId].SetActive(false);
        }
        Debug.Log("POSTSUB");
        //characterSetsList[setId][actualId].SetActive(false);
        if (actualId >= 0)
            actualId--;
        updateCount();
    }

    void changeSet()
    {
        // Desactivar el set actual y activar el siguiente
        characterSets[setId].obj.SetActive(false);
        if (setId == characterSets.Length - 1)
            setId = 0;
        else setId++;
        characterSets[setId].obj.SetActive(true);

        updateCanvas();
    }

    void ButtonPressed(InputAction.CallbackContext context)
    {
        showCharacter();
    }
    void ButtonReleased(InputAction.CallbackContext context)
    {
        hideCharacter();
    }
    void ButtonChange(InputAction.CallbackContext context)
    {
        changeSet();
    }

    void updateCanvas()
    {
        countText.text = "Cantidad: " + (actualId + 1);
        nameText.text = "Nombre: " + characterSets[setId].name;
        polText.text = "Poligonos: " + characterSets[setId].pol;
        matText.text = "Red Materiales: " + characterSets[setId].mat;
    }
    void updateCount()
    {
        countText.text = "Cantidad: " + (actualId + 1);
    }
}
