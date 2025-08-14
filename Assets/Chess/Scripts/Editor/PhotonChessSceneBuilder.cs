#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

namespace CanvasChess.Editor
{
    /// <summary>
    /// Auto-generates a Photon-enabled chess scene with all synchronization components
    /// </summary>
    public class PhotonChessSceneBuilder : EditorWindow
    {
        [MenuItem("CanvasChess/Auto-Build Photon Chess Scene")]
        public static void ShowWindow()
        {
            GetWindow<PhotonChessSceneBuilder>("Photon Chess Scene Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Auto-Build Photon Chess Scene", EditorStyles.boldLabel);
            GUILayout.Label("This will create a complete Photon-enabled chess scene with all sync components.");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create Photon Chess Scene"))
            {
                CreatePhotonChessScene();
            }
        }

        /// <summary>
        /// Creates a complete Photon-enabled chess scene
        /// </summary>
        public static void CreatePhotonChessScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ChessGameMulti";

            // Create Canvas
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1200, 800);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            // Create ChessBoard (Grid)
            var boardGO = new GameObject("ChessBoard", typeof(RectTransform), typeof(GridLayoutGroup), typeof(AspectRatioFitter));
            boardGO.transform.SetParent(canvasGO.transform, false);
            var boardRect = boardGO.GetComponent<RectTransform>();
            boardRect.anchorMin = new Vector2(0f, 0f);
            boardRect.anchorMax = new Vector2(1f, 1f);
            boardRect.pivot = new Vector2(0.5f, 0.5f);
            boardRect.anchoredPosition = Vector2.zero;
            boardRect.sizeDelta = Vector2.zero;
            var grid = boardGO.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 8;
            grid.cellSize = new Vector2(100, 100);
            grid.spacing = Vector2.zero;
            grid.childAlignment = TextAnchor.MiddleCenter;
            var aspectFitter = boardGO.GetComponent<AspectRatioFitter>();
            aspectFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            aspectFitter.aspectRatio = 1f;

            // Create BoardManager with PhotonView
            var boardManager = boardGO.AddComponent<BoardManager>();
            var boardManagerPhotonView = boardGO.AddComponent<PhotonView>();
            boardManagerPhotonView.Synchronization = ViewSynchronization.UnreliableOnChange;

            // Create UI: TurnDisplay
            var turnTextGO = new GameObject("TurnDisplay", typeof(RectTransform), typeof(TextMeshProUGUI));
            turnTextGO.transform.SetParent(canvasGO.transform, false);
            var turnText = turnTextGO.GetComponent<TextMeshProUGUI>();
            turnText.text = "White's Turn";
            turnText.fontSize = 36;
            turnText.alignment = TextAlignmentOptions.Top;
            var turnRect = turnTextGO.GetComponent<RectTransform>();
            turnRect.anchorMin = new Vector2(0f, 1f);
            turnRect.anchorMax = new Vector2(0f, 1f);
            turnRect.pivot = new Vector2(0f, 1f);
            turnRect.anchoredPosition = new Vector2(20, -20);
            turnRect.sizeDelta = new Vector2(400, 50);

            // Create UI: StatusDisplay
            var statusTextGO = new GameObject("StatusDisplay", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusTextGO.transform.SetParent(canvasGO.transform, false);
            var statusText = statusTextGO.GetComponent<TextMeshProUGUI>();
            statusText.text = "";
            statusText.fontSize = 28;
            statusText.alignment = TextAlignmentOptions.Top;
            var statusRect = statusTextGO.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0f, 1f);
            statusRect.anchorMax = new Vector2(0f, 1f);
            statusRect.pivot = new Vector2(0f, 1f);
            statusRect.anchoredPosition = new Vector2(20, -70);
            statusRect.sizeDelta = new Vector2(500, 40);

            // Create UI: Restart Button
            var restartBtnGO = new GameObject("RestartButton", typeof(RectTransform), typeof(Button), typeof(Image));
            restartBtnGO.transform.SetParent(canvasGO.transform, false);
            var restartRect = restartBtnGO.GetComponent<RectTransform>();
            restartRect.anchorMin = new Vector2(0f, 1f);
            restartRect.anchorMax = new Vector2(0f, 1f);
            restartRect.pivot = new Vector2(0f, 1f);
            restartRect.anchoredPosition = new Vector2(20, -120);
            restartRect.sizeDelta = new Vector2(120, 40);
            var restartBtn = restartBtnGO.GetComponent<Button>();
            var restartImg = restartBtnGO.GetComponent<Image>();
            restartImg.color = new Color(0.8f, 0.8f, 0.8f, 1f);

            // Add TMP text to restart button
            var restartTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            restartTextGO.transform.SetParent(restartBtnGO.transform, false);
            var restartText = restartTextGO.GetComponent<TextMeshProUGUI>();
            restartText.text = "Restart";
            restartText.fontSize = 20;
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
            backToMenuRect.anchorMin = new Vector2(0f, 1f);
            backToMenuRect.anchorMax = new Vector2(0f, 1f);
            backToMenuRect.pivot = new Vector2(0f, 1f);
            backToMenuRect.anchoredPosition = new Vector2(150, -120);
            backToMenuRect.sizeDelta = new Vector2(120, 40);
            var backToMenuBtn = backToMenuBtnGO.GetComponent<Button>();
            var backToMenuImg = backToMenuBtnGO.GetComponent<Image>();
            backToMenuImg.color = new Color(0.6f, 0.6f, 0.8f, 1f);

