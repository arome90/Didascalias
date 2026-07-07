using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConnectionComponent : MonoBehaviour
{
    public GameObject UIPrefab;
    public TextMeshProUGUI connectionsNum;
    int connections;

    public void SetConnection()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(true);
        transform.GetChild(3).gameObject.SetActive(true);
        StreamManager.Instance?.SetUIComponent(this);
        StreamManager.Instance?.CreateSignalingServer();
    }

    public void CreateUIRepresentation(string ip)
    {
        GameObject obj = Instantiate(UIPrefab, gameObject.transform);
        Color color = new Color(Random.value, Random.value, Random.value);
        Image sr = obj.transform.GetChild(0).GetComponent<Image>();
        sr.color = color;
        TextMeshProUGUI text = obj.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        text.color = color;
        text.text = ip;

        if (!transform.GetChild(2).gameObject.activeSelf)
            transform.GetChild(2).gameObject.SetActive(true);

        connections++;
        SetNumConnectionsInfo();
    }

    public void ChangeScene(string scene)
    {
        SceneChanger.Instance?.ChangeScene(scene);
    }

    public void SetNumConnectionsInfo()
    {
        connectionsNum.text = "Conexiones: " + connections +"/" + ClassManager.Instance.Settings.NumStudents;
    }

    public void Start()
    {
        connections = 0;
        SetNumConnectionsInfo();
    }
}
