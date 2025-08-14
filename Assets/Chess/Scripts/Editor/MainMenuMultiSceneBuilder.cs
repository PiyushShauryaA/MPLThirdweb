#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;
using UnityEditor.SceneManagement;
using System.IO;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Photon.Pun;

namespace CanvasChess.Editor
{
    /// <summary>
    /// Auto-generates the MainMenuMulti scene for Photon connection
    /// </summary>
    public class MainMenuMultiSceneBuilder : EditorWindow
    {
        [MenuItem("CanvasChess/Auto-Build MainMenuMulti Scene")]
        public static void ShowWindow()
        {
            GetWindow<MainMenuMultiSceneBuilder>("MainMenuMulti Scene Builder");
        }

        private void OnGUI()
        {
            GUILayout.Label("Auto-Build MainMenuMulti Scene", EditorStyles.boldLabel);
            GUILayout.Label("This will create a Photon connection scene for multiplayer chess.");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Create MainMenuMulti Scene"))
            {
                CreateMainMenuMultiScene();
            }
        }

        /// <summary>
        /// Creates the MainMenuMulti scene with Photon connection UI
        /// </summary>
        public static void CreateMainMenuMultiScene()
        {
            // Create new scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainMenuMulti";

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

            // Create main container
            var mainContainerGO = new GameObject("MainContainer", typeof(RectTransform), typeof(CanvasGroup));
            mainContainerGO.transform.SetParent(canvasGO.transform, false);
            var mainContainerRect = mainContainerGO.GetComponent<RectTransform>();
            mainContainerRect.anchorMin = Vector2.zero;
            mainContainerRect.anchorMax = Vector2.one;
            mainContainerRect.offsetMin = Vector2.zero;
            mainContainerRect.offsetMax = Vector2.zero;

            // Create Title
            var titleGO = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleGO.transform.SetParent(mainContainerGO.transform, false);
            var titleText = titleGO.GetComponent<TextMeshProUGUI>();
            titleText.text = "Chess Multiplayer";
            titleText.fontSize = 72;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;
            var titleRect = titleGO.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0.5f, 0.8f);
            titleRect.anchorMax = new Vector2(0.5f, 0.8f);
            titleRect.pivot = new Vector2(0.5f, 0.5f);
            titleRect.anchoredPosition = Vector2.zero;
            titleRect.sizeDelta = new Vector2(800, 100);

            // Create Username Input
            var usernameInputGO = new GameObject("UsernameInput", typeof(RectTransform), typeof(TMP_InputField), typeof(Image));
            usernameInputGO.transform.SetParent(mainContainerGO.transform, false);
            var usernameInputRect = usernameInputGO.GetComponent<RectTransform>();
            usernameInputRect.anchorMin = new Vector2(0.5f, 0.6f);
            usernameInputRect.anchorMax = new Vector2(0.5f, 0.6f);
            usernameInputRect.pivot = new Vector2(0.5f, 0.5f);
            usernameInputRect.anchoredPosition = Vector2.zero;
            usernameInputRect.sizeDelta = new Vector2(400, 60);
            var usernameInput = usernameInputGO.GetComponent<TMP_InputField>();
            var usernameInputImage = usernameInputGO.GetComponent<Image>();
            usernameInputImage.color = new Color(1f, 1f, 1f, 0.9f);

            // Add placeholder text to username input
            var placeholderGO = new GameObject("Placeholder", typeof(RectTransform), typeof(TextMeshProUGUI));
            placeholderGO.transform.SetParent(usernameInputGO.transform, false);
            var placeholderText = placeholderGO.GetComponent<TextMeshProUGUI>();
            placeholderText.text = "Enter your username...";
            placeholderText.fontSize = 24;
            placeholderText.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholderText.alignment = TextAlignmentOptions.Center;
            var placeholderRect = placeholderGO.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = new Vector2(10, 0);
            placeholderRect.offsetMax = new Vector2(-10, 0);

            // Add text component to username input
            var textGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textGO.transform.SetParent(usernameInputGO.transform, false);
            var textComponent = textGO.GetComponent<TextMeshProUGUI>();
            textComponent.fontSize = 24;
            textComponent.color = Color.black;
            textComponent.alignment = TextAlignmentOptions.Center;
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(10, 0);
            textRect.offsetMax = new Vector2(-10, 0);

            // Wire up the input field
            usernameInput.textComponent = textComponent;
            usernameInput.placeholder = placeholderText;

