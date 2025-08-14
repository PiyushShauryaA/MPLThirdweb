#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.EventSystems;

namespace CanvasChess.Editor
{
    /// <summary>
    /// Custom editor utility to auto-generate the complete ChessGame scene
    /// </summary>
    public class ChessSceneAutoBuilder : EditorWindow
    {
        [MenuItem("CanvasChess/Auto-Build Chess Scene")]
        public static void ShowWindow()
        {
            GetWindow<ChessSceneAutoBuilder>("Chess Scene Auto Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Auto-Build ChessGame Scene", EditorStyles.boldLabel);
            if (GUILayout.Button("Create ChessGame Scene"))
            {
                CreateChessGameScene();
            }
        }

        /// <summary>
        /// Creates the full ChessGame scene with all required objects and wiring
        /// </summary>
        public static void CreateChessGameScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ChessGame";

            // Create Canvas
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(800, 800);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            // Create ChessBoard (Grid)
            var boardGO = new GameObject("ChessBoard", typeof(RectTransform), typeof(GridLayoutGroup));
            boardGO.transform.SetParent(canvasGO.transform, false);
            var boardRect = boardGO.GetComponent<RectTransform>();
            boardRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardRect.pivot = new Vector2(0.5f, 0.5f);
            boardRect.sizeDelta = new Vector2(800, 800);
            boardRect.anchoredPosition = Vector2.zero;
            // Stretch to fit parent
            boardRect.offsetMin = Vector2.zero;
            boardRect.offsetMax = Vector2.zero;
            boardRect.localScale = Vector3.one;
            var grid = boardGO.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;
            grid.cellSize = new Vector2(100, 100);
            grid.spacing = Vector2.zero;
            grid.childAlignment = TextAnchor.MiddleCenter;

            // Create BoardManager
            var boardManager = boardGO.AddComponent<BoardManager>();

            // Create UI: TurnDisplay
            var turnTextGO = new GameObject("TurnDisplay", typeof(RectTransform), typeof(TextMeshProUGUI));
            turnTextGO.transform.SetParent(canvasGO.transform, false);
            var turnText = turnTextGO.GetComponent<TextMeshProUGUI>();
            turnText.text = "White's Turn";
            turnText.fontSize = 48;
            turnText.alignment = TextAlignmentOptions.Top;
            var turnRect = turnTextGO.GetComponent<RectTransform>();
            turnRect.anchorMin = new Vector2(0.5f, 1f);
            turnRect.anchorMax = new Vector2(0.5f, 1f);
            turnRect.pivot = new Vector2(0.5f, 1f);
            turnRect.anchoredPosition = new Vector2(0, -20);
            turnRect.sizeDelta = new Vector2(400, 60);

            // Create UI: StatusDisplay
            var statusTextGO = new GameObject("StatusDisplay", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusTextGO.transform.SetParent(canvasGO.transform, false);
            var statusText = statusTextGO.GetComponent<TextMeshProUGUI>();
            statusText.text = "";
            statusText.fontSize = 36;
            statusText.alignment = TextAlignmentOptions.Top;
            var statusRect = statusTextGO.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 1f);
            statusRect.anchorMax = new Vector2(0.5f, 1f);
            statusRect.pivot = new Vector2(0.5f, 1f);
            statusRect.anchoredPosition = new Vector2(0, -80);
            statusRect.sizeDelta = new Vector2(600, 50);

            // Create UI: Restart Button
            var restartBtnGO = new GameObject("RestartButton", typeof(RectTransform), typeof(Button), typeof(Image));
            restartBtnGO.transform.SetParent(canvasGO.transform, false);
            var restartRect = restartBtnGO.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(1f, 1f);
            restartRect.anchorMax = new Vector2(1f, 1f);
            restartRect.pivot = new Vector2(1f, 1f);
            restartRect.anchoredPosition = new Vector2(-20, -20);
            restartRect.sizeDelta = new Vector2(160, 60);
            var restartBtn = restartBtnGO.GetComponent<Button>();
            var restartImg = restartBtnGO.GetComponent<Image>();
            restartImg.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            // Add TMP text to restart button
            var restartTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            restartTextGO.transform.SetParent(restartBtnGO.transform, false);
            var restartText = restartTextGO.GetComponent<TextMeshProUGUI>();
            restartText.text = "Restart";
            restartText.fontSize = 32;
            restartText.alignment = TextAlignmentOptions.Center;
            var restartTextRect = restartTextGO.GetComponent<RectTransform>();
            restartTextRect.anchorMin = Vector2.zero;
            restartTextRect.anchorMax = Vector2.one;
            restartTextRect.offsetMin = Vector2.zero;
            restartTextRect.offsetMax = Vector2.zero;

