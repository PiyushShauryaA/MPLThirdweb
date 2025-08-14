using UnityEngine;
using System.IO;
using TMPro;

namespace CanvasChess
{
    /// <summary>
    /// Generates chess piece sprites from Unicode characters at runtime
    /// </summary>
    public class UnicodeSpriteGenerator : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Unicode Chess Characters")]
        [SerializeField] private string whiteKingChar = "♔";
        [SerializeField] private string whiteQueenChar = "♕";
        [SerializeField] private string whiteRookChar = "♖";
        [SerializeField] private string whiteBishopChar = "♗";
        [SerializeField] private string whiteKnightChar = "♘";
        [SerializeField] private string whitePawnChar = "♙";
        
        [SerializeField] private string blackKingChar = "♚";
        [SerializeField] private string blackQueenChar = "♛";
        [SerializeField] private string blackRookChar = "♜";
        [SerializeField] private string blackBishopChar = "♝";
        [SerializeField] private string blackKnightChar = "♞";
        [SerializeField] private string blackPawnChar = "♟";

        [Header("Generation Settings")]
        [SerializeField] private int textureSize = 256;
        [SerializeField] private Color whitePieceColor = Color.white;
        [SerializeField] private Color blackPieceColor = Color.black;
        [SerializeField] private Color backgroundColor = Color.clear;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // On mobile platforms, skip runtime sprite generation
            #if UNITY_ANDROID || UNITY_IOS
                Debug.Log("Runtime sprite generation disabled on mobile platforms");
                return;
            #else
                GenerateSpritesIfNeeded();
            #endif
        }
        #endregion

        #region Sprite Generation
        /// <summary>
        /// Generates chess piece sprites if they don't already exist
        /// </summary>
        private void GenerateSpritesIfNeeded()
        {
            // Only run on editor or standalone platforms
            #if UNITY_EDITOR || UNITY_STANDALONE
                string flagPath = Path.Combine(Application.persistentDataPath, "sprites_generated.flag");
                
                if (File.Exists(flagPath))
                {
                    Debug.Log("Chess sprites already generated, skipping...");
                    return;
                }

                Debug.Log("Generating chess piece sprites...");
                
                // Create Resources/Sprites directory if it doesn't exist
                string spritesPath = Path.Combine(Application.persistentDataPath, "Resources", "Sprites");
                if (!Directory.Exists(spritesPath))
                {
                    Directory.CreateDirectory(spritesPath);
                }

                // Generate white pieces
                GeneratePieceSprite(whiteKingChar, "w_king", whitePieceColor);
                GeneratePieceSprite(whiteQueenChar, "w_queen", whitePieceColor);
                GeneratePieceSprite(whiteRookChar, "w_rook", whitePieceColor);
                GeneratePieceSprite(whiteBishopChar, "w_bishop", whitePieceColor);
                GeneratePieceSprite(whiteKnightChar, "w_knight", whitePieceColor);
                GeneratePieceSprite(whitePawnChar, "w_pawn", whitePieceColor);

                // Generate black pieces
                GeneratePieceSprite(blackKingChar, "b_king", blackPieceColor);
                GeneratePieceSprite(blackQueenChar, "b_queen", blackPieceColor);
                GeneratePieceSprite(blackRookChar, "b_rook", blackPieceColor);
                GeneratePieceSprite(blackBishopChar, "b_bishop", blackPieceColor);
                GeneratePieceSprite(blackKnightChar, "b_knight", blackPieceColor);
                GeneratePieceSprite(blackPawnChar, "b_pawn", blackPieceColor);

                // Create flag file to indicate sprites have been generated
                File.WriteAllText(flagPath, "sprites_generated");
                
                Debug.Log("Chess piece sprites generated successfully!");
            #else
                Debug.Log("Sprite generation not supported on this platform");
            #endif
        }
        #endregion

        #region Individual Sprite Generation
        /// <summary>
        /// Generates a sprite for a specific chess piece character
        /// </summary>
        /// <param name="unicodeChar">The Unicode character representing the piece</param>
        /// <param name="filename">The filename to save the sprite as</param>
        /// <param name="pieceColor">The color of the piece</param>
        private void GeneratePieceSprite(string unicodeChar, string filename, Color pieceColor)
        {
            #if UNITY_EDITOR || UNITY_STANDALONE
            // Create a temporary GameObject with TextMeshPro to render the character
            GameObject tempGO = new GameObject("TempText");
            TextMeshProUGUI tmpText = tempGO.AddComponent<TextMeshProUGUI>();
            
            // Configure the text component
            tmpText.text = unicodeChar;
            tmpText.fontSize = textureSize * 0.6f;
            tmpText.color = pieceColor;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.enableAutoSizing = false;
            
            // Create a canvas to render the text
            GameObject canvasGO = new GameObject("TempCanvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            // Create a panel to hold the text
            GameObject panelGO = new GameObject("TempPanel");
            panelGO.transform.SetParent(canvasGO.transform, false);
            UnityEngine.UI.Image panelImage = panelGO.AddComponent<UnityEngine.UI.Image>();
            panelImage.color = backgroundColor;
            
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.sizeDelta = new Vector2(textureSize, textureSize);
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Parent the text to the panel
            tempGO.transform.SetParent(panelGO.transform, false);
            RectTransform textRect = tempGO.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(textureSize, textureSize);
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            // Create a render texture
            RenderTexture renderTexture = new RenderTexture(textureSize, textureSize, 24);
            renderTexture.Create();
            
            // Set the camera to render to our texture
            Camera tempCamera = new GameObject("TempCamera").AddComponent<Camera>();
            tempCamera.targetTexture = renderTexture;
            tempCamera.clearFlags = CameraClearFlags.SolidColor;
            tempCamera.backgroundColor = backgroundColor;
            tempCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            tempCamera.orthographic = true;
            tempCamera.orthographicSize = textureSize / 2f;
            tempCamera.transform.position = new Vector3(0, 0, -10);
            
            // Render the scene
            tempCamera.Render();
            
            // Read the pixels from the render texture
            RenderTexture.active = renderTexture;
            Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
            texture.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
            texture.Apply();
            RenderTexture.active = null;
            
            // Encode to PNG and save
            byte[] pngData = texture.EncodeToPNG();
            string filePath = Path.Combine(Application.persistentDataPath, "Resources", "Sprites", filename + ".png");
            File.WriteAllBytes(filePath, pngData);
            
            // Clean up
            DestroyImmediate(tempGO);
            DestroyImmediate(canvasGO);
            DestroyImmediate(tempCamera.gameObject);
            DestroyImmediate(renderTexture);
            DestroyImmediate(texture);
            #endif
        }
        #endregion
    }
} 