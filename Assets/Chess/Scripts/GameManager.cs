using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using Photon.Realtime;

namespace CanvasChess
{
    /// <summary>
    /// Types of draws in chess
    /// </summary>
    public enum DrawType
    {
        Stalemate,
        FiftyMoveRule,
        ThreefoldRepetition,
        InsufficientMaterial,
        Agreement
    }

    /// <summary>
    /// Controls the overall game state, turn management, and move execution
    /// </summary>
    public class GameManager : MonoBehaviourPunCallbacks
    {
        #region Serialized Fields
        [Header("Game State")]
        [SerializeField] private PieceColor currentTurn = PieceColor.White;
        [SerializeField] private Piece? selectedPiece;
        [SerializeField] private int turnNumber = 1;
        [SerializeField] private int movesWithoutCapture = 0;
        
        [Header("Game Flags")]
        [SerializeField] private bool whiteCanCastleKingside = true;
        [SerializeField] private bool whiteCanCastleQueenside = true;
        [SerializeField] private bool blackCanCastleKingside = true;
        [SerializeField] private bool blackCanCastleQueenside = true;
        
        [Header("References")]
        [SerializeField] private BoardManager boardManager = null!;
        [SerializeField] private UIManager uiManager = null!;
        
        [Header("Events")]
        [SerializeField] private UnityEvent<PieceColor> onTurnChanged = new UnityEvent<PieceColor>();
        [SerializeField] private UnityEvent<PieceColor> onCheck = new UnityEvent<PieceColor>();
        [SerializeField] private UnityEvent<PieceColor> onCheckmate = new UnityEvent<PieceColor>();
        [SerializeField] private UnityEvent onDraw = new UnityEvent();
        [SerializeField] private UnityEvent onGameEnd = new UnityEvent();
        
        [Header("Game History")]
        [SerializeField] private Dictionary<int, int> positionHistory = new Dictionary<int, int>();
        [SerializeField] private List<string> moveHistory = new List<string>();
        
        [Header("Last Move Tracking")]
        [SerializeField] private Tile? lastMoveFromTile = null;
        [SerializeField] private Tile? lastMoveToTile = null;
        
        [Header("En Passant")]
        [SerializeField] private Tile? enPassantTarget = null; // The tile that can be captured en passant
        
        [Header("Multiplayer")]
        [SerializeField] private bool isMasterClient = false; // Whether this client is the master client (host)
        [SerializeField] private bool debugLogs = true; // Enable debug logging
        
        [Header("Timer Settings")]
        [SerializeField] private bool timersEnabled = true;
        [SerializeField] private TextMeshProUGUI whiteTimerText = null!;
        [SerializeField] private TextMeshProUGUI blackTimerText = null!;
        
        private bool gameEnded = false;
        private bool isSyncingMove = false; // Prevent infinite sync loop
        private bool isWaitingForPromotion = false; // Track if we're waiting for promotion selection
        private Piece? pendingPromotionPiece = null; // The piece waiting for promotion
        private Tile? pendingPromotionTile = null; // The tile where the piece is waiting
        private Tile? promotionOriginalTile = null;
        
        // Timer fields
        private float whiteTimeLeft;
        private float blackTimeLeft;
        private bool whiteTimerActive = false;
        private bool blackTimerActive = false;
        private bool timerGameEnded = false;
        #endregion

        #region Properties
        /// <summary>
        /// The current player's turn
        /// </summary>
        public PieceColor CurrentTurn => currentTurn;

        /// <summary>
        /// The currently selected piece
        /// </summary>
        public Piece? SelectedPiece => selectedPiece;

        /// <summary>
        /// The current turn number
        /// </summary>
        public int TurnNumber => turnNumber;

        /// <summary>
        /// Whether the game has ended
        /// </summary>
        public bool GameEnded => gameEnded;

        /// <summary>
        /// Event fired when the turn changes
        /// </summary>
        public UnityEvent<PieceColor> OnTurnChanged => onTurnChanged;

        /// <summary>
        /// Event fired when a king is put in check
        /// </summary>
        public UnityEvent<PieceColor> OnCheck => onCheck;

        /// <summary>
        /// Event fired when a player is checkmated
        /// </summary>
        public UnityEvent<PieceColor> OnCheckmate => onCheckmate;

        /// <summary>
        /// Event fired when the game ends in a draw
        /// </summary>
        public UnityEvent OnDraw => onDraw;

        /// <summary>
        /// Event fired when the game ends
        /// </summary>
        public UnityEvent OnGameEnd => onGameEnd;

        public BoardManager BoardManager => boardManager;

        /// <summary>
        /// The move history list
        /// </summary>
        public List<string> MoveHistory => moveHistory;

        /// <summary>
        /// Gets the current en passant target tile
        /// </summary>
        public Tile? EnPassantTarget => enPassantTarget;

        /// <summary>
        /// Whether this client is the master client (host)
        /// </summary>
        public bool IsMasterClient => isMasterClient;