            // Create UI: Back to Menu Button
            var backToMenuBtnGO = new GameObject("BackToMenuButton", typeof(RectTransform), typeof(Button), typeof(Image));
            backToMenuBtnGO.transform.SetParent(canvasGO.transform, false);
            var backToMenuRect = backToMenuBtnGO.GetComponent<RectTransform>();
            backToMenuRect.anchorMin = new Vector2(1f, 1f);
            backToMenuRect.anchorMax = new Vector2(1f, 1f);
            backToMenuRect.pivot = new Vector2(1f, 1f);
            backToMenuRect.anchoredPosition = new Vector2(-20, -90);
            backToMenuRect.sizeDelta = new Vector2(160, 60);
            var backToMenuBtn = backToMenuBtnGO.GetComponent<Button>();
            var backToMenuImg = backToMenuBtnGO.GetComponent<Image>();
            backToMenuImg.color = new Color(0.6f, 0.6f, 0.8f, 1f);

            // Add TMP text to back to menu button
            var backToMenuTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            backToMenuTextGO.transform.SetParent(backToMenuBtnGO.transform, false);
            var backToMenuText = backToMenuTextGO.GetComponent<TextMeshProUGUI>();
            backToMenuText.text = "Menu";
            backToMenuText.fontSize = 32;
            backToMenuText.alignment = TextAlignmentOptions.Center;
            var backToMenuTextRect = backToMenuTextGO.GetComponent<RectTransform>();
            backToMenuTextRect.anchorMin = Vector2.zero;
            backToMenuTextRect.anchorMax = Vector2.one;
            backToMenuTextRect.offsetMin = Vector2.zero;
            backToMenuTextRect.offsetMax = Vector2.zero;

            // Create Bootstrap (UnicodeSpriteGenerator)
            var bootstrapGO = new GameObject("Bootstrap");
            bootstrapGO.AddComponent<UnicodeSpriteGenerator>();

            // Create EventSystem (required for input)
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // Create GameManager
            var gameManagerGO = new GameObject("GameManager");
            var gameManager = gameManagerGO.AddComponent<GameManager>();

            // Create UIManager
            var uiManagerGO = new GameObject("UIManager");
            var uiManager = uiManagerGO.AddComponent<UIManager>();

            // Wire up references
            // BoardManager
            boardManager.GetType().GetField("tilePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(boardManager, FindOrCreatePrefab("Tile"));
            boardManager.GetType().GetField("piecePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(boardManager, FindOrCreatePrefab("Piece"));
            boardManager.GetType().GetField("boardContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(boardManager, boardGO.transform);
            boardManager.GetType().GetField("gridLayout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(boardManager, grid);

            // UIManager
            uiManager.GetType().GetField("turnDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, turnText);
            uiManager.GetType().GetField("statusDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, statusText);
            uiManager.GetType().GetField("restartButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, restartBtn);
            uiManager.GetType().GetField("backToMenuButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, backToMenuBtn);
            uiManager.GetType().GetField("gameManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, gameManager);

            // GameManager
            gameManager.GetType().GetField("boardManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameManager, boardManager);
            gameManager.GetType().GetField("uiManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameManager, uiManager);

            // Save the scene
            string scenePath = "Assets/Scenes/ChessGame.unity";
            if (!Directory.Exists("Assets/Scenes"))
                Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            EditorUtility.DisplayDialog("Chess Scene Auto Builder", "ChessGame scene created and saved!\n\nTile and Piece prefabs have been created with all required components.", "OK");
        }

        /// <summary>
        /// Finds or creates a prefab with the given name in Assets/Prefabs
        /// </summary>
        private static GameObject FindOrCreatePrefab(string prefabName)
        {
            string dir = "Assets/Prefabs";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string path = $"{dir}/{prefabName}.prefab";
            // Always delete and recreate to ensure correctness
            if (File.Exists(path))
            {
                AssetDatabase.DeleteAsset(path);
            }
            // Create a fully functional prefab
            var prefab = CreateFunctionalPrefab(prefabName, path);
            return prefab;
        }

        /// <summary>
        /// Creates a fully functional prefab with all required components
        /// </summary>
        private static GameObject CreateFunctionalPrefab(string prefabName, string path)
        {
            GameObject go = new GameObject(prefabName);
            
            // Add RectTransform
            var rectTransform = go.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 100);
            
            if (prefabName == "Tile")
            {
                // Create Tile prefab
                var image = go.AddComponent<Image>();
                image.color = new Color(0.93f, 0.93f, 0.93f, 1f); // Light color
                
                // Add Tile script
                var tile = go.AddComponent<Tile>();
                
                // Add TileClickHandler
                var clickHandler = go.AddComponent<TileClickHandler>();
                
                // Try to assign lightSquare sprite if it exists
                var lightSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/lightSquare.png");
                if (lightSprite != null)
                {
                    image.sprite = lightSprite;
                }
            }
            else if (prefabName == "Piece")
            {
                // Create Piece prefab
                var image = go.AddComponent<Image>();
                image.color = Color.white;
                
                // Add Piece script
                var piece = go.AddComponent<Piece>();
                
                // Try to assign a default piece sprite if it exists
                var pieceSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/white_pawn.png");
                if (pieceSprite != null)
                {
                    image.sprite = pieceSprite;
                }
            }
            
            // Save as prefab
            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            GameObject.DestroyImmediate(go);
            
            return prefab;
        }
    }
}
#endif 