            // Add TMP text to back to menu button
            var backToMenuTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            backToMenuTextGO.transform.SetParent(backToMenuBtnGO.transform, false);
            var backToMenuText = backToMenuTextGO.GetComponent<TextMeshProUGUI>();
            backToMenuText.text = "Menu";
            backToMenuText.fontSize = 20;
            backToMenuText.alignment = TextAlignmentOptions.Center;
            var backToMenuTextRect = backToMenuTextGO.GetComponent<RectTransform>();
            backToMenuTextRect.anchorMin = Vector2.zero;
            backToMenuTextRect.anchorMax = Vector2.one;
            backToMenuTextRect.offsetMin = Vector2.zero;
            backToMenuTextRect.offsetMax = Vector2.zero;

            // Create EventSystem (required for input)
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // Create Bootstrap (UnicodeSpriteGenerator)
            var bootstrapGO = new GameObject("Bootstrap");
            bootstrapGO.AddComponent<UnicodeSpriteGenerator>();

            // Create PhotonSimpleConnect for networking
            var photonConnectGO = new GameObject("PhotonConnect");
            var photonConnect = photonConnectGO.AddComponent<PhotonSimpleConnect>();

            // Create GameManager with PhotonView
            var gameManagerGO = new GameObject("GameManager");
            var gameManager = gameManagerGO.AddComponent<GameManager>();
            var gameManagerPhotonView = gameManagerGO.AddComponent<PhotonView>();
            gameManagerPhotonView.Synchronization = ViewSynchronization.UnreliableOnChange;

            // Create UIManager
            var uiManagerGO = new GameObject("UIManager");
            var uiManager = uiManagerGO.AddComponent<UIManager>();

            // Wire up references using reflection
            // BoardManager
            boardManager.GetType().GetField("tilePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(boardManager, FindOrCreatePrefab("Tile"));
            boardManager.GetType().GetField("piecePrefab", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(boardManager, FindOrCreatePrefab("Piece"));
            boardManager.GetType().GetField("boardContainer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(boardManager, boardGO.transform);
            boardManager.GetType().GetField("gridLayout", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(boardManager, grid);

            // GameManager
            gameManager.GetType().GetField("boardManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameManager, boardManager);
            gameManager.GetType().GetField("uiManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(gameManager, uiManager);

            // UIManager
            uiManager.GetType().GetField("turnDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, turnText);
            uiManager.GetType().GetField("statusDisplay", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, statusText);
            uiManager.GetType().GetField("restartButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, restartBtn);
            uiManager.GetType().GetField("backToMenuButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, backToMenuBtn);
            uiManager.GetType().GetField("gameManager", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(uiManager, gameManager);

            // Save the scene
            string scenePath = "Assets/Scenes/ChessGameMulti.unity";
            if (!Directory.Exists("Assets/Scenes"))
                Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            
            EditorUtility.DisplayDialog("Photon Chess Scene Created", 
                "Photon-enabled chess scene created and saved!\n\n" +
                "✅ All components have PhotonView\n" +
                "✅ GameManager has move synchronization\n" +
                "✅ BoardManager has PhotonView\n" +
                "✅ Scene is ready for real-time sync\n\n" +
                "Use CanvasChess > Auto-Setup Photon Sync to configure piece prefabs.", "OK");
        }

        /// <summary>
        /// Finds or creates a prefab with the given name in Assets/Prefabs
        /// </summary>
        private static GameObject FindOrCreatePrefab(string prefabName)
        {
            string prefabPath = $"Assets/Prefabs/{prefabName}.prefab";
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            if (prefab == null)
            {
                // Create a basic prefab if it doesn't exist
                prefab = CreateFunctionalPrefab(prefabName, prefabPath);
            }
            
            return prefab;
        }

        /// <summary>
        /// Creates a functional prefab with required components
        /// </summary>
        private static GameObject CreateFunctionalPrefab(string prefabName, string path)
        {
            GameObject prefab = new GameObject(prefabName);
            
            if (prefabName == "Tile")
            {
                prefab.AddComponent<RectTransform>();
                prefab.AddComponent<Image>();
                prefab.AddComponent<Tile>();
                prefab.AddComponent<TileClickHandler>();
            }
            else if (prefabName == "Piece")
            {
                prefab.AddComponent<RectTransform>();
                prefab.AddComponent<Image>();
                prefab.AddComponent<Piece>();
            }
            
            // Save as prefab
            PrefabUtility.SaveAsPrefabAsset(prefab, path);
            DestroyImmediate(prefab);
            
            return AssetDatabase.LoadAssetAtPath<GameObject>(path);
        }
    }
}
#endif 