        /// <summary>
        /// Checks if castling is legal for the given side
        /// </summary>
        /// <param name="color">The color attempting to castle</param>
        /// <param name="kingside">True for kingside castling, false for queenside</param>
        /// <returns>True if castling is legal</returns>
        public bool IsCastlingLegal(PieceColor color, bool kingside)
        {
            // Check if castling rights exist
            bool canCastle = color == PieceColor.White 
                ? (kingside ? whiteCanCastleKingside : whiteCanCastleQueenside)
                : (kingside ? blackCanCastleKingside : blackCanCastleQueenside);
            
            if (!canCastle) return false;
            
            // Find king and rook
            int kingX = 4; // e-file
            int kingY = color == PieceColor.White ? 0 : 7;
            int rookX = kingside ? 7 : 0; // h-file for kingside, a-file for queenside
            
            Tile kingTile = boardManager.GetTile(kingX, kingY);
            Tile rookTile = boardManager.GetTile(rookX, kingY);
            
            // Check if king and rook are in correct positions
            if (kingTile.GetPiece()?.Type != PieceType.King || 
                kingTile.GetPiece()?.Color != color ||
                rookTile.GetPiece()?.Type != PieceType.Rook || 
                rookTile.GetPiece()?.Color != color)
            {
                return false;
            }
            
            // Check if king is in check
            if (MoveValidator.IsKingInCheck(color, boardManager.Board))
                return false;
            
            // Check if squares between king and rook are empty
            int startX = kingside ? kingX + 1 : kingX - 1;
            int endX = kingside ? rookX - 1 : rookX + 1;
            int step = kingside ? 1 : -1;
            
            for (int x = startX; x != endX + step; x += step)
            {
                if (!boardManager.GetTile(x, kingY).IsEmpty())
                    return false;
            }
            
            // Check if king would pass through check by directly checking if any opponent piece can attack the squares
            int kingMoveX = kingside ? kingX + 1 : kingX - 1;
            Tile kingMoveTile = boardManager.GetTile(kingMoveX, kingY);
            
            // Check if any opponent piece can attack the king's path
            PieceColor opponentColor = color == PieceColor.White ? PieceColor.Black : PieceColor.White;
            
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece? piece = boardManager.Board[x, y].GetPiece();
                    if (piece != null && piece.Color == opponentColor)
                    {
                        // Check if this piece can attack the king's path
                        List<Tile> moves = piece.GetLegalMoves(boardManager.Board, true); // Use forCheck=true to avoid recursion
                        if (moves.Contains(kingMoveTile))
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            if (boardManager == null)
                boardManager = FindObjectOfType<BoardManager>();
            
            if (uiManager == null)
                uiManager = FindObjectOfType<UIManager>();
            
            // Initialize master client status
            UpdateMasterClientStatus();
            
            // Initialize multiplayer turn system if in a room
            if (PhotonNetwork.InRoom)
            {
                // In multiplayer, white always starts first
                currentTurn = PieceColor.White;
                
                // Update board rotation for multiplayer
                UpdateBoardRotation();
            }
            
            // Initialize timer system
            InitializeTimerSystem();
            
            // Ensure timers are properly set up for multiplayer
            if (PhotonNetwork.InRoom)
            {
                // In multiplayer, white always starts first, so white timer should be active
                whiteTimerActive = true;
                blackTimerActive = false;
            }
            
            UpdateUI();
            
            // Ensure event system is properly initialized for multiplayer
            if (PhotonNetwork.InRoom && uiManager != null)
            {
                uiManager.UpdateMyTurnIndicator();
            }
        }

        private void Update()
        {
            if (!timersEnabled || gameEnded || timerGameEnded) return;
            
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

        #region Photon Callbacks
        public override void OnJoinedRoom()
        {
            if (debugLogs)
                Debug.Log($"GameManager: Joined room with {PhotonNetwork.CurrentRoom.PlayerCount} players");
            
            // Initialize multiplayer turn system when joining room
            // In multiplayer, white always starts first
            currentTurn = PieceColor.White;
            
            // Update board rotation for multiplayer
            UpdateBoardRotation();
            
            // Ensure timers are properly initialized for multiplayer
            // In multiplayer, white always starts first, so white timer should be active
            whiteTimerActive = true;
            blackTimerActive = false;
            
            UpdateTimerUI();
            
            // Ensure event system is properly initialized for multiplayer
            if (uiManager != null)
            {
                uiManager.UpdateMyTurnIndicator();
            }
        }
        
        public override void OnPlayerEnteredRoom(Photon.Realtime.Player newPlayer)
        {
            if (debugLogs)
                Debug.Log($"GameManager: Player {newPlayer.NickName} entered room. Total players: {PhotonNetwork.CurrentRoom.PlayerCount}");
            
            // Re-initialize turn system when new player joins
        }
        
        public override void OnMasterClientSwitched(Photon.Realtime.Player newMasterClient)
        {
            UpdateMasterClientStatus();
            Debug.Log($"Master client switched to: {newMasterClient.NickName}");
            
            // Re-initialize turn system when master client changes
        }

        // Handle when a player leaves the room (disconnects)
        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            Debug.Log($"{otherPlayer.NickName} has left the room.");
            var uiManager = FindObjectOfType<CanvasChess.UIManager>();
            if (uiManager != null)
            {
                uiManager.HandlePlayerDisconnected(otherPlayer.NickName);
            }
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// Handles tile click events
        /// </summary>
        /// <param name="tile">The tile that was clicked</param>
        /// <param name="isAI">Whether this is an AI move</param>
        public void OnTileClicked(Tile tile, bool isAI = false)
        {
            if (gameEnded) return;
            
            // If waiting for promotion, ignore tile clicks
            if (isWaitingForPromotion) return;
            
            // Restrict input to the correct player, unless this is an AI move
            PieceColor allowedColor = currentTurn;
            if (!isAI)
            {
                // Check if this is multiplayer
                if (PhotonNetwork.InRoom)
                {
                    // In multiplayer, master client plays white, others play black
                    PieceColor playerColor = PhotonNetwork.IsMasterClient ? PieceColor.White : PieceColor.Black;
                    if (currentTurn != playerColor)
                        return; // Not player's turn
                }
                else
                {
                    // Single player mode - check for AI
                    var gameManagerAI = FindObjectOfType<GameManagerAI>();
                    if (gameManagerAI != null)
                    {
                        allowedColor = gameManagerAI.PlayerColor;
                        if (currentTurn != allowedColor)
                            return; // Not player's turn
                    }
                }
            }

            Piece? pieceOnTile = tile.GetPiece();
            
            // If no piece is selected, try to select a piece
            if (selectedPiece == null)
            {
                if (pieceOnTile != null && pieceOnTile.Color == allowedColor)
                {
                    SelectPiece(pieceOnTile);
                }
            }
            // If a piece is selected, try to move it
            else
            {
                // If clicking on the same piece, deselect it
                if (pieceOnTile == selectedPiece)
                {
                    DeselectPiece();
                }
                // If clicking on a legal move tile, make the move
                else if (tile.IsHighlighted)
                {
                    // Check if this is a castling move
                    if (selectedPiece.Type == PieceType.King && 
                        selectedPiece.CurrentTile!.X == 4 && // King on e-file
                        Mathf.Abs(tile.X - selectedPiece.CurrentTile!.X) == 2) // King moving 2 squares
                    {
                        // Determine castling direction
                        bool kingside = tile.X > selectedPiece.CurrentTile!.X;
                        if (!isSyncingMove && PhotonNetwork.InRoom)
                        {
                            photonView.RPC("SyncCastling", RpcTarget.Others, currentTurn == PieceColor.White, kingside);
                        }
                        ExecuteCastling(currentTurn, kingside);
                        DeselectPiece();
                        CheckGameEndConditions();
                        if (!gameEnded)
                        {
                            SwitchTurn();
                        }
                    }
                    else
                    {
                        MakeMove(selectedPiece, tile);
                    }
                }
                // If clicking on another piece of the same color, select that piece instead
                else if (pieceOnTile != null && pieceOnTile.Color == allowedColor)
                {
                    SelectPiece(pieceOnTile);
                }
                // Otherwise, deselect the current piece
                else
                {
                    DeselectPiece();
                }
            }
        }
        #endregion

        #region Piece Selection
        /// <summary>
        /// Selects a piece and highlights its legal moves
        /// </summary>
        /// <param name="piece">The piece to select</param>
        private void SelectPiece(Piece piece)
        {
            selectedPiece = piece;
            boardManager.HighlightLegalMoves(piece);
        }

        /// <summary>
        /// Deselects the currently selected piece
        /// </summary>
        private void DeselectPiece()
        {
            selectedPiece = null;
            boardManager.ClearAllHighlights();
        }

        /// <summary>
        /// Highlights the last move on the board
        /// </summary>
        /// <param name="fromTile">The tile the piece moved from</param>
        /// <param name="toTile">The tile the piece moved to</param>
        private void HighlightLastMove(Tile fromTile, Tile toTile)
        {
            Debug.Log($"HighlightLastMove called: fromTile={fromTile?.X},{fromTile?.Y}, toTile={toTile?.X},{toTile?.Y}");
            
            // Clear previous last move highlight
            ClearLastMoveHighlight();
            
            // Set new last move tiles
            lastMoveFromTile = fromTile;
            lastMoveToTile = toTile;
            
            // Highlight the tiles
            if (fromTile != null)
            {
                Debug.Log($"Highlighting fromTile at {fromTile.X},{fromTile.Y} with highlight=true");
                fromTile.HighlightLastMove(true);
            }
            if (toTile != null)
            {
                Debug.Log($"Highlighting toTile at {toTile.X},{toTile.Y} with highlight=true");
                toTile.HighlightLastMove(true);
            }
        }

        /// <summary>
        /// Clears the last move highlight
        /// </summary>
        private void ClearLastMoveHighlight()
        {
            Debug.Log($"ClearLastMoveHighlight called: lastMoveFromTile={lastMoveFromTile != null}, lastMoveToTile={lastMoveToTile != null}");
            if (lastMoveFromTile != null)
                lastMoveFromTile.ClearLastMoveHighlight();
            if (lastMoveToTile != null)
                lastMoveToTile.ClearLastMoveHighlight();
            
            lastMoveFromTile = null;
            lastMoveToTile = null;
        }
        #endregion

        #region Move Execution
        /// <summary>
        /// Executes a move from one tile to another
        /// </summary>
        /// <param name="piece">The piece to move</param>
        /// <param name="targetTile">The destination tile</param>
        private void MakeMove(Piece piece, Tile targetTile)
        {
            Debug.Log($"MakeMove called: piece={piece.Type} at {piece.CurrentTile?.X},{piece.CurrentTile?.Y} to {targetTile.X},{targetTile.Y}");
            
            if (!isSyncingMove && PhotonNetwork.InRoom)
            {
                // Broadcast move to others
                photonView.RPC("SyncMove", RpcTarget.Others, piece.CurrentTile.X, piece.CurrentTile.Y, targetTile.X, targetTile.Y);
            }
            Tile originalTile = piece.CurrentTile!;
            Piece? capturedPiece = targetTile.GetPiece();

            // Handle en passant capture
            if (piece.Type == PieceType.Pawn && enPassantTarget != null && targetTile == enPassantTarget)
            {
                // Find and remove the pawn that was captured en passant
                int capturedPawnY = piece.Color == PieceColor.White ? targetTile.Y - 1 : targetTile.Y + 1;
                Tile capturedPawnTile = boardManager.GetTile(targetTile.X, capturedPawnY);
                capturedPiece = capturedPawnTile.GetPiece();
                if (capturedPiece != null)
                {
                    if (uiManager != null) uiManager.AddCapturedPiece(capturedPiece);
                    boardManager.RemovePiece(capturedPiece);
                    movesWithoutCapture = 0;
                }
            }
            else if (capturedPiece != null)
            {
                // Normal capture
                if (uiManager != null) uiManager.AddCapturedPiece(capturedPiece);
                boardManager.RemovePiece(capturedPiece);
                movesWithoutCapture = 0;
            }
            else
            {
                movesWithoutCapture++;
            }
            
            // Move the piece
            originalTile.RemovePiece();
            targetTile.SetPiece(piece);
            piece.RecordMove(turnNumber);
            
            // Handle pawn promotion
            if (piece.Type == PieceType.Pawn)
            {
                int promotionRank = piece.Color == PieceColor.White ? 7 : 0;
                if (targetTile.Y == promotionRank)
                {
                    if (PhotonNetwork.InRoom)
                    {
                        bool isMyTurn = (piece.Color == PieceColor.White && PhotonNetwork.IsMasterClient) ||
                                        (piece.Color == PieceColor.Black && !PhotonNetwork.IsMasterClient);

                        if (isMyTurn)
                        {
                            pendingPromotionPiece = piece;
                            pendingPromotionTile = targetTile;
                            isWaitingForPromotion = true;
                            promotionOriginalTile = originalTile;
                            uiManager.ShowPromotionUI(piece);
                            return;
                        }
                        else
                        {
                            // Do nothing! Wait for the RPC from the promoting player.
                            return;
                        }
                    }
                    else
                    {
                        // Single player mode
                        pendingPromotionPiece = piece;
                        pendingPromotionTile = targetTile;
                        isWaitingForPromotion = true;
                        promotionOriginalTile = originalTile;
                        uiManager.ShowPromotionUI(piece);
                        return;
                    }
                }
            }
            
            // Update castling flags
            UpdateCastlingFlags(piece, originalTile);
            
            // Update en passant target
            UpdateEnPassantTarget(piece, originalTile, targetTile);
            
            // Record the move
            string moveNotation = GetMoveNotation(piece, originalTile, targetTile, capturedPiece);
            moveHistory.Add(moveNotation);
            
            // Highlight the last move
            Debug.Log($"About to call HighlightLastMove with originalTile={originalTile.X},{originalTile.Y} and targetTile={targetTile.X},{targetTile.Y}");
            HighlightLastMove(originalTile, targetTile);
            Debug.Log($"HighlightLastMove call completed");
            
            // Update position history for threefold repetition
            int boardHash = MoveValidator.CalculateBoardHash(boardManager.Board);
            if (positionHistory.ContainsKey(boardHash))
            {
                positionHistory[boardHash]++;
            }
            else
            {
                positionHistory[boardHash] = 1;
            }
            
            // Clear selection and highlights
            DeselectPiece();
            
            // Check for game end conditions
            CheckGameEndConditions();
            
            // If game hasn't ended, switch turns
            if (!gameEnded)
            {
                SwitchTurn();
            }

            if (uiManager != null)
                uiManager.UpdateMoveHistory(moveHistory);
        }

        [PunRPC]
        public void SyncMove(int fromX, int fromY, int toX, int toY)
        {
            isSyncingMove = true;
            Tile fromTile = boardManager.GetTile(fromX, fromY);
            Tile toTile = boardManager.GetTile(toX, toY);
            Piece piece = fromTile.GetPiece();
            if (piece != null)
            {
                MakeMove(piece, toTile);
            }
            isSyncingMove = false;
        }

        [PunRPC]
        public void SyncCastling(bool isWhite, bool kingside)
        {
            isSyncingMove = true;
            var color = isWhite ? PieceColor.White : PieceColor.Black;
            ExecuteCastling(color, kingside);
            DeselectPiece();
            CheckGameEndConditions();
            if (!gameEnded)
                SwitchTurn();
            isSyncingMove = false;
        }

        [PunRPC]
        public void SyncPromotion(int fromX, int fromY, int toX, int toY, int pieceType)
        {
            isSyncingMove = true;
            Debug.Log($"[SyncPromotion] Received promotion: from ({fromX},{fromY}) to ({toX},{toY}), type: {(PieceType)pieceType}");
            
            Tile fromTile = boardManager.GetTile(fromX, fromY);
            Tile toTile = boardManager.GetTile(toX, toY);
            Piece piece = fromTile.GetPiece();

            // If no piece at fromTile, check if there's already a piece at toTile (pawn might have been moved already)
            if (piece == null)
            {
                piece = toTile.GetPiece();
                if (piece != null && piece.Type == PieceType.Pawn)
                {
                    Debug.Log($"[SyncPromotion] Found pawn at destination tile ({toX},{toY}), promoting directly");
                    // The pawn is already at the destination, just promote it
                    PieceType promotionType = (PieceType)pieceType;
                    piece.PromoteToPiece(promotionType);

                    // Set up promotion state for CompletePromotionMove
                    pendingPromotionPiece = piece;
                    pendingPromotionTile = toTile;
                    isWaitingForPromotion = true;
                    promotionOriginalTile = fromTile; // Keep original tile for move notation

                    // Complete the promotion move (this will call SwitchTurn, update UI, etc.)
                    CompletePromotionMove();
                }
                else
                {
                    Debug.LogError($"[SyncPromotion] No piece found at fromTile ({fromX},{fromY}) or valid pawn at toTile ({toX},{toY})");
                }
            }
            else
            {
                // Normal case: pawn is at fromTile, move it to toTile and promote
                fromTile.RemovePiece();
                toTile.SetPiece(piece);
                piece.RecordMove(turnNumber);

                PieceType promotionType = (PieceType)pieceType;
                piece.PromoteToPiece(promotionType);

                // Set up promotion state for CompletePromotionMove
                pendingPromotionPiece = piece;
                pendingPromotionTile = toTile;
                isWaitingForPromotion = true;
                promotionOriginalTile = fromTile;

                // Complete the promotion move (this will call SwitchTurn, update UI, etc.)
                CompletePromotionMove();
            }

            isSyncingMove = false;
        }
        #endregion

        #region Special Moves
        /// <summary>
        /// Promotes a pawn to a queen
        /// </summary>
        /// <param name="pawn">The pawn to promote</param>
        private void PromotePawn(Piece pawn)
        {
            // Auto-promote to queen using the new method
            pawn.PromoteToQueen();
        }

        /// <summary>
        /// Promotes the pending piece and completes the move
        /// </summary>
        /// <param name="pieceType">The piece type to promote to</param>
        public void PromoteAndCompleteMove(PieceType pieceType)
        {
            if (!isWaitingForPromotion || pendingPromotionPiece == null || pendingPromotionTile == null)
                return;

            // Synchronize the promotion move across devices
            if (PhotonNetwork.InRoom && !isSyncingMove)
            {
                // Send the promotion move to other players
                photonView.RPC("SyncPromotion", RpcTarget.Others, 
                    promotionOriginalTile.X, promotionOriginalTile.Y,
                    pendingPromotionTile.X, pendingPromotionTile.Y, (int)pieceType);
            }

            // Promote the piece
            pendingPromotionPiece.PromoteToPiece(pieceType);

            // Hide the promotion UI
            if (uiManager != null)
                uiManager.HidePromotionUI();

            // Update castling flags
            UpdateCastlingFlags(pendingPromotionPiece, pendingPromotionTile);
            
            // Update en passant target (should be null for promotion moves)
            UpdateEnPassantTarget(pendingPromotionPiece, pendingPromotionTile, pendingPromotionTile);
            
            // Record the move
            string moveNotation = GetMoveNotation(pendingPromotionPiece, pendingPromotionTile, pendingPromotionTile, null);
            moveHistory.Add(moveNotation);
            
            // Highlight the last move
            HighlightLastMove(pendingPromotionTile, pendingPromotionTile);
            
            // Update position history for threefold repetition
            int boardHash = MoveValidator.CalculateBoardHash(boardManager.Board);
            if (positionHistory.ContainsKey(boardHash))
            {
                positionHistory[boardHash]++;
            }
            else
            {
                positionHistory[boardHash] = 1;
            }
            
            // Clear selection and highlights
            DeselectPiece();
            
            // Reset promotion state
            isWaitingForPromotion = false;
            pendingPromotionPiece = null;
            pendingPromotionTile = null;
            promotionOriginalTile = null;
            
            // Check for game end conditions
            CheckGameEndConditions();
            
            // If game hasn't ended, switch turns
            if (!gameEnded)
            {
                SwitchTurn();
            }

            if (uiManager != null)
                uiManager.UpdateMoveHistory(moveHistory);
        }

        /// <summary>
        /// Completes the promotion move after the player has selected a piece type
        /// </summary>
        public void CompletePromotionMove()
        {
            if (!isWaitingForPromotion || pendingPromotionPiece == null || pendingPromotionTile == null)
                return;

            // Hide the promotion UI
            if (uiManager != null)
                uiManager.HidePromotionUI();

            // Update castling flags
            UpdateCastlingFlags(pendingPromotionPiece, pendingPromotionTile);
            
            // Update en passant target (should be null for promotion moves)
            UpdateEnPassantTarget(pendingPromotionPiece, pendingPromotionTile, pendingPromotionTile);
            
            // Record the move
            string moveNotation = GetMoveNotation(pendingPromotionPiece, pendingPromotionTile, pendingPromotionTile, null);
            moveHistory.Add(moveNotation);
            
            // Highlight the last move
            HighlightLastMove(pendingPromotionTile, pendingPromotionTile);
            
            // Update position history for threefold repetition
            int boardHash = MoveValidator.CalculateBoardHash(boardManager.Board);
            if (positionHistory.ContainsKey(boardHash))
            {
                positionHistory[boardHash]++;
            }
            else
            {
                positionHistory[boardHash] = 1;
            }
            
            // Clear selection and highlights
            DeselectPiece();
            
            // Reset promotion state
            isWaitingForPromotion = false;
            pendingPromotionPiece = null;
            pendingPromotionTile = null;
            
            // Check for game end conditions
            CheckGameEndConditions();
            
            // If game hasn't ended, switch turns
            if (!gameEnded)
            {
                SwitchTurn();
            }

            if (uiManager != null)
                uiManager.UpdateMoveHistory(moveHistory);
        }

        /// <summary>
        /// Updates castling flags based on piece movement
        /// </summary>
        /// <param name="piece">The piece that moved</param>
        /// <param name="originalTile">The original tile</param>
        private void UpdateCastlingFlags(Piece piece, Tile originalTile)
        {
            if (piece.Type == PieceType.King)
            {
                if (piece.Color == PieceColor.White)
                {
                    whiteCanCastleKingside = false;
                    whiteCanCastleQueenside = false;
                }
                else
                {
                    blackCanCastleKingside = false;
                    blackCanCastleQueenside = false;
                }
            }
            else if (piece.Type == PieceType.Rook)
            {
                if (piece.Color == PieceColor.White)
                {
                    if (originalTile.X == 0) // Queenside rook
                        whiteCanCastleQueenside = false;
                    else if (originalTile.X == 7) // Kingside rook
                        whiteCanCastleKingside = false;
                }
                else
                {
                    if (originalTile.X == 0) // Queenside rook
                        blackCanCastleQueenside = false;
                    else if (originalTile.X == 7) // Kingside rook
                        blackCanCastleKingside = false;
                }
            }
        }

        /// <summary>
        /// Updates en passant target based on pawn movement
        /// </summary>
        /// <param name="piece">The piece that moved</param>
        /// <param name="fromTile">The starting tile</param>
        /// <param name="toTile">The destination tile</param>
        private void UpdateEnPassantTarget(Piece piece, Tile fromTile, Tile toTile)
        {
            // Clear previous en passant target
            enPassantTarget = null;
            
            // Check if a pawn made a double move
            if (piece.Type == PieceType.Pawn)
            {
                int startRank = piece.Color == PieceColor.White ? 1 : 6;
                int direction = piece.Color == PieceColor.White ? 1 : -1;
                
                // If pawn moved from starting rank and moved 2 squares
                if (fromTile.Y == startRank && Mathf.Abs(toTile.Y - fromTile.Y) == 2)
                {
                    // Set en passant target to the square the pawn passed over
                    int enPassantY = fromTile.Y + direction;
                    enPassantTarget = boardManager.GetTile(fromTile.X, enPassantY);
                }
            }
        }

        /// <summary>
        /// Executes a castling move
        /// </summary>
        /// <param name="color">The color castling</param>
        /// <param name="kingside">True for kingside castling, false for queenside</param>
        private void ExecuteCastling(PieceColor color, bool kingside)
        {
            int kingX = 4; // e-file
            int kingY = color == PieceColor.White ? 0 : 7;
            int rookX = kingside ? 7 : 0;
            
            Tile kingTile = boardManager.GetTile(kingX, kingY);
            Tile rookTile = boardManager.GetTile(rookX, kingY);
            
            Piece king = kingTile.GetPiece()!;
            Piece rook = rookTile.GetPiece()!;
            
            // Calculate new positions
            int newKingX = kingside ? kingX + 2 : kingX - 2;
            int newRookX = kingside ? kingX + 1 : kingX - 1;
            
            Tile newKingTile = boardManager.GetTile(newKingX, kingY);
            Tile newRookTile = boardManager.GetTile(newRookX, kingY);
            
            // Move pieces
            kingTile.RemovePiece();
            rookTile.RemovePiece();
            
            newKingTile.SetPiece(king);
            newRookTile.SetPiece(rook);
            
            // Record moves
            king.RecordMove(turnNumber);
            rook.RecordMove(turnNumber);
            
            // Update castling flags
            if (color == PieceColor.White)
            {
                whiteCanCastleKingside = false;
                whiteCanCastleQueenside = false;
            }
            else
            {
                blackCanCastleKingside = false;
                blackCanCastleQueenside = false;
            }
            
            // Record the move
            string moveNotation = kingside ? "O-O" : "O-O-O";
            moveHistory.Add(moveNotation);
            
            // Highlight the last move (king's move)
            HighlightLastMove(kingTile, newKingTile);
            
            // Update position history
            int boardHash = MoveValidator.CalculateBoardHash(boardManager.Board);
            if (positionHistory.ContainsKey(boardHash))
            {
                positionHistory[boardHash]++;
            }
            else
            {
                positionHistory[boardHash] = 1;
            }
        }
        #endregion

        #region Move Notation
        /// <summary>
        /// Gets algebraic notation for a move
        /// </summary>
        /// <param name="piece">The piece that moved</param>
        /// <param name="fromTile">The starting tile</param>
        /// <param name="toTile">The destination tile</param>
        /// <param name="capturedPiece">The captured piece, if any</param>
        /// <returns>Algebraic notation string</returns>
        private string GetMoveNotation(Piece piece, Tile fromTile, Tile toTile, Piece? capturedPiece)
        {
            string fromSquare = GetSquareNotation(fromTile.X, fromTile.Y);
            string toSquare = GetSquareNotation(toTile.X, toTile.Y);
            return $"{fromSquare} to {toSquare}";
        }

        /// <summary>
        /// Converts board coordinates to algebraic notation
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Algebraic notation (e.g., "e4")</returns>
        private string GetSquareNotation(int x, int y)
        {
            char file = (char)('a' + x);
            char rank = (char)('1' + y);
            return file.ToString() + rank.ToString();
        }
        #endregion

        #region Game State Management
        /// <summary>
        /// Checks for game end conditions
        /// </summary>
        private void CheckGameEndConditions()
        {
            PieceColor opponentColor = currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
            
            // Check for checkmate
            if (MoveValidator.IsCheckmate(opponentColor, boardManager.Board))
            {
                gameEnded = true;
                onCheckmate.Invoke(currentTurn);
                onGameEnd.Invoke();
                return;
            }
            
            // Check for stalemate
            if (MoveValidator.IsStalemate(opponentColor, boardManager.Board))
            {
                gameEnded = true;
                HandleDraw(DrawType.Stalemate);
                return;
            }
            
            // Check for 50-move rule
            if (movesWithoutCapture >= 100) // 50 moves = 100 half-moves
            {
                gameEnded = true;
                HandleDraw(DrawType.FiftyMoveRule);
                return;
            }
            
            // Check for threefold repetition
            int boardHash = MoveValidator.CalculateBoardHash(boardManager.Board);
            if (positionHistory.ContainsKey(boardHash) && positionHistory[boardHash] >= 3)
            {
                gameEnded = true;
                HandleDraw(DrawType.ThreefoldRepetition);
                return;
            }
            

            
            // Check for check
            if (MoveValidator.IsKingInCheck(opponentColor, boardManager.Board))
            {
                onCheck.Invoke(opponentColor);
            }
        }

        /// <summary>
        /// Switches the turn to the other player
        /// </summary>
        private void SwitchTurn()
        {
            currentTurn = currentTurn == PieceColor.White ? PieceColor.Black : PieceColor.White;
            turnNumber++;
            onTurnChanged.Invoke(currentTurn);
            
            // Switch timer when turn changes
            SwitchTimer();
            
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI to reflect the current game state
        /// </summary>
        private void UpdateUI()
        {
            if (uiManager != null)
            {
                uiManager.UpdateTurnDisplay(currentTurn);
                uiManager.UpdateStatusDisplay("");
                
                // Update multiplayer-specific UI elements
                if (PhotonNetwork.InRoom)
                {
                    uiManager.UpdateMyTurnIndicator();
                }
            }
        }
        #endregion

        #region Game Control
        /// <summary>
        /// Restarts the game
        /// </summary>
        public void RestartGame()
        {
            gameEnded = false;
            timerGameEnded = false;
            currentTurn = PieceColor.White;
            turnNumber = 1;
            movesWithoutCapture = 0;
            selectedPiece = null;
            
            // Reset castling flags
            whiteCanCastleKingside = true;
            whiteCanCastleQueenside = true;
            blackCanCastleKingside = true;
            blackCanCastleQueenside = true;
            
            // Reset en passant target
            enPassantTarget = null;
            
            // Clear history
            positionHistory.Clear();
            moveHistory.Clear();
            
            // Clear last move highlight
            ClearLastMoveHighlight();
            
            // Reset board
            boardManager.ResetBoard();
            
            // Reset timer system
            InitializeTimerSystem();
            
            // Update UI
            UpdateUI();
            
            // Fire events
            onTurnChanged.Invoke(currentTurn);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets the current game state as a string
        /// </summary>
        /// <returns>String representation of the game state</returns>
        public string GetGameState()
        {
            string state = $"Turn: {currentTurn}, Turn Number: {turnNumber}\n";
            state += $"FEN: {boardManager.GetFEN()}\n";
            state += $"Moves without capture: {movesWithoutCapture}\n";
            state += $"Game ended: {gameEnded}";
            return state;
        }
        #endregion

        #region Event Handlers
        private void HandleCheckmate(PieceColor winnerColor)
        {
            // Highlight the checkmated king's tile in red
            PieceColor loserColor = winnerColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            Tile kingTile = MoveValidator.FindKing(loserColor, boardManager.Board);
            if (kingTile != null)
            {
                var img = kingTile.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                    img.color = Color.red;
            }
            
            string status = winnerColor == PieceColor.White ? "White wins by Checkmate!" : "Black wins by Checkmate!";
            uiManager.UpdateStatusDisplay(status);
            
            // Show appropriate win/lose message based on player role
            if (uiManager != null)
            {
                if (PhotonNetwork.InRoom)
                {
                    // Multiplayer mode
                    if ((winnerColor == PieceColor.White && isMasterClient) || 
                        (winnerColor == PieceColor.Black && !isMasterClient))
                    {
                        uiManager.ShowWinMessage();
                    }
                    else
                    {
                        uiManager.ShowLoseMessage();
                    }
                }
                else
                {
                    // Single player mode
                    if (winnerColor == PieceColor.White)
                    {
                        uiManager.ShowWinMessage();
                    }
                    else
                    {
                        uiManager.ShowLoseMessage();
                    }
                }
            }
        }
        #endregion

        #region Multiplayer Utilities
        /// <summary>
        /// Updates the master client status based on current Photon network state
        /// </summary>
        private void UpdateMasterClientStatus()
        {
            isMasterClient = PhotonNetwork.IsMasterClient;
            Debug.Log($"Master client status updated: {isMasterClient}");
        }
        #endregion

        #region Timer System
        /// <summary>
        /// Initializes the timer system
        /// </summary>
        private void InitializeTimerSystem()
        {
            float timerDuration = PlayerSettingsChess.GetTimerDuration();
            whiteTimeLeft = timerDuration;
            blackTimeLeft = timerDuration;
            whiteTimerActive = false;
            blackTimerActive = false;
            timerGameEnded = false;
            
            // Start white's timer if white starts
            if (currentTurn == PieceColor.White)
            {
                whiteTimerActive = true;
            }
            
            UpdateTimerUI();
        }

        /// <summary>
        /// Updates the timer UI display
        /// </summary>
        private void UpdateTimerUI()
        {
            if (whiteTimerText != null)
                whiteTimerText.text = FormatTime(whiteTimeLeft);
            if (blackTimerText != null)
                blackTimerText.text = FormatTime(blackTimeLeft);
        }

        /// <summary>
        /// Formats time in MM:SS format
        /// </summary>
        /// <param name="time">Time in seconds</param>
        /// <returns>Formatted time string</returns>
        private string FormatTime(float time)
        {
            int minutes = Mathf.FloorToInt(time / 60f);
            int seconds = Mathf.FloorToInt(time % 60f);
            return $"{minutes:00}:{seconds:00}";
        }

        /// <summary>
        /// Ends the game when a player runs out of time
        /// </summary>
        /// <param name="winner">The player who wins (the one who didn't run out of time)</param>
        private void EndGameOnTimeout(PieceColor winner)
        {
            timerGameEnded = true;
            gameEnded = true;
            whiteTimerActive = false;
            blackTimerActive = false;
            
            Debug.Log($"Time out! {winner} wins by timeout.");
            
            // Show appropriate message based on player role
            if (uiManager != null)
            {
                if (PhotonNetwork.InRoom)
                {
                    // Multiplayer mode
                    if ((winner == PieceColor.White && isMasterClient) || 
                        (winner == PieceColor.Black && !isMasterClient))
                    {
                        uiManager.ShowWinMessage();
                    }
                    else
                    {
                        uiManager.ShowLoseMessage();
                    }
                }
                else
                {
                    // Single player mode
                    if (winner == PieceColor.White)
                    {
                        uiManager.ShowWinMessage();
                    }
                    else
                    {
                        uiManager.ShowLoseMessage();
                    }
                }
            }
            
            onGameEnd.Invoke();
        }

        /// <summary>
        /// Switches the active timer when turn changes
        /// </summary>
        private void SwitchTimer()
        {
            if (currentTurn == PieceColor.White)
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
        /// Public method to set timer UI references
        /// </summary>
        /// <param name="whiteText">White timer text component</param>
        /// <param name="blackText">Black timer text component</param>
        public void SetTimerUI(TextMeshProUGUI whiteText, TextMeshProUGUI blackText)
        {
            whiteTimerText = whiteText;
            blackTimerText = blackText;
            UpdateTimerUI();
        }

        /// <summary>
        /// Enables or disables the timer system
        /// </summary>
        /// <param name="enabled">Whether timers should be enabled</param>
        public void SetTimersEnabled(bool enabled)
        {
            timersEnabled = enabled;
            if (!enabled)
            {
                whiteTimerActive = false;
                blackTimerActive = false;
            }
        }

        /// <summary>
        /// Gets the current time remaining for a player
        /// </summary>
        /// <param name="color">The player color</param>
        /// <returns>Time remaining in seconds</returns>
        public float GetTimeRemaining(PieceColor color)
        {
            return color == PieceColor.White ? whiteTimeLeft : blackTimeLeft;
        }

        /// <summary>
        /// Handles different types of draws
        /// </summary>
        /// <param name="drawType">The type of draw that occurred</param>
        private void HandleDraw(DrawType drawType)
        {
            string drawMessage = drawType switch
            {
                DrawType.Stalemate => "Draw by Stalemate!",
                DrawType.FiftyMoveRule => "Draw by 50-Move Rule!",
                DrawType.ThreefoldRepetition => "Draw by Threefold Repetition!",
                DrawType.InsufficientMaterial => "Draw by Insufficient Material!",
                DrawType.Agreement => "Draw by Agreement!",
                _ => "Draw!"
            };

            Debug.Log(drawMessage);
            
            if (uiManager != null)
            {
                uiManager.UpdateStatusDisplay(drawMessage);
                uiManager.ShowDrawMessage();
            }
            
            onDraw.Invoke();
            onGameEnd.Invoke();
        }

        /// <summary>
        /// Public method to handle draw by agreement
        /// </summary>
        public void HandleDrawByAgreement()
        {
            if (!gameEnded)
            {
                gameEnded = true;
                HandleDraw(DrawType.Agreement);
            }
        }

        /// <summary>
        /// Updates board rotation based on player role in multiplayer
        /// </summary>
        public void UpdateBoardRotation()
        {
            if (boardManager != null)
            {
                // Force board manager to re-evaluate rotation
                boardManager.ForceBoardRotation();
            }
        }
        #endregion
    }
} 