using System.Linq;
using UnityEngine;
public class BlackBoardMarker : MonoBehaviour
{
    [SerializeField] Transform tip;
    [SerializeField] int penSize = 5;

    Color[] colors;
    float tipHeight;

    RaycastHit touch;
    BlackBoard blackBoard;

    bool touchedLastTime;
    Vector2 lastTouchPos;
    Quaternion lastTouchRot;

    void Start()
    {
        colors = Enumerable.Repeat(tip.GetComponent<Renderer>().material.color, penSize * penSize).ToArray();
        tipHeight = tip.localScale.y;
    }

    void Update()
    {
        Draw();
    }

    private void Draw()
    {
        if (Physics.Raycast(tip.position, transform.up, out touch, tipHeight))
        {
            if (touch.transform.CompareTag("BlackBoard"))
            {
                if (!blackBoard)
                    blackBoard = touch.transform.GetComponent<BlackBoard>();

                int x = (int)(touch.textureCoord.x * blackBoard.TextureSize.x - (penSize / 2));
                int y = (int)(touch.textureCoord.y * blackBoard.TextureSize.y - (penSize / 2));

                if (y < 0 || y > blackBoard.TextureSize.y || x < 0 || x > blackBoard.TextureSize.x) return;

                if (touchedLastTime)
                {
                    blackBoard.Texture.SetPixels(x, y, penSize, penSize, colors);

                    for (float f = 0.01f; f < 1.00f; f += 0.01f)
                    {
                        int lerpX = (int)Mathf.Lerp(lastTouchPos.x, x, f);
                        int lerpY = (int)Mathf.Lerp(lastTouchPos.y, y, f);
                        blackBoard.Texture.SetPixels(lerpX, lerpY, penSize, penSize, colors);
                    }
                    transform.rotation = lastTouchRot;

                    blackBoard.Texture.Apply();
                }

                lastTouchPos = new Vector2(x, y);
                lastTouchRot = transform.rotation;
                touchedLastTime = true;
                return;
            }
        }

        blackBoard = null;
        touchedLastTime = false;
    }
}
