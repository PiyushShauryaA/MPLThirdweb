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
    /// Custom editor utility to auto-generate the MainMenu scene
    /// </summary>
    public class MainMenuSceneBuilder : EditorWindow
    {
        [MenuItem("CanvasChess/Auto-Build Main Menu Scene")]
        public static void ShowWindow()
        {
            GetWindow<MainMenuSceneBuilder>("Main Menu Scene Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Auto-Build Main Menu Scene", EditorStyles.boldLabel);
            if (GUILayout.Button("Create Main Menu Scene"))
            {
                CreateMainMenuScene();
            }
        }

        /// <summary>
        /// Creates the full MainMenu scene with all required objects and wiring
        /// </summary>
        public static void CreateMainMenuScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenu";

            // Create Canvas
            var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGO.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGO.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;

            // Create background
            var backgroundGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundGO.transform.SetParent(canvasGO.transform, false);
            var backgroundRect = backgroundGO.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            var backgroundImage = backgroundGO.GetComponent<Image>();
            backgroundImage.color = new Color(0.1f, 0.1f, 0.15f, 1f); // Dark blue-gray

            // Create main menu container with CanvasGroup for fade animation
            var menuContainerGO = new GameObject("MenuContainer", typeof(RectTransform), typeof(CanvasGroup));
            menuContainerGO.transform.SetParent(canvasGO.transform, false);
            var menuContainerRect = menuContainerGO.GetComponent<RectTransform>();
            menuContainerRect.anchorMin = Vector2.zero;
            menuContainerRect.anchorMax = Vector2.one;
            menuContainerRect.offsetMin = Vector2.zero;
            menuContainerRect.offsetMax = Vector2.zero;
            var menuContainerGroup = menuContainerGO.GetComponent<CanvasGroup>();

            // Create Title
            var titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGO.transform.SetParent(menuContainerGO.transform, false);
            var titleText = titleGO.GetComponent<TextMeshProUGUI>();
            titleText.text = "Chess Game";
            titleText.fontSize = 72;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.7f);
            titleRect.anchorMax = new Vector2(0.5f, 0.7f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(800, 100);

            // Create Version Text
            var versionGO = new GameObject("Version", typeof(RectTransform), typeof(TextMeshProUGUI));
            versionGO.transform.SetParent(menuContainerGO.transform, false);
            var versionText = versionGO.GetComponent<TextMeshProUGUI>();
            versionText.text = "v1.0.0";
            versionText.fontSize = 24;
            versionText.color = new Color(1f, 1f, 1f, 0.7f);
            versionText.alignment = TextAlignmentOptions.Center;
            var versionRect = versionGO.GetComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(0.5f, 0.6f);
            versionRect.anchorMax = new Vector2(0.5f, 0.6f);
            versionRect.pivot = new Vector2(0.5f, 0.5f);
            versionRect.anchoredPosition = Vector2.zero;
            versionRect.sizeDelta = new Vector2(200, 40);

            // Create Play Button
            var playBtnGO = new GameObject("PlayButton", typeof(RectTransform), typeof(Button), typeof(Image));
            playBtnGO.transform.SetParent(menuContainerGO.transform, false);
            var playRect = playBtnGO.GetComponent<RectTransform>();
            playRect.anchorMin = new Vector2(0.5f, 0.45f);
            playRect.anchorMax = new Vector2(0.5f, 0.45f);
            playRect.pivot = new Vector2(0.5f, 0.5f);
            playRect.anchoredPosition = Vector2.zero;
            playRect.sizeDelta = new Vector2(300, 80);
            var playBtn = playBtnGO.GetComponent<Button>();
            var playImg = playBtnGO.GetComponent<Image>();
            playImg.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green

            // Add TMP text to play button
            var playTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            playTextGO.transform.SetParent(playBtnGO.transform, false);
            var playText = playTextGO.GetComponent<TextMeshProUGUI>();
            playText.text = "Play Game";
            playText.fontSize = 36;
            playText.color = Color.white;
            playText.alignment = TextAlignmentOptions.Center;
            var playTextRect = playTextGO.GetComponent<RectTransform>();
            playTextRect.anchorMin = Vector2.zero;
            playTextRect.anchorMax = Vector2.one;
            playTextRect.offsetMin = Vector2.zero;
            playTextRect.offsetMax = Vector2.zero;

            // Create Play AI Button
            var playAIBtnGO = new GameObject("PlayAIButton", typeof(RectTransform), typeof(Button), typeof(Image));
            playAIBtnGO.transform.SetParent(menuContainerGO.transform, false);
            var playAIRect = playAIBtnGO.GetComponent<RectTransform>();
            playAIRect.anchorMin = new Vector2(0.5f, 0.3f);
            playAIRect.anchorMax = new Vector2(0.5f, 0.3f);
            playAIRect.pivot = new Vector2(0.5f, 0.5f);
            playAIRect.anchoredPosition = Vector2.zero;
            playAIRect.sizeDelta = new Vector2(300, 80);
            var playAIBtn = playAIBtnGO.GetComponent<Button>();
            var playAIImg = playAIBtnGO.GetComponent<Image>();
            playAIImg.color = new Color(0.2f, 0.4f, 0.8f, 1f); // Blue

            // Add TMP text to play AI button
            var playAITextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            playAITextGO.transform.SetParent(playAIBtnGO.transform, false);
            var playAIText = playAITextGO.GetComponent<TextMeshProUGUI>();
            playAIText.text = "Play vs AI";
            playAIText.fontSize = 36;
            playAIText.color = Color.white;
            playAIText.alignment = TextAlignmentOptions.Center;
            var playAITextRect = playAITextGO.GetComponent<RectTransform>();
            playAITextRect.anchorMin = Vector2.zero;
            playAITextRect.anchorMax = Vector2.one;
            playAITextRect.offsetMin = Vector2.zero;
            playAITextRect.offsetMax = Vector2.zero;

            // Create Quit Button
            var quitBtnGO = new GameObject("QuitButton", typeof(RectTransform), typeof(Button), typeof(Image));
            quitBtnGO.transform.SetParent(menuContainerGO.transform, false);
            var quitRect = quitBtnGO.GetComponent<RectTransform>();
            quitRect.anchorMin = new Vector2(0.5f, 0.15f);
            quitRect.anchorMax = new Vector2(0.5f, 0.15f);
            quitRect.pivot = new Vector2(0.5f, 0.5f);
            quitRect.anchoredPosition = Vector2.zero;
            quitRect.sizeDelta = new Vector2(300, 80);
            var quitBtn = quitBtnGO.GetComponent<Button>();
            var quitImg = quitBtnGO.GetComponent<Image>();
            quitImg.color = new Color(0.6f, 0.2f, 0.2f, 1f); // Red

            // Add TMP text to quit button
            var quitTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            quitTextGO.transform.SetParent(quitBtnGO.transform, false);
            var quitText = quitTextGO.GetComponent<TextMeshProUGUI>();
            quitText.text = "Quit Game";
            quitText.fontSize = 36;
            quitText.color = Color.white;
            quitText.alignment = TextAlignmentOptions.Center;
            var quitTextRect = quitTextGO.GetComponent<RectTransform>();
            quitTextRect.anchorMin = Vector2.zero;
            quitTextRect.anchorMax = Vector2.one;
            quitTextRect.offsetMin = Vector2.zero;
            quitTextRect.offsetMax = Vector2.zero;

            // Create EventSystem (required for input)
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // Create MainMenuManager
            var menuManagerGO = new GameObject("MainMenuManager");
            var menuManager = menuManagerGO.AddComponent<MainMenuManager>();

            // Wire up references using reflection
            menuManager.GetType().GetField("playButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(menuManager, playBtn);
            menuManager.GetType().GetField("playAIButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(menuManager, playAIBtn);
            menuManager.GetType().GetField("quitButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(menuManager, quitBtn);
            menuManager.GetType().GetField("titleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(menuManager, titleText);
            menuManager.GetType().GetField("versionText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(menuManager, versionText);
            menuManager.GetType().GetField("mainMenuGroup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(menuManager, menuContainerGroup);

            // Create Camera
            var cameraGO = new GameObject("Camera", typeof(Camera), typeof(AudioListener));
            var camera = cameraGO.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.1f, 0.1f, 0.15f, 1f);
            camera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0, 0, -10);

            // Save the scene
            string scenePath = "Assets/Scenes/MainMenu.unity";
            if (!Directory.Exists("Assets/Scenes"))
                Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            
            EditorUtility.DisplayDialog("Main Menu Scene Builder", 
                "MainMenu scene created and saved!\n\n" +
                "Features included:\n" +
                "• Professional UI layout\n" +
                "• Fade in/out animations\n" +
                "• Play and Quit buttons\n" +
                "• Version display\n" +
                "• Proper scene transitions", "OK");
        }
    }
}
#endif 