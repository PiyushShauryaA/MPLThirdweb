using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using CanvasChess.AI;
using TMPro;
using CanvasChess; // Ensure PlayerSettings is accessible

namespace CanvasChess
{
    /// <summary>
    /// AI extension component that works with the existing GameManager
    /// </summary>
    public class GameManagerAI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("AI Settings")]
        [SerializeField] private ChessAI chessAI = null!;
        [SerializeField] private PieceColor playerColor = PieceColor.White;
        [SerializeField] private bool aiEnabled = true;
        
        [Header("AI UI")]
        [SerializeField] private GameObject thinkingIndicator = null!;
        [SerializeField] private UnityEngine.UI.Slider aiDifficultySlider = null!;
        [SerializeField] private TextMeshProUGUI difficultyText = null!;
        [SerializeField] private UnityEngine.UI.Button toggleAIButton = null!;
        [SerializeField] private UnityEngine.UI.Button colorToggleButton = null!;
        [SerializeField] private TextMeshProUGUI toggleAIText = null!;
        [SerializeField] private TextMeshProUGUI colorToggleText = null!;

        // Timer fields
        [Header("Timer Settings")]
        private float whiteTimeLeft;
        private float blackTimeLeft;
        private bool whiteTimerActive = false;
        private bool blackTimerActive = false;
        private bool timersEnabled = true;
        private bool gameEnded = false;

        // Timer UI
        [SerializeField] private TextMeshProUGUI whiteTimerText;
        [SerializeField] private TextMeshProUGUI blackTimerText;
        #endregion

        #region Properties
        /// <summary>
        /// Whether the AI is enabled
        /// </summary>
        public bool AIEnabled => aiEnabled;

        /// <summary>
        /// The player's color
        /// </summary>
        public PieceColor PlayerColor => playerColor;

        /// <summary>
        /// The AI's color
        /// </summary>
        public PieceColor AIColor => playerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
        #endregion

        #region Private Fields
        private GameManager gameManager = null!;
        private BoardManager boardManager = null!;
        private bool isAITurn = false;
        private UIManager uiManager = null!;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            // Set player color from PlayerSettings
            playerColor = PlayerSettingsChess.PlayerColor;
            // Find required components
            gameManager = FindObjectOfType<GameManager>();
            boardManager = FindObjectOfType<BoardManager>();
            uiManager = FindObjectOfType<UIManager>();
            
            // Find AI if not assigned
            if (chessAI == null)
                chessAI = FindObjectOfType<ChessAI>();
            
            // Setup AI
            if (chessAI != null)
            {
                chessAI.AIColor = AIColor;
                chessAI.OnAIMoveReady += OnAIMoveComplete;
            }
            
            // Setup UI
            SetupAIUI();
            
            // Subscribe to game events
            if (gameManager != null)
            {
                gameManager.OnTurnChanged.AddListener(OnTurnChanged);
            }
            
            // Start AI thinking if it's AI's turn
            if (aiEnabled && gameManager != null && gameManager.CurrentTurn == AIColor)
            {
                StartAIThinking();
            }

            float timerDuration = PlayerSettingsChess.GetTimerDuration();
            whiteTimeLeft = timerDuration;
            blackTimeLeft = timerDuration;
            UpdateTimerUI();
            if (gameManager != null)
            {
                gameManager.OnTurnChanged.AddListener(OnTurnChanged);
            }
            // Start white's timer if white starts
            if (gameManager != null && gameManager.CurrentTurn == PieceColor.White)
            {
                whiteTimerActive = true;
            }
        }

        private void OnDestroy()
        {
            if (chessAI != null)
            {
                chessAI.OnAIMoveReady -= OnAIMoveComplete;
            }
            
            if (gameManager != null)
            {
                gameManager.OnTurnChanged.RemoveListener(OnTurnChanged);
            }
        }

        private void Update()
        {
            if (!timersEnabled || gameEnded) return;
            if (whiteTimerActive)
            {
                whiteTimeLeft -= Time.deltaTime;
                if (whiteTimeLeft <= 0)
                {
                    whiteTimeLeft = 0;
                    EndGameOnTimeout(PieceColor.Black);
                }
            }
            else if (blackTimerActive)
            {
                blackTimeLeft -= Time.deltaTime;
                if (blackTimeLeft <= 0)
                {
                    blackTimeLeft = 0;
                    EndGameOnTimeout(PieceColor.White);
                }
            }
            UpdateTimerUI();
        }
        #endregion

        #region AI Integration
        /// <summary>
        /// Called when the turn changes
        /// </summary>
        /// <param name="newTurn">The new turn color</param>
        private void OnTurnChanged(PieceColor newTurn)
        {
            if (!timersEnabled || gameEnded) return;
            isAITurn = aiEnabled && newTurn == AIColor && !gameManager.GameEnded;
            
            if (isAITurn)
            {
                StartAIThinking();
            }
            else
            {
                ShowThinkingIndicator(false);
            }

            if (newTurn == PieceColor.White)
            {
                whiteTimerActive = true;
                blackTimerActive = false;
            }
            else
            {
                whiteTimerActive = false;
                blackTimerActive = true;
            }
        }

        /// <summary>
        /// Starts the AI thinking process
        /// </summary>
        private void StartAIThinking()
        {
            if (chessAI != null && !chessAI.IsThinking && aiEnabled)
            {
                ShowThinkingIndicator(true);
                chessAI.StartThinking();
            }
        }

        /// <summary>
        /// Called when AI completes its move
        /// </summary>
        /// <param name="move">The move the AI made</param>
        private void OnAIMoveComplete(Move move)
        {
            ShowThinkingIndicator(false);
            Debug.Log($"AI moved {move.Piece.Type} from ({move.FromX},{move.FromY}) to ({move.ToX},{move.ToY})");
        }

        /// <summary>
        /// Shows or hides the thinking indicator
        /// </summary>
        /// <param name="show">Whether to show the indicator</param>
        private void ShowThinkingIndicator(bool show)
        {
            if (thinkingIndicator != null)
            {
                thinkingIndicator.SetActive(show);
            }
        }
        #endregion

        #region AI UI Setup
        /// <summary>
        /// Sets up the AI-related UI elements
        /// </summary>
        private void SetupAIUI()
        {
            if (aiDifficultySlider != null)
            {
                aiDifficultySlider.onValueChanged.AddListener(OnDifficultyChanged);
                aiDifficultySlider.value = 3; // Default difficulty
                OnDifficultyChanged(3);
            }
            
            if (toggleAIButton != null)
            {
                toggleAIButton.onClick.AddListener(OnToggleAIClicked);
            }
            
            if (colorToggleButton != null)
            {
                colorToggleButton.onClick.AddListener(OnColorToggleClicked);
            }
            
            UpdateUI();
        }

        /// <summary>
        /// Called when AI difficulty is changed
        /// </summary>
        /// <param name="difficulty">The new difficulty level</param>
        private void OnDifficultyChanged(float difficulty)
        {
            if (chessAI != null)
            {
                int searchDepth = Mathf.RoundToInt(difficulty);
                chessAI.GetType().GetField("searchDepth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(chessAI, searchDepth);
            }
            
            if (difficultyText != null)
            {
                string difficultyName = difficulty switch
                {
                    1 => "Easy",
                    2 => "Medium",
                    3 => "Hard",
                    4 => "Expert",
                    5 => "Master",
                    _ => "Custom"
                };
                difficultyText.text = $"AI Difficulty: {difficultyName}";
            }
        }

        /// <summary>
        /// Called when toggle AI button is clicked
        /// </summary>
        private void OnToggleAIClicked()
        {
            aiEnabled = !aiEnabled;
            
            if (aiEnabled && gameManager != null && gameManager.CurrentTurn == AIColor && !gameManager.GameEnded)
            {
                StartAIThinking();
            }
            else if (!aiEnabled && chessAI != null)
            {
                chessAI.StopThinking();
                ShowThinkingIndicator(false);
            }
            
            UpdateUI();
        }

        /// <summary>
        /// Called when color toggle button is clicked
        /// </summary>
        private void OnColorToggleClicked()
        {
            playerColor = playerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            
            if (chessAI != null)
            {
                chessAI.AIColor = AIColor;
            }
            
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI elements
        /// </summary>
        private void UpdateUI()
        {
            if (toggleAIText != null)
            {
                toggleAIText.text = aiEnabled ? "AI: ON" : "AI: OFF";
            }
            
            if (colorToggleText != null)
            {
                colorToggleText.text = $"You Play: {(playerColor == PieceColor.White ? "White" : "Black")}";
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Toggles AI on/off
        /// </summary>
        public void ToggleAI()
        {
            OnToggleAIClicked();
        }

        /// <summary>
        /// Sets the player color
        /// </summary>
        /// <param name="color">The color for the player</param>
        public void SetPlayerColor(PieceColor color)
        {
            playerColor = color;
            if (chessAI != null)
            {
                chessAI.AIColor = AIColor;
            }
            UpdateUI();
        }

        /// <summary>
        /// Restarts the game with current settings
        /// </summary>
        public void RestartGame()
        {
            // Stop AI thinking
            if (chessAI != null)
            {
                chessAI.StopThinking();
            }
            ShowThinkingIndicator(false);
            
            // Restart game using the base GameManager
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
            
            // Start AI if it's AI's turn
            if (aiEnabled && gameManager != null && gameManager.CurrentTurn == AIColor)
            {
                StartAIThinking();
            }
        }

        /// <summary>
        /// Gets the current AI difficulty
        /// </summary>
        /// <returns>The current search depth</returns>
        public int GetAIDifficulty()
        {
            if (chessAI != null)
            {
                var field = chessAI.GetType().GetField("searchDepth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                return field != null ? (int)field.GetValue(chessAI) : 3;
            }
            return 3;
        }

        /// <summary>
        /// Sets the AI difficulty
        /// </summary>
        /// <param name="depth">The search depth</param>
        public void SetAIDifficulty(int depth)
        {
            if (chessAI != null)
            {
                var field = chessAI.GetType().GetField("searchDepth", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                field?.SetValue(chessAI, Mathf.Clamp(depth, 1, 5));
            }
        }

        private void EndGameOnTimeout(PieceColor winner)
        {
            gameEnded = true;
            whiteTimerActive = false;
            blackTimerActive = false;
            Debug.Log($"Time out! {winner} wins.");
            // Show win/lose message
            if (uiManager != null)
            {
                if ((winner == playerColor && aiEnabled) || (!aiEnabled && winner == PieceColor.White))
                {
                    uiManager.ShowWinMessage();
                }
                else
                {
                    uiManager.ShowLoseMessage();
                }
            }
            // Optionally, call gameManager.OnGameEnd.Invoke();
        }

        private void UpdateTimerUI()
        {
            if (whiteTimerText != null)
                whiteTimerText.text = FormatTime(whiteTimeLeft);
            if (blackTimerText != null)
                blackTimerText.text = FormatTime(blackTimeLeft);
        }

        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        // Public methods to set timer TMP Texts
        public void SetWhiteTimerText(TextMeshProUGUI text)
        {
            whiteTimerText = text;
            UpdateTimerUI();
        }
        public void SetBlackTimerText(TextMeshProUGUI text)
        {
            blackTimerText = text;
            UpdateTimerUI();
        }
        #endregion
    }
} 