            // Create Connect Button
            var connectBtnGO = new GameObject("ConnectButton", typeof(RectTransform), typeof(Button), typeof(Image));
            connectBtnGO.transform.SetParent(mainContainerGO.transform, false);
            var connectRect = connectBtnGO.GetComponent<RectTransform>();
            connectRect.anchorMin = new Vector2(0.5f, 0.45f);
            connectRect.anchorMax = new Vector2(0.5f, 0.45f);
            connectRect.pivot = new Vector2(0.5f, 0.5f);
            connectRect.anchoredPosition = Vector2.zero;
            connectRect.sizeDelta = new Vector2(300, 80);
            var connectBtn = connectBtnGO.GetComponent<Button>();
            var connectImg = connectBtnGO.GetComponent<Image>();
            connectImg.color = new Color(0.2f, 0.6f, 0.2f, 1f); // Green

            // Add TMP text to connect button
            var connectTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            connectTextGO.transform.SetParent(connectBtnGO.transform, false);
            var connectText = connectTextGO.GetComponent<TextMeshProUGUI>();
            connectText.text = "Connect & Join Game";
            connectText.fontSize = 28;
            connectText.color = Color.white;
            connectText.alignment = TextAlignmentOptions.Center;
            var connectTextRect = connectTextGO.GetComponent<RectTransform>();
            connectTextRect.anchorMin = Vector2.zero;
            connectTextRect.anchorMax = Vector2.one;
            connectTextRect.offsetMin = Vector2.zero;
            connectTextRect.offsetMax = Vector2.zero;

            // Create Timer Selection Section
            var timerSectionGO = new GameObject("TimerSection", typeof(RectTransform));
            timerSectionGO.transform.SetParent(mainContainerGO.transform, false);
            var timerSectionRect = timerSectionGO.GetComponent<RectTransform>();
            timerSectionRect.anchorMin = new Vector2(0.5f, 0.35f);
            timerSectionRect.anchorMax = new Vector2(0.5f, 0.35f);
            timerSectionRect.pivot = new Vector2(0.5f, 0.5f);
            timerSectionRect.anchoredPosition = Vector2.zero;
            timerSectionRect.sizeDelta = new Vector2(600, 200);

            // Create Timer Title
            var timerTitleGO = new GameObject("TimerTitle", typeof(RectTransform), typeof(TextMeshProUGUI));
            timerTitleGO.transform.SetParent(timerSectionGO.transform, false);
            var timerTitleText = timerTitleGO.GetComponent<TextMeshProUGUI>();
            timerTitleText.text = "Select Timer:";
            timerTitleText.fontSize = 24;
            timerTitleText.color = Color.white;
            timerTitleText.alignment = TextAlignmentOptions.Center;
            timerTitleText.fontStyle = FontStyles.Bold;
            var timerTitleRect = timerTitleGO.GetComponent<RectTransform>();
            timerTitleRect.anchorMin = new Vector2(0.5f, 0.8f);
            timerTitleRect.anchorMax = new Vector2(0.5f, 0.8f);
            timerTitleRect.pivot = new Vector2(0.5f, 0.5f);
            timerTitleRect.anchoredPosition = Vector2.zero;
            timerTitleRect.sizeDelta = new Vector2(400, 50);

            // Create Timer Toggles Container
            var timerTogglesGO = new GameObject("TimerToggles", typeof(RectTransform), typeof(VerticalLayoutGroup));
            timerTogglesGO.transform.SetParent(timerSectionGO.transform, false);
            var timerTogglesRect = timerTogglesGO.GetComponent<RectTransform>();
            timerTogglesRect.anchorMin = new Vector2(0.5f, 0.2f);
            timerTogglesRect.anchorMax = new Vector2(0.5f, 0.7f);
            timerTogglesRect.pivot = new Vector2(0.5f, 0.5f);
            timerTogglesRect.anchoredPosition = Vector2.zero;
            timerTogglesRect.sizeDelta = new Vector2(400, 200);
            var timerTogglesLayout = timerTogglesGO.GetComponent<VerticalLayoutGroup>();
            timerTogglesLayout.spacing = 10;
            timerTogglesLayout.childControlHeight = true;
            timerTogglesLayout.childControlWidth = true;

            // Create Rapid Toggle
            var rapidToggleGO = CreateTimerToggle("RapidToggle", "Rapid (10 min)", timerTogglesGO.transform);
            var rapidToggle = rapidToggleGO.GetComponent<Toggle>();
            rapidToggle.isOn = true; // Default to Rapid

            // Create Blitz Toggle
            var blitzToggleGO = CreateTimerToggle("BlitzToggle", "Blitz (5 min)", timerTogglesGO.transform);
            var blitzToggle = blitzToggleGO.GetComponent<Toggle>();

            // Create Bullet Toggle
            var bulletToggleGO = CreateTimerToggle("BulletToggle", "Bullet (3 min)", timerTogglesGO.transform);
            var bulletToggle = bulletToggleGO.GetComponent<Toggle>();

