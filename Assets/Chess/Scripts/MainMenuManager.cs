using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace CanvasChess
{
    /// <summary>
    /// Manages the main menu scene and handles scene transitions
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private Button playButton = null!;
        [SerializeField] private Button playAIButton = null!;
        [SerializeField] private Button quitButton = null!;
        [SerializeField] private TextMeshProUGUI titleText = null!;
        [SerializeField] private TextMeshProUGUI versionText = null!;
        [SerializeField] private Button playAsWhiteButton = null!;
        [SerializeField] private Button playAsBlackButton = null!;
        
        [Header("Animation")]
        [SerializeField] private CanvasGroup mainMenuGroup = null!;
        [SerializeField] private float fadeInDuration = 1f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        
        [Header("Settings")]
        [SerializeField] private string chessGameSceneName = "ChessGame";
        [SerializeField] private string chessAISceneName = "ChessGameVS_AI";
        [SerializeField] private string chessMultiSceneName = "ChessGameMulti";
        [SerializeField] private string mainMenuMultiSceneName = "MainMenuMulti";
        [SerializeField] private string gameVersion = "v1.0.0";
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            InitializeUI();
            SetupButtonListeners();
            StartFadeIn();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the UI elements
        /// </summary>
        private void InitializeUI()
        {
            if (titleText != null)
            {
                titleText.text = "Chess Game";
                titleText.fontSize = 72;
                titleText.color = Color.white;
            }
            
            if (versionText != null)
            {
                versionText.text = gameVersion;
                versionText.fontSize = 24;
                versionText.color = new Color(1f, 1f, 1f, 0.7f);
            }
            
            if (mainMenuGroup != null)
            {
                mainMenuGroup.alpha = 0f;
            }
        }

        /// <summary>
        /// Sets up button click listeners
        /// </summary>
        private void SetupButtonListeners()
        {
            if (playButton != null)
            {
                playButton.onClick.AddListener(OnPlayButtonClicked);
            }
            
            if (playAIButton != null)
            {
                playAIButton.onClick.AddListener(OnPlayAIButtonClicked);
            }
            
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitButtonClicked);
            }
            
            if (playAsWhiteButton != null)
            {
                playAsWhiteButton.onClick.AddListener(OnPlayAsWhiteClicked);
            }
            
            if (playAsBlackButton != null)
            {
                playAsBlackButton.onClick.AddListener(OnPlayAsBlackClicked);
            }
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Handles play button click - transitions to chess game scene
        /// </summary>
        private void OnPlayButtonClicked()
        {
            Debug.Log("Play button clicked - transitioning to chess game");
            StartFadeOut(() => LoadChessGameScene());
        }

        /// <summary>
        /// Handles play AI button click - transitions to chess AI game scene
        /// </summary>
        private void OnPlayAIButtonClicked()
        {
            Debug.Log("Play AI button clicked - transitioning to chess AI game");
            StartFadeOut(() => LoadChessAIScene());
        }

        /// <summary>
        /// Handles quit button click
        /// </summary>
        private void OnQuitButtonClicked()
        {
            Debug.Log("Quit button clicked");
            
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        /// <summary>
        /// Handles play as white button click - sets player color to white and transitions to chess AI game scene
        /// </summary>
        private void OnPlayAsWhiteClicked()
        {
            PlayerSettingsChess.PlayerColor = PieceColor.White;
            Debug.Log("Play as White selected");
            StartFadeOut(() => LoadChessAIScene());
        }

        /// <summary>
        /// Handles play as black button click - sets player color to black and transitions to chess AI game scene
        /// </summary>
        private void OnPlayAsBlackClicked()
        {
            PlayerSettingsChess.PlayerColor = PieceColor.Black;
            Debug.Log("Play as Black selected");
            StartFadeOut(() => LoadChessAIScene());
        }

        /// <summary>
        /// Handles play multiplayer button click - transitions to chess multiplayer scene
        /// </summary>
        private void OnPlayMultiplayerClicked()
        {
            Debug.Log("Play Multiplayer selected");
            StartFadeOut(() => LoadChessMultiScene());
        }

        /// <summary>
        /// Handles play multiplayer with connection button click - transitions to MainMenuMulti scene
        /// </summary>
        private void OnPlayMultiplayerWithConnectionClicked()
        {
            Debug.Log("Play Multiplayer with Connection selected");
            StartFadeOut(() => LoadMainMenuMultiScene());
        }
        #endregion

        #region Scene Management
        /// <summary>
        /// Loads the chess game scene
        /// </summary>
        private void LoadChessGameScene()
        {
            SceneManager.LoadScene(chessGameSceneName);
        }

        /// <summary>
        /// Loads the chess AI game scene
        /// </summary>
        private void LoadChessAIScene()
        {
            SceneManager.LoadScene(chessAISceneName);
        }

        /// <summary>
        /// Loads the chess multiplayer game scene
        /// </summary>
        private void LoadChessMultiScene()
        {
            SceneManager.LoadScene(chessMultiSceneName);
        }

        /// <summary>
        /// Loads the MainMenuMulti scene
        /// </summary>
        private void LoadMainMenuMultiScene()
        {
            SceneManager.LoadScene(mainMenuMultiSceneName);
        }
        #endregion

        #region Animation
        /// <summary>
        /// Starts the fade in animation
        /// </summary>
        private void StartFadeIn()
        {
            if (mainMenuGroup != null)
            {
                StartCoroutine(FadeInCoroutine());
            }
        }

        /// <summary>
        /// Starts the fade out animation
        /// </summary>
        /// <param name="onComplete">Callback when fade out is complete</param>
        private void StartFadeOut(System.Action onComplete = null)
        {
            if (mainMenuGroup != null)
            {
                StartCoroutine(FadeOutCoroutine(onComplete));
            }
            else
            {
                onComplete?.Invoke();
            }
        }

        /// <summary>
        /// Coroutine for fade in animation
        /// </summary>
        private System.Collections.IEnumerator FadeInCoroutine()
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                mainMenuGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
                yield return null;
            }
            
            mainMenuGroup.alpha = 1f;
        }

        /// <summary>
        /// Coroutine for fade out animation
        /// </summary>
        /// <param name="onComplete">Callback when fade out is complete</param>
        private System.Collections.IEnumerator FadeOutCoroutine(System.Action onComplete = null)
        {
            float elapsedTime = 0f;
            
            while (elapsedTime < fadeOutDuration)
            {
                elapsedTime += Time.deltaTime;
                mainMenuGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
                yield return null;
            }
            
            mainMenuGroup.alpha = 0f;
            onComplete?.Invoke();
        }
        #endregion
    }
} 