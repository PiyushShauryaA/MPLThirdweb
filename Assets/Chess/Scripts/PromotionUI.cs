using UnityEngine;
using UnityEngine.UI;

namespace CanvasChess
{
    /// <summary>
    /// Handles the promotion UI panel and piece selection
    /// </summary>
    public class PromotionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject promotionPanel = null!;
    [SerializeField] private Button queenButton = null!;
    [SerializeField] private Button rookButton = null!;
    [SerializeField] private Button bishopButton = null!;
    [SerializeField] private Button knightButton = null!;
    [SerializeField] private Image queenImage = null!;
    [SerializeField] private Image rookImage = null!;
    [SerializeField] private Image bishopImage = null!;
    [SerializeField] private Image knightImage = null!;

    [Header("Game Manager Reference")]
    [SerializeField] private GameManager gameManager = null!;

    [Header("Manual Sprite Assignment (Fallback)")]
    [SerializeField] private Sprite whiteQueenSprite = null!;
    [SerializeField] private Sprite whiteRookSprite = null!;
    [SerializeField] private Sprite whiteBishopSprite = null!;
    [SerializeField] private Sprite whiteKnightSprite = null!;
    [SerializeField] private Sprite blackQueenSprite = null!;
    [SerializeField] private Sprite blackRookSprite = null!;
    [SerializeField] private Sprite blackBishopSprite = null!;
    [SerializeField] private Sprite blackKnightSprite = null!;

    private void Start()
    {
        // Hide the panel initially
        if (promotionPanel != null)
            promotionPanel.SetActive(false);

        // Set up button listeners
        SetupButtonListeners();
    }

    private void SetupButtonListeners()
    {
        if (queenButton != null)
            queenButton.onClick.AddListener(() => OnPieceSelected(PieceType.Queen));
        if (rookButton != null)
            rookButton.onClick.AddListener(() => OnPieceSelected(PieceType.Rook));
        if (bishopButton != null)
            bishopButton.onClick.AddListener(() => OnPieceSelected(PieceType.Bishop));
        if (knightButton != null)
            knightButton.onClick.AddListener(() => OnPieceSelected(PieceType.Knight));
    }

    /// <summary>
    /// Shows the promotion UI with the appropriate piece sprites
    /// </summary>
    /// <param name="pieceColor">The color of the pawn being promoted</param>
    public void ShowPromotionUI(PieceColor pieceColor)
    {
        if (promotionPanel == null) return;

        // Set up the promotion piece images
        SetupPromotionImages(pieceColor);

        // Show the panel
        promotionPanel.SetActive(true);
    }

    /// <summary>
    /// Hides the promotion UI
    /// </summary>
    public void HidePromotionUI()
    {
        if (promotionPanel != null)
            promotionPanel.SetActive(false);
    }

    /// <summary>
    /// Sets up the promotion piece images based on the pawn's color
    /// </summary>
    /// <param name="pieceColor">The color of the pawn being promoted</param>
    private void SetupPromotionImages(PieceColor pieceColor)
    {
        Debug.Log($"=== PROMOTION UI: Setting up for {pieceColor} pawn ===");
        
        // Use manually assigned sprites first (recommended approach)
        if (pieceColor == PieceColor.White)
        {
            Debug.Log("Loading WHITE piece sprites for promotion...");
            
            // Update both the separate images and the button images
            if (queenImage != null && whiteQueenSprite != null) 
            {
                queenImage.sprite = whiteQueenSprite;
                Debug.Log("✓ White Queen sprite assigned to queenImage");
            }
            if (queenButton != null && whiteQueenSprite != null)
            {
                Image buttonImage = queenButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = whiteQueenSprite;
                    Debug.Log("✓ White Queen sprite assigned to queenButton");
                }
            }
            
            if (rookImage != null && whiteRookSprite != null) 
            {
                rookImage.sprite = whiteRookSprite;
                Debug.Log("✓ White Rook sprite assigned to rookImage");
            }
            if (rookButton != null && whiteRookSprite != null)
            {
                Image buttonImage = rookButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = whiteRookSprite;
                    Debug.Log("✓ White Rook sprite assigned to rookButton");
                }
            }
            
