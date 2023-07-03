using UnityEngine;

public class BlackBoard : MonoBehaviour
{
    [SerializeField] Vector2Int textureSize = new Vector2Int(2048, 2048);
    private Texture2D texture;

    public Vector2Int TextureSize => textureSize;
    public Texture2D Texture => texture;

    private Renderer blackboardRenderer;

    void Start()
    {
        InitializeTexture();
        InitializeRenderer();
    }

    private void InitializeTexture()
    {
        texture = new Texture2D(textureSize.x, textureSize.y);
    }

    private void InitializeRenderer()
    {
        blackboardRenderer = GetComponent<Renderer>();
        blackboardRenderer.material.mainTexture = texture;
    }
}
