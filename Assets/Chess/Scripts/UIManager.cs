using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

namespace CanvasChess
{
    /// <summary>
    /// Manages the user interface elements and displays game information
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI turnDisplay = null!;
        [SerializeField] private TextMeshProUGUI statusDisplay = null!;
        [SerializeField] private Button restartButton = null!;
        [SerializeField] private Button backToMenuButton = null!;
        
        [Header("Game Manager Reference")]
        [SerializeField] private GameManager gameManager = null!;

        [Header("Game End UI")]
        [SerializeField] private GameObject winMessage;
        [SerializeField] private GameObject loseMessage;
        [SerializeField] private GameObject drawMessage;
        
        [Header("Draw Options")]
        [SerializeField] private UnityEngine.UI.Button drawOfferButton;

        [Header("Captured Pieces")]
        [SerializeField] private Transform whiteCapturedPanel = null!;
        [SerializeField] private Transform blackCapturedPanel = null!;
        [SerializeField] private GameObject capturedPieceIconPrefab = null!;

        [SerializeField] private TextMeshProUGUI whiteMoveHistoryDisplay = null!;
        [SerializeField] private TextMeshProUGUI blackMoveHistoryDisplay = null!;

        [Header("Player Controllers")]
        [SerializeField] public GameObject PlayerEventSystem;

        [Header("Timer UI")]
        [SerializeField] private TextMeshProUGUI whiteTimerText = null!;
        [SerializeField] private TextMeshProUGUI blackTimerText = null!;

        [Header("Disconnect UI")]
        [SerializeField] private GameObject disconnectPanel = null!;
        [SerializeField] private TextMeshProUGUI disconnectTimerText = null!;
        private Coroutine disconnectCoroutine;
        private string disconnectedPlayerName;

        [Header("Promotion UI")]
        [SerializeField] private PromotionUI promotionUI = null!;

        private Tile lastCheckedKingTile = null;
        private PieceColor? lastCheckedColor = null;
        private bool isKingInCheck = false;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
            
            if (restartButton != null)
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            
            if (backToMenuButton != null)
                backToMenuButton.onClick.AddListener(OnBackToMenuButtonClicked);
            
            // Subscribe to game events
            if (gameManager != null)
            {
                gameManager.OnTurnChanged.AddListener(OnTurnChanged);
                gameManager.OnCheck.AddListener(OnCheck);
                gameManager.OnCheckmate.AddListener(HandleCheckmate);
                gameManager.OnDraw.AddListener(OnDraw);
                gameManager.OnGameEnd.AddListener(OnGameEnd);
            }
            
            // Hide all end panels at start
            HideEndMessages();
            // Hide disconnect panel at start
            if (disconnectPanel != null) disconnectPanel.SetActive(false);
            // Setup timer UI
            SetupTimerUI();
            
