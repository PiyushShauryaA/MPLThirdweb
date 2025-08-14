using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace CanvasChess.AI
{
    /// <summary>
    /// Chess AI that can evaluate positions and make moves
    /// </summary>
    public class ChessAI : MonoBehaviour
    {
        #region Serialized Fields
        [Header("AI Settings")]
        [SerializeField] private int searchDepth = 1; // Reduced to 1 for faster response
        [SerializeField] private bool useAlphaBetaPruning = true;
        [SerializeField] private float moveDelay = 0.5f; // Reduced delay
        [SerializeField] private int maxNodesPerSecond = 5000; // Reduced node limit
        
        [Header("Piece Values")]
        [SerializeField] private int pawnValue = 100;
        [SerializeField] private int knightValue = 320;
        [SerializeField] private int bishopValue = 330;
        [SerializeField] private int rookValue = 500;
        [SerializeField] private int queenValue = 900;
        [SerializeField] private int kingValue = 20000;
        #endregion

        #region Properties
        /// <summary>
        /// Whether the AI is currently thinking
        /// </summary>
        public bool IsThinking { get; private set; }

        /// <summary>
        /// The AI's color
        /// </summary>
        public PieceColor AIColor { get; set; } = PieceColor.Black;
        #endregion

        #region Events
        public System.Action<Move> OnAIMoveReady = null!;
        #endregion

        #region Private Fields
        private GameManager gameManager = null!;
        private BoardManager boardManager = null!;
        private System.Collections.IEnumerator? thinkingCoroutine;
        
        // Performance tracking
        private int nodesEvaluated = 0;
        private float searchStartTime = 0f;
        
        // Simple transposition table to avoid recalculating positions
        private Dictionary<int, int> transpositionTable = new Dictionary<int, int>();
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            boardManager = FindObjectOfType<BoardManager>();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Starts the AI thinking process
        /// </summary>
        public void StartThinking()
        {
            if (IsThinking) return;
            
            IsThinking = true;
            thinkingCoroutine = ThinkAndMakeMove();
            StartCoroutine(thinkingCoroutine);
        }

        /// <summary>
        /// Stops the AI thinking process
        /// </summary>
        public void StopThinking()
        {
            if (thinkingCoroutine != null)
            {
                StopCoroutine(thinkingCoroutine);
                thinkingCoroutine = null;
            }
            IsThinking = false;
        }

        /// <summary>
        /// Checks if it's the AI's turn
        /// </summary>
        /// <returns>True if it's the AI's turn</returns>
        public bool IsAITurn()
        {
            return gameManager.CurrentTurn == AIColor;
        }
        #endregion

        #region AI Thinking
        /// <summary>
        /// Coroutine that handles AI thinking and move execution
        /// </summary>
        private System.Collections.IEnumerator ThinkAndMakeMove()
        {
            // Wait a random time between 1 and 2 seconds before making the move
            float waitTime = Random.Range(1f, 2f);
            yield return new WaitForSeconds(waitTime);

            // Find the best move asynchronously
            yield return StartCoroutine(FindBestMoveAsync());

            IsThinking = false;
        }

        /// <summary>
        /// Asynchronously finds the best move using coroutines
        /// </summary>
        private System.Collections.IEnumerator FindBestMoveAsync()
        {
            Tile[,] board = boardManager.Board;
            List<Move> possibleMoves = GetAllPossibleMoves(AIColor, board);
            
            if (possibleMoves.Count == 0)
            {
                ExecuteAIMove(Move.Invalid);
                yield break;
            }

            // Reset performance tracking
            nodesEvaluated = 0;
            searchStartTime = Time.time;
            transpositionTable.Clear();

            Move bestMove = possibleMoves[0];
            int bestScore = int.MinValue;

            // Process moves in batches to avoid blocking the main thread
            int movesPerFrame = 5; // Process 5 moves per frame for faster completion
            int currentMoveIndex = 0;

            while (currentMoveIndex < possibleMoves.Count)
            {
                // Check if we should stop searching
                if (ShouldStopSearch())
                {
                    Debug.Log("AI search timeout - using best move found so far");
                    break;
                }

                // Process a batch of moves
                int endIndex = Mathf.Min(currentMoveIndex + movesPerFrame, possibleMoves.Count);
                
                for (int i = currentMoveIndex; i < endIndex; i++)
                {
                    Move move = possibleMoves[i];
                    
                    // Make the move
                    Tile originalTile = board[move.FromX, move.FromY];
                    Tile targetTile = board[move.ToX, move.ToY];
                    Piece? capturedPiece = targetTile.GetPiece();
                    
                    originalTile.RemovePiece();
                    targetTile.SetPiece(move.Piece);

                    // Evaluate the position
                    int score = useAlphaBetaPruning 
                        ? MinimaxWithAlphaBeta(board, searchDepth - 1, int.MinValue, int.MaxValue, false)
                        : Minimax(board, searchDepth - 1, false);

                    // Undo the move
                    targetTile.RemovePiece();
                    originalTile.SetPiece(move.Piece);
                    if (capturedPiece != null)
                    {
                        targetTile.SetPiece(capturedPiece);
                    }

                    // Update best move
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMove = move;
                    }
                }

                currentMoveIndex = endIndex;
                
                // Show progress
                float progress = (float)currentMoveIndex / possibleMoves.Count;
                Debug.Log($"AI thinking... {progress:P0} complete ({currentMoveIndex}/{possibleMoves.Count} moves)");
                
                // Yield to allow other processing
                yield return null;
            }

            Debug.Log($"AI evaluated {nodesEvaluated} nodes in {Time.time - searchStartTime:F2} seconds");
            
            // Execute the best move
            ExecuteAIMove(bestMove);
        }

        /// <summary>
        /// Finds the best move using minimax algorithm
        /// </summary>
        /// <returns>The best move found</returns>
        private Move FindBestMove()
        {
            Tile[,] board = boardManager.Board;
            List<Move> possibleMoves = GetAllPossibleMoves(AIColor, board);
            
            if (possibleMoves.Count == 0)
                return Move.Invalid;

            // Reset performance tracking
            nodesEvaluated = 0;
            searchStartTime = Time.time;
            transpositionTable.Clear(); // Clear transposition table for new search

            Move bestMove = possibleMoves[0];
            int bestScore = int.MinValue;

            foreach (Move move in possibleMoves)
            {
                // Check if we've exceeded time or node limits
                if (ShouldStopSearch())
                    break;

                // Make the move
                Tile originalTile = board[move.FromX, move.FromY];
                Tile targetTile = board[move.ToX, move.ToY];
                Piece? capturedPiece = targetTile.GetPiece();
                
                originalTile.RemovePiece();
                targetTile.SetPiece(move.Piece);

                // Evaluate the position
                int score = useAlphaBetaPruning 
                    ? MinimaxWithAlphaBeta(board, searchDepth - 1, int.MinValue, int.MaxValue, false)
                    : Minimax(board, searchDepth - 1, false);

                // Undo the move
                targetTile.RemovePiece();
                originalTile.SetPiece(move.Piece);
                if (capturedPiece != null)
                {
                    targetTile.SetPiece(capturedPiece);
                }

                // Update best move
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }

            Debug.Log($"AI evaluated {nodesEvaluated} nodes in {Time.time - searchStartTime:F2} seconds");
            return bestMove;
        }

        /// <summary>
        /// Checks if the search should stop due to time or node limits
        /// </summary>
        /// <returns>True if search should stop</returns>
        private bool ShouldStopSearch()
        {
            float elapsedTime = Time.time - searchStartTime;
            
            // Stop if we've been searching for too long (reduced from 5s to 2s)
            if (elapsedTime > 2f)
                return true;
                
            // Stop if we're evaluating too many nodes per second
            if (elapsedTime > 0.1f)
            {
                float nodesPerSecond = nodesEvaluated / elapsedTime;
                if (nodesPerSecond > maxNodesPerSecond)
                    return true;
            }
                
            return false;
        }

        /// <summary>
        /// Gets all possible moves for a given color
        /// </summary>
        /// <param name="color">The color to get moves for</param>
        /// <param name="board">The board state</param>
        /// <returns>List of possible moves</returns>
        private List<Move> GetAllPossibleMoves(PieceColor color, Tile[,] board)
        {
            List<Move> moves = new List<Move>();

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece? piece = board[x, y].GetPiece();
                    if (piece != null && piece.Color == color)
                    {
                        // Use forCheck=true to avoid expensive castling calculations during AI search
                        List<Tile> legalMoves = piece.GetLegalMoves(board, true);
                        foreach (Tile targetTile in legalMoves)
                        {
                            moves.Add(new Move(piece, x, y, targetTile.X, targetTile.Y));
                        }
                    }
                }
            }

            // Sort moves for better alpha-beta pruning (captures first, then other moves)
            moves.Sort((a, b) => {
                bool aIsCapture = board[a.ToX, a.ToY].GetPiece() != null;
                bool bIsCapture = board[b.ToX, b.ToY].GetPiece() != null;
                
                if (aIsCapture && !bIsCapture) return -1;
                if (!aIsCapture && bIsCapture) return 1;
                return 0;
            });

            return moves;
        }

        /// <summary>
        /// Minimax algorithm with alpha-beta pruning
        /// </summary>
        /// <param name="board">The board state</param>
        /// <param name="depth">Search depth</param>
        /// <param name="alpha">Alpha value</param>
        /// <param name="beta">Beta value</param>
        /// <param name="isMaximizing">Whether this is a maximizing player</param>
        /// <returns>The best score</returns>
        private int MinimaxWithAlphaBeta(Tile[,] board, int depth, int alpha, int beta, bool isMaximizing)
        {
            if (depth == 0)
            {
                nodesEvaluated++;
                return EvaluatePosition(board);
            }

            // Check if we should stop searching
            if (ShouldStopSearch())
                return EvaluatePosition(board);

            PieceColor currentColor = isMaximizing ? AIColor : (AIColor == PieceColor.White ? PieceColor.Black : PieceColor.White);
            List<Move> moves = GetAllPossibleMoves(currentColor, board);

            if (isMaximizing)
            {
                int maxScore = int.MinValue;
                foreach (Move move in moves)
                {
                    int score = SimulateMove(board, move, depth, alpha, beta, false);
                    maxScore = Mathf.Max(maxScore, score);
                    alpha = Mathf.Max(alpha, score);
                    if (beta <= alpha)
                        break; // Beta cutoff
                }
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                foreach (Move move in moves)
                {
                    int score = SimulateMove(board, move, depth, alpha, beta, true);
                    minScore = Mathf.Min(minScore, score);
                    beta = Mathf.Min(beta, score);
                    if (beta <= alpha)
                        break; // Alpha cutoff
                }
                return minScore;
            }
        }

        /// <summary>
        /// Standard minimax algorithm
        /// </summary>
        /// <param name="board">The board state</param>
        /// <param name="depth">Search depth</param>
        /// <param name="isMaximizing">Whether this is a maximizing player</param>
        /// <returns>The best score</returns>
        private int Minimax(Tile[,] board, int depth, bool isMaximizing)
        {
            if (depth == 0)
            {
                nodesEvaluated++;
                return EvaluatePosition(board);
            }

            // Check if we should stop searching
            if (ShouldStopSearch())
                return EvaluatePosition(board);

            PieceColor currentColor = isMaximizing ? AIColor : (AIColor == PieceColor.White ? PieceColor.Black : PieceColor.White);
            List<Move> moves = GetAllPossibleMoves(currentColor, board);

            if (isMaximizing)
            {
                int maxScore = int.MinValue;
                foreach (Move move in moves)
                {
                    int score = SimulateMove(board, move, depth, int.MinValue, int.MaxValue, false);
                    maxScore = Mathf.Max(maxScore, score);
                }
                return maxScore;
            }
            else
            {
                int minScore = int.MaxValue;
                foreach (Move move in moves)
                {
                    int score = SimulateMove(board, move, depth, int.MinValue, int.MaxValue, true);
                    minScore = Mathf.Min(minScore, score);
                }
                return minScore;
            }
        }

        /// <summary>
        /// Simulates a move and evaluates the resulting position
        /// </summary>
        /// <param name="board">The board state</param>
        /// <param name="move">The move to simulate</param>
        /// <param name="depth">Remaining depth</param>
        /// <param name="alpha">Alpha value</param>
        /// <param name="beta">Beta value</param>
        /// <param name="isMaximizing">Whether this is a maximizing player</param>
        /// <returns>The evaluation score</returns>
        private int SimulateMove(Tile[,] board, Move move, int depth, int alpha, int beta, bool isMaximizing)
        {
            // Make the move
            Tile originalTile = board[move.FromX, move.FromY];
            Tile targetTile = board[move.ToX, move.ToY];
            Piece? capturedPiece = targetTile.GetPiece();
            
            originalTile.RemovePiece();
            targetTile.SetPiece(move.Piece);

            // Evaluate
            int score = useAlphaBetaPruning 
                ? MinimaxWithAlphaBeta(board, depth - 1, alpha, beta, isMaximizing)
                : Minimax(board, depth - 1, isMaximizing);

            // Undo the move
            targetTile.RemovePiece();
            originalTile.SetPiece(move.Piece);
            if (capturedPiece != null)
            {
                targetTile.SetPiece(capturedPiece);
            }

            return score;
        }
        #endregion

        #region Position Evaluation
        /// <summary>
        /// Evaluates the current board position
        /// </summary>
        /// <param name="board">The board to evaluate</param>
        /// <returns>The evaluation score (positive favors AI, negative favors opponent)</returns>
        private int EvaluatePosition(Tile[,] board)
        {
            int boardHash = GetBoardHash(board);
            
            // Check transposition table first
            if (transpositionTable.ContainsKey(boardHash))
            {
                return transpositionTable[boardHash];
            }

            int score = 0;

            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece? piece = board[x, y].GetPiece();
                    if (piece != null)
                    {
                        int pieceValue = GetPieceValue(piece.Type);
                        int positionBonus = GetPositionBonus(piece, x, y);
                        
                        if (piece.Color == AIColor)
                        {
                            score += pieceValue + positionBonus;
                        }
                        else
                        {
                            score -= pieceValue + positionBonus;
                        }
                    }
                }
            }

            // Store in transposition table
            transpositionTable[boardHash] = score;

            return score;
        }

        /// <summary>
        /// Gets a simple hash of the board position
        /// </summary>
        /// <param name="board">The board to hash</param>
        /// <returns>A hash value representing the board position</returns>
        private int GetBoardHash(Tile[,] board)
        {
            int hash = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece? piece = board[x, y].GetPiece();
                    if (piece != null)
                    {
                        int pieceValue = (int)piece.Type * 2 + (piece.Color == PieceColor.White ? 0 : 1);
                        hash = hash * 31 + pieceValue + x * 8 + y;
                    }
                }
            }
            return hash;
        }

        /// <summary>
        /// Gets the base value of a piece type
        /// </summary>
        /// <param name="pieceType">The piece type</param>
        /// <returns>The piece value</returns>
        private int GetPieceValue(PieceType pieceType)
        {
            return pieceType switch
            {
                PieceType.Pawn => pawnValue,
                PieceType.Knight => knightValue,
                PieceType.Bishop => bishopValue,
                PieceType.Rook => rookValue,
                PieceType.Queen => queenValue,
                PieceType.King => kingValue,
                _ => 0
            };
        }

        /// <summary>
        /// Gets position bonus for a piece
        /// </summary>
        /// <param name="piece">The piece</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>Position bonus</returns>
        private int GetPositionBonus(Piece piece, int x, int y)
        {
            // Simple position bonuses - can be expanded with piece-square tables
            switch (piece.Type)
            {
                case PieceType.Pawn:
                    // Encourage pawn advancement
                    if (piece.Color == PieceColor.White)
                        return y * 10; // White pawns moving up
                    else
                        return (7 - y) * 10; // Black pawns moving down
                
                case PieceType.Knight:
                case PieceType.Bishop:
                    // Encourage central control
                    float centerDistance = Mathf.Abs(3.5f - x) + Mathf.Abs(3.5f - y);
                    return (int)((7 - centerDistance) * 5);
                
                case PieceType.Rook:
                    // Encourage rooks on open files
                    return 0; // Could be expanded
                
                case PieceType.Queen:
                    // Keep queen safe early game
                    return 0; // Could be expanded
                
                case PieceType.King:
                    // Keep king safe
                    return 0; // Could be expanded
                
                default:
                    return 0;
            }
        }
        #endregion

        #region Move Execution
        /// <summary>
        /// Executes the AI's move
        /// </summary>
        /// <param name="move">The move to execute</param>
        private void ExecuteAIMove(Move move)
        {
            Tile fromTile = boardManager.GetTile(move.FromX, move.FromY);
            Tile toTile = boardManager.GetTile(move.ToX, move.ToY);
            
            if (fromTile != null && toTile != null)
            {
                // Use the GameManager to make the move
                gameManager.OnTileClicked(fromTile, true);
                gameManager.OnTileClicked(toTile, true);
                
                // Notify that AI move is complete
                OnAIMoveReady?.Invoke(move);
            }
        }
        #endregion
    }

    /// <summary>
    /// Represents a chess move
    /// </summary>
    public struct Move
    {
        public Piece Piece;
        public int FromX, FromY;
        public int ToX, ToY;
        public bool IsValid;

        public Move(Piece piece, int fromX, int fromY, int toX, int toY)
        {
            Piece = piece;
            FromX = fromX;
            FromY = fromY;
            ToX = toX;
            ToY = toY;
            IsValid = true;
        }

        public static Move Invalid => new Move();
    }
} 