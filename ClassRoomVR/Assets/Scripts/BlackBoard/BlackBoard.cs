using UnityEngine;

public class BlackBoard : MonoBehaviour
{
    // Tamaño de la textura que se utilizará en el tablero negro
    [SerializeField] Vector2Int textureSize = new Vector2Int(2048, 2048);

    // Textura que se utilizará en el tablero negro
    private Texture2D texture;

    // Renderizador asociado al tablero negro
    private Renderer blackboardRenderer;

    // Propiedad para acceder al tamaño de la textura desde otras clases
    public Vector2Int TextureSize => textureSize;

    // Propiedad para acceder a la textura desde otras clases
    public Texture2D Texture => texture;

    // Método llamado al inicio del juego
    void Start()
    {
        // Inicializa los componentes necesarios
        InitializeComponents();
    }

    // Inicializa todos los componentes del tablero negro
    private void InitializeComponents()
    {
        // Inicializa la textura del tablero negro
        InitializeTexture();

        // Inicializa el renderizador del tablero negro
        InitializeRenderer();
    }

    // Inicializa la textura que se utilizará en el tablero negro
    private void InitializeTexture()
    {
        texture = new Texture2D(textureSize.x, textureSize.y);
    }

    // Inicializa el renderizador asociado al tablero negro
    private void InitializeRenderer()
    {
        // Obtiene el componente Renderer de este GameObject
        blackboardRenderer = GetComponent<Renderer>();

        // Asigna la textura creada al componente material del renderizador
        blackboardRenderer.material.mainTexture = texture;
    }
}