            // Initialize UI
            UpdateTurnDisplay(PieceColor.White);
            UpdateStatusDisplay("");
        }

        private void OnDestroy()
        {
            // Unsubscribe from events to prevent memory leaks
            if (gameManager != null)
            {
                gameManager.OnTurnChanged.RemoveListener(OnTurnChanged);
                gameManager.OnCheck.RemoveListener(OnCheck);
                gameManager.OnCheckmate.RemoveListener(HandleCheckmate);
                gameManager.OnDraw.RemoveListener(OnDraw);
                gameManager.OnGameEnd.RemoveListener(OnGameEnd);
            }
            
            if (restartButton != null)
                restartButton.onClick.RemoveListener(OnRestartButtonClicked);
                
            if (backToMenuButton != null)
                backToMenuButton.onClick.RemoveListener(OnBackToMenuButtonClicked);
        }
        #endregion

        #region UI Updates
        /// <summary>
        /// Updates the turn display
        /// </summary>
        /// <param name="currentTurn">The current player's turn</param>
        public void UpdateTurnDisplay(PieceColor currentTurn)
        {
            if (turnDisplay != null)
            {
                string turnText = currentTurn == PieceColor.White ? "White's Turn" : "Black's Turn";
                turnDisplay.text = turnText;
                turnDisplay.color = currentTurn == PieceColor.White ? Color.white : Color.black;
            }
        }

        /// <summary>
        /// Updates the status display
        /// </summary>
        /// <param name="status">The status message to display</param>
        public void UpdateStatusDisplay(string status)
        {
            if (statusDisplay != null)
            {
                statusDisplay.text = status;
            }
        }

        /// <summary>
        /// Shows a temporary status message
        /// </summary>
        /// <param name="message">The message to show</param>
        /// <param name="duration">How long to show the message</param>
        public void ShowTemporaryMessage(string message, float duration = 3f)
        {
            UpdateStatusDisplay(message);
            
            // Clear the message after the specified duration
            Invoke(nameof(ClearTemporaryMessage), duration);
        }

        /// <summary>
        /// Clears the temporary status message
        /// </summary>
        private void ClearTemporaryMessage()
        {
            UpdateStatusDisplay("");
        }

        /// <summary>
        /// Updates the turn indicator GameObject based on whose turn it is
        /// </summary>
        public void UpdateMyTurnIndicator()
        {
            if (PlayerEventSystem == null || gameManager == null)
                return;
            bool shouldBeActive = false;
            if (gameManager.IsMasterClient && gameManager.CurrentTurn == PieceColor.White)
                shouldBeActive = true;
            else if (!gameManager.IsMasterClient && gameManager.CurrentTurn == PieceColor.Black)
                shouldBeActive = true;
            PlayerEventSystem.SetActive(shouldBeActive);
            
            // Debug log to track event system state
            Debug.Log($"Event System Update - IsMasterClient: {gameManager.IsMasterClient}, CurrentTurn: {gameManager.CurrentTurn}, ShouldBeActive: {shouldBeActive}");
        }
        #endregion

        #region Promotion UI
        /// <summary>
        /// Shows the promotion selection UI for a pawn
        /// </summary>
        /// <param name="pawn">The pawn to be promoted</param>
        public void ShowPromotionUI(Piece pawn)
        {
            if (promotionUI == null) return;
            
            // Check if this is the correct player's turn in multiplayer
            if (PhotonNetwork.InRoom)
            {
                bool isMyTurn = (pawn.Color == PieceColor.White && PhotonNetwork.IsMasterClient) ||
                               (pawn.Color == PieceColor.Black && !PhotonNetwork.IsMasterClient);
                
                if (!isMyTurn)
                {
                    Debug.Log("Not my turn to promote - hiding promotion UI");
                    return; // Don't show promotion UI to other players
                }
            }
            
            // Show the promotion UI
            promotionUI.ShowPromotionUI(pawn.Color);
            
            // Disable other UI interactions
            if (restartButton != null) restartButton.interactable = false;
            if (backToMenuButton != null) backToMenuButton.interactable = false;
            
            Debug.Log($"Showing promotion UI for {pawn.Color} pawn");
        }

        /// <summary>
        /// Hides the promotion selection UI
        /// </summary>
        public void HidePromotionUI()
        {
            if (promotionUI != null)
                promotionUI.HidePromotionUI();
            
            // Re-enable other UI interactions
            if (restartButton != null) restartButton.interactable = true;
            if (backToMenuButton != null) backToMenuButton.interactable = true;
        }
        #endregion

        #region Event Handlers
        /// <summary>
        /// Handles turn change events
        /// </summary>
        /// <param name="newTurn">The new player's turn</param>
        private void OnTurnChanged(PieceColor newTurn)
        {
            // Only clear the check highlight if the king that was in check is no longer in check
            if (lastCheckedKingTile != null && lastCheckedColor != null)
            {
                var kingColor = lastCheckedColor.Value;
                if (!MoveValidator.IsKingInCheck(kingColor, gameManager.BoardManager.Board))
                {
                    lastCheckedKingTile.ClearHighlight();
                    lastCheckedKingTile.checkHighlightObject.SetActive(false);
                    Debug.Log($"{kingColor} king is no longer in check!");
                    lastCheckedKingTile = null;
                    lastCheckedColor = null;
                    isKingInCheck = false;
                }
            }
            UpdateTurnDisplay(newTurn);
            UpdateStatusDisplay("");
            UpdateMyTurnIndicator();
        }

        /// <summary>
        /// Handles check events
        /// </summary>
        /// <param name="checkedColor">The color of the player in check</param>
        private void OnCheck(PieceColor checkedColor)
        {
            // Check if the king is still in check
            if (MoveValidator.IsKingInCheck(checkedColor, gameManager.BoardManager.Board))
            {
                // Highlight the checked king's tile using red color
                Tile kingTile = MoveValidator.FindKing(checkedColor, gameManager.BoardManager.Board);
                if (kingTile != null)
                {
                    lastCheckedKingTile = kingTile;
                    kingTile.checkHighlightObject.SetActive(true);
                }
                lastCheckedColor = checkedColor;
                isKingInCheck = true;
                Debug.Log($"{checkedColor} king is in check!");
                string status = checkedColor == PieceColor.White ? "White is in Check!" : "Black is in Check!";
                UpdateStatusDisplay(status);
            }
        }

        /// <summary>
        /// Handles checkmate events
        /// </summary>
        /// <param name="winnerColor">The color of the winning player</param>
        private void HandleCheckmate(PieceColor winnerColor)
        {
            // Highlight the checkmated king's tile using red color
            PieceColor loserColor = winnerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            Tile kingTile = MoveValidator.FindKing(loserColor, gameManager.BoardManager.Board);
            if (kingTile != null)
            {
                kingTile.checkmateHighlightObject.SetActive(true);
            }
            else
            {
                Debug.Log($"No king found for {loserColor}");
            }
            Debug.Log($"{loserColor} king is in checkmate!");
            string status = winnerColor == PieceColor.White ? "White wins by Checkmate!" : "Black wins by Checkmate!";
            UpdateStatusDisplay(status);
            // Show win/lose panels immediately
            if (gameManager != null)
            {
                bool isPlayerWinner = false;
                // Multiplayer logic
                if (Photon.Pun.PhotonNetwork.InRoom)
                {
                    if ((winnerColor == PieceColor.White && gameManager.IsMasterClient) ||
                        (winnerColor == PieceColor.Black && !gameManager.IsMasterClient))
                    {
                        isPlayerWinner = true;
                    }
                }
                else
                {
                    // Single player: assume local player is always white
                    isPlayerWinner = (winnerColor == PieceColor.White);
                }
                if (isPlayerWinner)
                    ShowWinMessage();
                else
                    ShowLoseMessage();
            }
        }

        /// <summary>
        /// Handles draw events
        /// </summary>
        private void OnDraw()
        {
            UpdateStatusDisplay("Game ended in a Draw!");
            ShowDrawMessage();
        }

        /// <summary>
        /// Handles game end events
        /// </summary>
        private void OnGameEnd()
        {
            if (restartButton != null)
                restartButton.interactable = true;
        }
        #endregion

        #region Button Handlers
        /// <summary>
        /// Handles restart button click
        /// </summary>
        private void OnRestartButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }

        /// <summary>
        /// Handles back to menu button click
        /// </summary>
        private void OnBackToMenuButtonClicked()
        {
            Debug.Log("Back to menu button clicked");
            // Disconnect from Photon if connected
            if (Photon.Pun.PhotonNetwork.IsConnected)
            {
                // Disable automatic scene sync before disconnecting to prevent SetProperties errors
                Photon.Pun.PhotonNetwork.AutomaticallySyncScene = false;
                Photon.Pun.PhotonNetwork.Disconnect();
                StartCoroutine(LoadMenuAfterDelay(0.2f));
            }
            else
            {
                SceneManager.LoadScene("MainMenuMulti");
            }
        }

        private System.Collections.IEnumerator LoadMenuAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene("MainMenuMulti");
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Sets the restart button interactable state
        /// </summary>
        /// <param name="interactable">Whether the button should be interactable</param>
        public void SetRestartButtonInteractable(bool interactable)
        {
            if (restartButton != null)
                restartButton.interactable = interactable;
        }

        /// <summary>
        /// Shows game statistics
        /// </summary>
        /// <param name="gameManager">The game manager to get stats from</param>
        public void ShowGameStats(GameManager gameManager)
        {
            string stats = $"Turn: {gameManager.TurnNumber} | Moves: {gameManager.MoveHistory.Count}";
            ShowTemporaryMessage(stats, 5f);
        }

        // Call this when the player wins
        public void ShowWinMessage()
        {
            if (winMessage != null) winMessage.SetActive(true);
            if (loseMessage != null) loseMessage.SetActive(false);
            if (PlayerEventSystem != null) PlayerEventSystem.SetActive(true);
        }

        // Call this when the player loses
        public void ShowLoseMessage()
        {
            if (winMessage != null) winMessage.SetActive(false);
            if (loseMessage != null) loseMessage.SetActive(true);
            if (drawMessage != null) drawMessage.SetActive(false);
            if (PlayerEventSystem != null) PlayerEventSystem.SetActive(true);
        }

        // Call this when the game ends in a draw
        public void ShowDrawMessage()
        {
            if (winMessage != null) winMessage.SetActive(false);
            if (loseMessage != null) loseMessage.SetActive(false);
            if (drawMessage != null) drawMessage.SetActive(true);
            if (PlayerEventSystem != null) PlayerEventSystem.SetActive(true);
        }

        // Optionally, call this to hide all end messages
        public void HideEndMessages()
        {
            if (winMessage != null) winMessage.SetActive(false);
            if (loseMessage != null) loseMessage.SetActive(false);
            if (drawMessage != null) drawMessage.SetActive(false);
        }

        public void AddCapturedPiece(Piece piece)
        {
            Transform targetPanel = piece.Color == PieceColor.White ? whiteCapturedPanel : blackCapturedPanel;
            GameObject icon = Instantiate(capturedPieceIconPrefab, targetPanel);
            Image img = icon.GetComponent<Image>();
            Image pieceImg = piece.GetComponent<Image>();
            if (img != null && pieceImg != null)
            {
                img.sprite = pieceImg.sprite;
                img.color = pieceImg.color;
                img.preserveAspect = true;
            }
            // Flip the icon upright if the board is rotated
            bool shouldFlip = false;
            
            if (PhotonNetwork.IsConnected)
            {
                // In multiplayer, flip if not master client (black player)
                shouldFlip = !PhotonNetwork.IsMasterClient;
            }
            else
            {
                // In single player, use PlayerSettings
                shouldFlip = PlayerSettingsChess.PlayerColor == PieceColor.Black;
            }
            
            if (shouldFlip)
            {
                icon.transform.localRotation = Quaternion.Euler(0, 0, 180);
            }
            else
            {
                icon.transform.localRotation = Quaternion.identity;
            }
        }

        public void UpdateMoveHistory(List<string> moveHistory)
        {
            if (whiteMoveHistoryDisplay == null || blackMoveHistoryDisplay == null) return;

            List<string> whiteMoves = new List<string>();
            List<string> blackMoves = new List<string>();

            for (int i = 0; i < moveHistory.Count; i++)
            {
                if (i % 2 == 0)
                    whiteMoves.Add(moveHistory[i]);
                else
                    blackMoves.Add(moveHistory[i]);
            }

            whiteMoveHistoryDisplay.text = string.Join("\n", whiteMoves);
            blackMoveHistoryDisplay.text = string.Join("\n", blackMoves);
        }

        /// <summary>
        /// Sets up the timer UI and connects it to the GameManager
        /// </summary>
        private void SetupTimerUI()
        {
            if (gameManager != null && whiteTimerText != null && blackTimerText != null)
            {
                gameManager.SetTimerUI(whiteTimerText, blackTimerText);
            }
        }

        /// <summary>
        /// Public method to set timer UI references
        /// </summary>
        /// <param name="whiteText">White timer text component</param>
        /// <param name="blackText">Black timer text component</param>
        public void SetTimerUI(TextMeshProUGUI whiteText, TextMeshProUGUI blackText)
        {
            whiteTimerText = whiteText;
            blackTimerText = blackText;
            SetupTimerUI();
        }

        /// <summary>
        /// Handles draw offer button click
        /// </summary>
        public void OnDrawOfferButtonClicked()
        {
            if (gameManager != null)
            {
                gameManager.HandleDrawByAgreement();
            }
        }

        /// <summary>
        /// Updates the timer display for both players
        /// </summary>
        /// <param name="whiteTime">White player's time remaining</param>
        /// <param name="blackTime">Black player's time remaining</param>
        public void UpdateTimerDisplay(float whiteTime, float blackTime)
        {
            if (whiteTimerText != null)
            {
                int minutes = Mathf.FloorToInt(whiteTime / 60);
                int seconds = Mathf.FloorToInt(whiteTime % 60);
                whiteTimerText.text = $"{minutes:00}:{seconds:00}";
            }
            
            if (blackTimerText != null)
            {
                int minutes = Mathf.FloorToInt(blackTime / 60);
                int seconds = Mathf.FloorToInt(blackTime % 60);
                blackTimerText.text = $"{minutes:00}:{seconds:00}";
            }
        }

        // Call this when a player disconnects
        public void HandlePlayerDisconnected(string playerName)
        {
            // Check if the game has already ended (e.g., by checkmate)
            // If so, don't show disconnection countdown
            if (gameManager != null && gameManager.GameEnded)
            {
                Debug.Log($"Player {playerName} disconnected after game ended, not showing disconnection UI");
                // Just terminate the room without showing disconnection countdown
                if (Photon.Pun.PhotonNetwork.InRoom)
                {
                    Photon.Pun.PhotonNetwork.LeaveRoom();
                }
                return;
            }

            disconnectedPlayerName = playerName;
            if (disconnectPanel != null) disconnectPanel.SetActive(true);
            if (disconnectCoroutine != null) StopCoroutine(disconnectCoroutine);
            disconnectCoroutine = StartCoroutine(DisconnectCountdownCoroutine());
        }

        private IEnumerator DisconnectCountdownCoroutine()
        {
            int secondsLeft = 5;
            while (secondsLeft > 0)
            {
                if (disconnectTimerText != null)
                    disconnectTimerText.text = $"{disconnectedPlayerName} disconnected. Forfeiting in {secondsLeft}...";
                yield return new WaitForSeconds(1f);
                secondsLeft--;
            }
            if (disconnectTimerText != null)
                disconnectTimerText.text = $"{disconnectedPlayerName} forfeited.";
            yield return new WaitForSeconds(1f);
            EndGameOnDisconnect();
        }

        private void EndGameOnDisconnect()
        {
            // Check if the game has already ended (e.g., by checkmate)
            // If so, don't show disconnection countdown
            if (gameManager != null && gameManager.GameEnded)
            {
                Debug.Log("Game already ended, not overriding win/lose state due to disconnection");
                // Hide disconnect panel
                if (disconnectPanel != null) disconnectPanel.SetActive(false);
                // Terminate the room
                if (Photon.Pun.PhotonNetwork.InRoom)
                {
                    // Disable automatic scene sync before leaving room to prevent SetProperties errors
                    Photon.Pun.PhotonNetwork.AutomaticallySyncScene = false;
                    Photon.Pun.PhotonNetwork.LeaveRoom();
                    StartCoroutine(LoadMenuAfterDelay(0.2f));
                }
                else
                {
                    SceneManager.LoadScene("MainMenuMulti");
                }
                return;
            }

            // Determine winner/loser only if game hasn't already ended
            bool isLocalPlayerWinner = false;
            if (Photon.Pun.PhotonNetwork.InRoom)
            {
                // If local player is still in room, they win
                isLocalPlayerWinner = true;
            }
            if (isLocalPlayerWinner)
                ShowWinMessage();
            else
                ShowLoseMessage();
            // Hide disconnect panel
            if (disconnectPanel != null) disconnectPanel.SetActive(false);
            // Terminate the room
            if (Photon.Pun.PhotonNetwork.InRoom)
            {
                // Disable automatic scene sync before leaving room to prevent SetProperties errors
                Photon.Pun.PhotonNetwork.AutomaticallySyncScene = false;
                Photon.Pun.PhotonNetwork.LeaveRoom();
                StartCoroutine(LoadMenuAfterDelay(0.2f));
            }
            else
            {
                SceneManager.LoadScene("MainMenuMulti");
            }
        }
        #endregion
    }
} 