            // Create Selected Timer Display
            var selectedTimerGO = new GameObject("SelectedTimer", typeof(RectTransform), typeof(TextMeshProUGUI));
            selectedTimerGO.transform.SetParent(timerSectionGO.transform, false);
            var selectedTimerText = selectedTimerGO.GetComponent<TextMeshProUGUI>();
            selectedTimerText.text = "Selected: Rapid (10 minutes)";
            selectedTimerText.fontSize = 18;
            selectedTimerText.color = Color.yellow;
            selectedTimerText.alignment = TextAlignmentOptions.Center;
            var selectedTimerRect = selectedTimerGO.GetComponent<RectTransform>();
            selectedTimerRect.anchorMin = new Vector2(0.5f, 0.05f);
            selectedTimerRect.anchorMax = new Vector2(0.5f, 0.05f);
            selectedTimerRect.pivot = new Vector2(0.5f, 0.5f);
            selectedTimerRect.anchoredPosition = Vector2.zero;
            selectedTimerRect.sizeDelta = new Vector2(400, 50);

            // Create Status Text
            var statusTextGO = new GameObject("StatusText", typeof(RectTransform), typeof(TextMeshProUGUI));
            statusTextGO.transform.SetParent(mainContainerGO.transform, false);
            var statusText = statusTextGO.GetComponent<TextMeshProUGUI>();
            statusText.text = "Enter username and press Connect.";
            statusText.fontSize = 20;
            statusText.color = Color.white;
            statusText.alignment = TextAlignmentOptions.Center;
            var statusRect = statusTextGO.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0.25f);
            statusRect.anchorMax = new Vector2(0.5f, 0.25f);
            statusRect.pivot = new Vector2(0.5f, 0.5f);
            statusRect.anchoredPosition = Vector2.zero;
            statusRect.sizeDelta = new Vector2(600, 100);

