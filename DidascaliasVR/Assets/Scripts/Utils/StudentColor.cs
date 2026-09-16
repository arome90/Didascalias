using UnityEngine;

public class StudentColor : MonoBehaviour
{
    [SerializeField]
    Color[] hairColors;




    [SerializeField] private Renderer studentRenderer;


    private static readonly int ColorBagID =
        Shader.PropertyToID("_Color_Hair");



    void Start()
    {
        // A PARA NO CREAR INSTANCIAS DEL MATERIAL // B CREA DIFERENTES INSTANCIAS -> DA UN POCO IGUAL DE RENDIMIENTO
        a();
        //b();
    }

    private Color GetHairRandomColor()
    {
        return hairColors[Random.Range(0, hairColors.Length)];
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void a()
    {
        MaterialPropertyBlock block = new MaterialPropertyBlock();

        // ───── MOCHILA ─────

        studentRenderer.GetPropertyBlock(block);

        Color bagColor = GetHairRandomColor();

        block.SetColor(ColorBagID, bagColor);

        studentRenderer.SetPropertyBlock(block);

    }

    void b()
    {
        studentRenderer.material.SetColor("_Color_Hair", GetHairRandomColor());
    }

}
