using UnityEngine;

public class PropColor : MonoBehaviour
{
    [SerializeField]
    Color[] bagColors;
    [SerializeField]
    Color[] caseColors;
    [SerializeField]
    Color[] notebookColors;



    [SerializeField] private Renderer bagRenderer;
    [SerializeField] private Renderer caseRenderer;
    [SerializeField] private Renderer notebookRenderer;


    private static readonly int ColorBagID =
        Shader.PropertyToID("_Color_Bag");

    private static readonly int ColorCaseID =
        Shader.PropertyToID("_Color_Case");


    void Start()
    {
        // A PARA NO CREAR INSTANCIAS DEL MATERIAL // B CREA DIFERENTES INSTANCIAS -> DA UN POCO IGUAL DE RENDIMIENTO
        a();
        //b();
    }

    private Color GetBagRandomColor()
    {
        return bagColors[Random.Range(0, bagColors.Length)];
    }
    private Color GetCaseRandomColor()
    {
        return caseColors[Random.Range(0, caseColors.Length)];
    }
    private Color GetNotebookRandomColor()
    {
        return notebookColors[Random.Range(0, notebookColors.Length)];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void a()
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        // ───── MOCHILA ─────

        bagRenderer.GetPropertyBlock(block);

        Color bagColor = GetBagRandomColor();

        block.SetColor(ColorBagID, bagColor);

        bagRenderer.SetPropertyBlock(block);


        // ───── ESTUCHE ─────

        block.Clear();

        caseRenderer.GetPropertyBlock(block);

        Color caseColor = GetCaseRandomColor();

        block.SetColor(ColorCaseID, caseColor);

        caseRenderer.SetPropertyBlock(block);

        // ───── CUADERNO ─────

        block.Clear();

        notebookRenderer.GetPropertyBlock(block);

        Color notebookColor = GetNotebookRandomColor();

        block.SetColor(ColorBagID, notebookColor);

        notebookRenderer.SetPropertyBlock(block);
    }

    void b()
    {
        bagRenderer.material.SetColor("_Color_Bag", GetBagRandomColor());
        caseRenderer.material.SetColor("_Color_Case", GetCaseRandomColor());
        notebookRenderer.material.SetColor("_Color_Bag", GetNotebookRandomColor());
    }

}
