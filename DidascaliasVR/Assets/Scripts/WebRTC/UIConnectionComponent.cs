using TMPro;
using UnityEngine;

public class UIConnectionComponent : MonoBehaviour
{
    public GameObject UIPrefab;

    public void SetConnection()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);
        StreamManager.Instance?.CreateSignalingServer();
    }

    public void CreateUIRepresentation(string ip)
    {
        GameObject obj = Instantiate(UIPrefab);
        Color color = new Color(Random.value, Random.value, Random.value);
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = color;
        TextMeshProUGUI text = GetComponent<TextMeshProUGUI>();
        text.color = color;
        text.text = ip;
    }

    public void ChangeScene(string scene)
    {
        SceneChanger.Instance?.ChangeScene(scene);
    }

}
