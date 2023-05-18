using UnityEngine;
public class BlackBoard : MonoBehaviour
{
    Texture2D texture;
    [SerializeField] Vector2 textureSize = new Vector2(2048, 2048);

    public Vector2 TextureSize => textureSize;
    public Texture2D Texture => texture;

    void Start()
    {
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        GetComponent<Renderer>().material.mainTexture = texture;
    }
}