            if (bishopImage != null && whiteBishopSprite != null) 
            {
                bishopImage.sprite = whiteBishopSprite;
                Debug.Log("✓ White Bishop sprite assigned to bishopImage");
            }
            if (bishopButton != null && whiteBishopSprite != null)
            {
                Image buttonImage = bishopButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = whiteBishopSprite;
                    Debug.Log("✓ White Bishop sprite assigned to bishopButton");
                }
            }
            
            if (knightImage != null && whiteKnightSprite != null) 
            {
                knightImage.sprite = whiteKnightSprite;
                Debug.Log("✓ White Knight sprite assigned to knightImage");
            }
            if (knightButton != null && whiteKnightSprite != null)
            {
                Image buttonImage = knightButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = whiteKnightSprite;
                    Debug.Log("✓ White Knight sprite assigned to knightButton");
                }
            }
        }
        else // PieceColor.Black
        {
            Debug.Log("Loading BLACK piece sprites for promotion...");
            
            // Update both the separate images and the button images
            if (queenImage != null && blackQueenSprite != null) 
            {
                queenImage.sprite = blackQueenSprite;
                Debug.Log("✓ Black Queen sprite assigned to queenImage");
            }
            if (queenButton != null && blackQueenSprite != null)
            {
                Image buttonImage = queenButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = blackQueenSprite;
                    Debug.Log("✓ Black Queen sprite assigned to queenButton");
                }
            }
            
            if (rookImage != null && blackRookSprite != null) 
            {
                rookImage.sprite = blackRookSprite;
                Debug.Log("✓ Black Rook sprite assigned to rookImage");
            }
            if (rookButton != null && blackRookSprite != null)
            {
                Image buttonImage = rookButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = blackRookSprite;
                    Debug.Log("✓ Black Rook sprite assigned to rookButton");
                }
            }
            
            if (bishopImage != null && blackBishopSprite != null) 
            {
                bishopImage.sprite = blackBishopSprite;
                Debug.Log("✓ Black Bishop sprite assigned to bishopImage");
            }
            if (bishopButton != null && blackBishopSprite != null)
            {
                Image buttonImage = bishopButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = blackBishopSprite;
                    Debug.Log("✓ Black Bishop sprite assigned to bishopButton");
                }
            }
            
            if (knightImage != null && blackKnightSprite != null) 
            {
                knightImage.sprite = blackKnightSprite;
                Debug.Log("✓ Black Knight sprite assigned to knightImage");
            }
            if (knightButton != null && blackKnightSprite != null)
            {
                Image buttonImage = knightButton.GetComponent<Image>();
                if (buttonImage != null)
                {
                    buttonImage.sprite = blackKnightSprite;
                    Debug.Log("✓ Black Knight sprite assigned to knightButton");
                }
            }
        }

        // Check if any sprites are missing
        bool allSpritesAssigned = true;
        if (pieceColor == PieceColor.White)
        {
            if (whiteQueenSprite == null) { Debug.LogWarning("❌ White Queen sprite not assigned!"); allSpritesAssigned = false; }
            if (whiteRookSprite == null) { Debug.LogWarning("❌ White Rook sprite not assigned!"); allSpritesAssigned = false; }
            if (whiteBishopSprite == null) { Debug.LogWarning("❌ White Bishop sprite not assigned!"); allSpritesAssigned = false; }
            if (whiteKnightSprite == null) { Debug.LogWarning("❌ White Knight sprite not assigned!"); allSpritesAssigned = false; }
        }
        else
        {
            if (blackQueenSprite == null) { Debug.LogWarning("❌ Black Queen sprite not assigned!"); allSpritesAssigned = false; }
            if (blackRookSprite == null) { Debug.LogWarning("❌ Black Rook sprite not assigned!"); allSpritesAssigned = false; }
            if (blackBishopSprite == null) { Debug.LogWarning("❌ Black Bishop sprite not assigned!"); allSpritesAssigned = false; }
            if (blackKnightSprite == null) { Debug.LogWarning("❌ Black Knight sprite not assigned!"); allSpritesAssigned = false; }
        }

        if (allSpritesAssigned)
        {
            Debug.Log($"🎉 All {pieceColor} promotion sprites successfully assigned!");
        }
        else
        {
            Debug.LogError($"⚠️ Some {pieceColor} promotion sprites are missing! Please assign them in the Inspector.");
        }
    }

    /// <summary>
    /// Handles when a promotion piece is selected
    /// </summary>
    /// <param name="pieceType">The selected piece type</param>
    private void OnPieceSelected(PieceType pieceType)
    {
        if (gameManager == null) return;

        // Hide the promotion UI
        HidePromotionUI();

        // Promote the piece and complete the move
        gameManager.PromoteAndCompleteMove(pieceType);
    }
}
} 