            // Create Back to Menu Button
            var backBtnGO = new GameObject("BackButton", typeof(RectTransform), typeof(Button), typeof(Image));
            backBtnGO.transform.SetParent(mainContainerGO.transform, false);
            var backRect = backBtnGO.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0.5f, 0.15f);
            backRect.anchorMax = new Vector2(0.5f, 0.15f);
            backRect.pivot = new Vector2(0.5f, 0.5f);
            backRect.anchoredPosition = Vector2.zero;
            backRect.sizeDelta = new Vector2(300, 80);
            var backBtn = backBtnGO.GetComponent<Button>();
            var backImg = backBtnGO.GetComponent<Image>();
            backImg.color = new Color(0.6f, 0.2f, 0.2f, 1f); // Red

            // Add TMP text to back button
            var backTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            backTextGO.transform.SetParent(backBtnGO.transform, false);
            var backText = backTextGO.GetComponent<TextMeshProUGUI>();
            backText.text = "Back to Menu";
            backText.fontSize = 28;
            backText.color = Color.white;
            backText.alignment = TextAlignmentOptions.Center;
            var backTextRect = backTextGO.GetComponent<RectTransform>();
            backTextRect.anchorMin = Vector2.zero;
            backTextRect.anchorMax = Vector2.one;
            backTextRect.offsetMin = Vector2.zero;
            backTextRect.offsetMax = Vector2.zero;

            // Create EventSystem (required for input)
            var eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // Create PhotonSimpleConnect
            var photonConnectGO = new GameObject("PhotonConnect");
            var photonConnect = photonConnectGO.AddComponent<PhotonSimpleConnect>();
            
            // Add PhotonView for RPC calls
            var photonView = photonConnectGO.AddComponent<PhotonView>();
            photonView.Synchronization = ViewSynchronization.UnreliableOnChange;

            // Wire up references using reflection
            photonConnect.GetType().GetField("connectButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(photonConnect, connectBtn);
            photonConnect.GetType().GetField("statusText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(photonConnect, statusText);
            photonConnect.GetType().GetField("usernameInput", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(photonConnect, usernameInput);
            
            // Wire up timer selection references
            photonConnect.GetType().GetField("rapidToggle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(photonConnect, rapidToggle);
            photonConnect.GetType().GetField("blitzToggle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(photonConnect, blitzToggle);
            photonConnect.GetType().GetField("bulletToggle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(photonConnect, bulletToggle);
            photonConnect.GetType().GetField("selectedTimerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(photonConnect, selectedTimerText);

            // Add back button functionality
            backBtn.onClick.AddListener(() => SceneManager.LoadScene("MainMenu"));

            // Add manual switch button for testing
            var manualSwitchBtnGO = new GameObject("ManualSwitchButton", typeof(RectTransform), typeof(Button), typeof(Image));
            manualSwitchBtnGO.transform.SetParent(mainContainerGO.transform, false);
            var manualSwitchRect = manualSwitchBtnGO.GetComponent<RectTransform>();
            manualSwitchRect.anchorMin = new Vector2(0.5f, 0.05f);
            manualSwitchRect.anchorMax = new Vector2(0.5f, 0.05f);
            manualSwitchRect.pivot = new Vector2(0.5f, 0.5f);
            manualSwitchRect.anchoredPosition = Vector2.zero;
            manualSwitchRect.sizeDelta = new Vector2(300, 60);
            var manualSwitchBtn = manualSwitchBtnGO.GetComponent<Button>();
            var manualSwitchImg = manualSwitchBtnGO.GetComponent<Image>();
            manualSwitchImg.color = new Color(0.8f, 0.4f, 0.2f, 1f); // Orange

            // Add TMP text to manual switch button
            var manualSwitchTextGO = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            manualSwitchTextGO.transform.SetParent(manualSwitchBtnGO.transform, false);
            var manualSwitchText = manualSwitchTextGO.GetComponent<TextMeshProUGUI>();
            manualSwitchText.text = "Manual Switch to Game";
            manualSwitchText.fontSize = 20;
            manualSwitchText.color = Color.white;
            manualSwitchText.alignment = TextAlignmentOptions.Center;
            var manualSwitchTextRect = manualSwitchTextGO.GetComponent<RectTransform>();
            manualSwitchTextRect.anchorMin = Vector2.zero;
            manualSwitchTextRect.anchorMax = Vector2.one;
            manualSwitchTextRect.offsetMin = Vector2.zero;
            manualSwitchTextRect.offsetMax = Vector2.zero;

            // Wire up manual switch button
            manualSwitchBtn.onClick.AddListener(() => photonConnect.ManualSwitchToGame());

            // Save the scene
            string scenePath = "Assets/Scenes/MainMenuMulti.unity";
            if (!Directory.Exists("Assets/Scenes"))
                Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, scenePath);
            
            EditorUtility.DisplayDialog("MainMenuMulti Scene Created", 
                "MainMenuMulti scene created and saved!\n\n" +
                "✅ Photon connection UI ready\n" +
                "✅ Auto-switch to ChessGameMulti when 2 players join\n" +
                "✅ Username input and connect button\n" +
                "✅ Status display for connection progress\n" +
                "✅ Timer selection (Rapid/Blitz/Bullet)\n\n" +
                "Scene is ready for multiplayer connection flow.", "OK");
        }
        
        /// <summary>
        /// Creates a timer toggle button
        /// </summary>
        /// <param name="name">Name of the toggle</param>
        /// <param name="label">Label text</param>
        /// <param name="parent">Parent transform</param>
        /// <returns>The created toggle GameObject</returns>
        private static GameObject CreateTimerToggle(string name, string label, Transform parent)
        {
            var toggleGO = new GameObject(name, typeof(RectTransform), typeof(Toggle), typeof(Image));
            toggleGO.transform.SetParent(parent, false);
            var toggle = toggleGO.GetComponent<Toggle>();
            var toggleImg = toggleGO.GetComponent<Image>();
            toggleImg.color = new Color(0.2f, 0.2f, 0.3f, 1f);
            
            // Create background
            var backgroundGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
            backgroundGO.transform.SetParent(toggleGO.transform, false);
            var backgroundRect = backgroundGO.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.offsetMin = Vector2.zero;
            backgroundRect.offsetMax = Vector2.zero;
            var backgroundImg = backgroundGO.GetComponent<Image>();
            backgroundImg.color = new Color(0.3f, 0.3f, 0.4f, 1f);
            
            // Create checkmark
            var checkmarkGO = new GameObject("Checkmark", typeof(RectTransform), typeof(Image));
            checkmarkGO.transform.SetParent(toggleGO.transform, false);
            var checkmarkRect = checkmarkGO.GetComponent<RectTransform>();
            checkmarkRect.anchorMin = Vector2.zero;
            checkmarkRect.anchorMax = Vector2.one;
            checkmarkRect.offsetMin = Vector2.zero;
            checkmarkRect.offsetMax = Vector2.zero;
            var checkmarkImg = checkmarkGO.GetComponent<Image>();
            checkmarkImg.color = new Color(0.2f, 0.8f, 0.2f, 1f);
            
            // Create label
            var labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(toggleGO.transform, false);
            var labelText = labelGO.GetComponent<TextMeshProUGUI>();
            labelText.text = label;
            labelText.fontSize = 20;
            labelText.color = Color.white;
            labelText.alignment = TextAlignmentOptions.Left;
            var labelRect = labelGO.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(50, 0);
            labelRect.offsetMax = Vector2.zero;
            
            // Wire up toggle
            toggle.targetGraphic = toggleImg;
            toggle.graphic = checkmarkImg;
            
            return toggleGO;
        }
    }
}
#endif 