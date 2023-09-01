using System.Linq;
using UnityEngine;

public class BlackBoardMarker : MonoBehaviour
{
    // The tip of the marker
    [SerializeField] Transform tip;

    // Size of the marker
    [SerializeField] int penSize = 5;

    // Colors to be used for drawing
    Color[] colors;

    // Height of the marker's tip
    float tipHeight;

    // Raycast information when touching the blackboard
    RaycastHit touch;

    // Reference to the blackboard
    BlackBoard blackBoard;

    // Previous interaction state
    bool touchedLastTime;

    // Last touch position
    Vector2 lastTouchPos;

    // Last touch rotation
    Quaternion lastTouchRot;

    // Increment for interpolation (lerp)
    private const float LerpIncrement = 0.01f;

    // Method called at the start of the game
    void Start()
    {
        // Initialize necessary components
        Initialize();
    }

    // Method called every frame
    void Update()
    {
        // Perform drawing on the blackboard
        Draw();
    }

    // Initialize the marker's components
    private void Initialize()
    {
        // Create an array of colors with the color of the marker's tip
        colors = Enumerable.Repeat(tip.GetComponent<Renderer>().material.color, penSize * penSize).ToArray();

        // Get the height of the marker's tip
        tipHeight = tip.localScale.y;
    }

    // Perform drawing on the blackboard
    private void Draw()
    {
        // Perform a raycast upwards from the marker's tip
        if (Physics.Raycast(tip.position, transform.up, out touch, tipHeight) && touch.transform.CompareTag("BlackBoard"))
        {
            // Handle interaction with the blackboard
            HandleBlackBoardInteraction();
        }
        else
        {
            // Reset interaction state
            ResetInteractionState();
        }
    }

    // Handle interaction with the blackboard
    private void HandleBlackBoardInteraction()
    {
        // If the blackboard reference is not assigned yet, obtain it
        if (!blackBoard)
            blackBoard = touch.transform.GetComponent<BlackBoard>();

        // Calculate the position on the blackboard's texture
        int x = Mathf.FloorToInt(touch.textureCoord.x * blackBoard.TextureSize.x - (penSize / 2));
        int y = Mathf.FloorToInt(touch.textureCoord.y * blackBoard.TextureSize.y - (penSize / 2));

        // If the position is out of the texture bounds, do nothing
        if (y < 0 || y >= blackBoard.TextureSize.y || x < 0 || x >= blackBoard.TextureSize.x)
            return;

        // If touched last time, perform drawing
        if (touchedLastTime)
        {
            // Draw the marker's marks on the texture
            DrawPenMarks(x, y);

            // Smoothly interpolate between the last position and the current position
            for (float f = LerpIncrement; f < 1.00f; f += LerpIncrement)
            {
                int lerpX = Mathf.RoundToInt(Mathf.Lerp(lastTouchPos.x, x, f));
                int lerpY = Mathf.RoundToInt(Mathf.Lerp(lastTouchPos.y, y, f));
                DrawPenMarks(lerpX, lerpY);
            }

            // Maintain the marker's rotation as the last touch rotation
            transform.rotation = lastTouchRot;

            // Apply changes to the texture
            ApplyTextureChanges();
        }

        // Store the current position and rotation for the next iteration
        lastTouchPos = new Vector2(x, y);
        lastTouchRot = transform.rotation;
        touchedLastTime = true;
    }

    // Draw the marker's marks on the texture
    private void DrawPenMarks(int x, int y)
    {
        blackBoard.Texture.SetPixels(x, y, penSize, penSize, colors);
    }

    // Apply the changes made to the texture
    private void ApplyTextureChanges()
    {
        blackBoard.Texture.Apply();
    }

    // Reset the interaction state
    private void ResetInteractionState()
    {
        blackBoard = null;
        touchedLastTime = false;
    }
}
