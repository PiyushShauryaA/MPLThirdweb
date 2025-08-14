using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CanvasChess
{
    /// <summary>
    /// Represents a chess piece with movement logic and legal move calculation
    /// </summary>
    public class Piece : MonoBehaviour
    {
        #region Pawn Promotion Feature
        /*
         * Pawn Promotion Feature:
         * - When a pawn reaches the last rank (rank 8 for white, rank 1 for black), it automatically promotes to a queen
         * - The queen sprite can be assigned via the Inspector or programmatically using SetQueenSprite()
         * - If no queen sprite is assigned, the system will load the default queen sprite from Resources/Sprites/
         * - Use ShouldPromote() to check if a pawn should be promoted
         * - Use PromoteToQueen() to manually trigger promotion
         */
        #endregion

        #region Serialized Fields
        [Header("Piece Properties")]
        [SerializeField] private PieceType type;
        [SerializeField] private PieceColor color;
        [SerializeField] private Tile? currentTile;
        
        [Header("Visual Components")]
        [SerializeField] private Image pieceImage;
        [SerializeField] private Sprite queenSprite; // Queen sprite for pawn promotion
        
        [Header("Manual Sprite Assignment (Fallback)")]
        [SerializeField] private Sprite whitePawnSprite = null!;
        [SerializeField] private Sprite whiteRookSprite = null!;
        [SerializeField] private Sprite whiteKnightSprite = null!;
        [SerializeField] private Sprite whiteBishopSprite = null!;
        [SerializeField] private Sprite whiteQueenSprite = null!;
        [SerializeField] private Sprite whiteKingSprite = null!;
        [SerializeField] private Sprite blackPawnSprite = null!;
        [SerializeField] private Sprite blackRookSprite = null!;
        [SerializeField] private Sprite blackKnightSprite = null!;
        [SerializeField] private Sprite blackBishopSprite = null!;
        [SerializeField] private Sprite blackQueenSprite = null!;
        [SerializeField] private Sprite blackKingSprite = null!;
        
        [Header("Movement History")]
        [SerializeField] private bool hasMoved = false;
        [SerializeField] private int lastMoveTurn = -1;
        #endregion

        #region Properties
        /// <summary>
        /// The type of this chess piece
        /// </summary>
        public PieceType Type => type;

        /// <summary>
        /// The color of this chess piece
        /// </summary>
        public PieceColor Color => color;

        /// <summary>
        /// The tile this piece is currently on
        /// </summary>
        public Tile? CurrentTile
        {
            get => currentTile;
            set => currentTile = value;
        }

        /// <summary>
        /// Whether this piece has moved from its initial position
        /// </summary>
        public bool HasMoved
        {
            get => hasMoved;
            set => hasMoved = value;
        }

        /// <summary>
        /// The turn number when this piece last moved
        /// </summary>
        public int LastMoveTurn
        {
            get => lastMoveTurn;
            set => lastMoveTurn = value;
        }

        /// <summary>
        /// The queen sprite used for pawn promotion
        /// </summary>
        public Sprite QueenSprite
        {
            get => queenSprite;
            set => queenSprite = value;
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (pieceImage == null)
                pieceImage = GetComponent<Image>();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the piece with type, color, and sprite
        /// </summary>
        /// <param name="pieceType">The type of piece</param>
        /// <param name="pieceColor">The color of the piece</param>
        public void Initialize(PieceType pieceType, PieceColor pieceColor)
        {
            type = pieceType;
            color = pieceColor;
            hasMoved = false;
            lastMoveTurn = -1;
            
            LoadSprite();
        }
        #endregion

        #region Visual Management
        /// <summary>
        /// Loads the appropriate sprite for this piece
        /// </summary>
        private void LoadSprite()
        {
            // Use manually assigned sprites first (recommended approach)
            Sprite sprite = GetManuallyAssignedSprite();
            
            if (sprite != null)
            {
                pieceImage.sprite = sprite;
                Debug.Log($"Successfully loaded manually assigned sprite for {type} {color}");
                return;
            }
            
            // Fallback to Resources.Load if manual assignment fails
            string spriteName = GetSpriteName();
            sprite = Resources.Load<Sprite>($"Sprites/{spriteName}");
            
            if (sprite != null)
            {
                pieceImage.sprite = sprite;
                Debug.Log($"Successfully loaded sprite from Resources: {spriteName}");
            }
            else
            {
                Debug.LogWarning($"Could not load sprite: {spriteName}");
            }
        }

        /// <summary>
        /// Gets the manually assigned sprite for this piece
        /// </summary>
        /// <returns>The sprite or null if not assigned</returns>
        private Sprite GetManuallyAssignedSprite()
        {
            if (color == PieceColor.White)
            {
                switch (type)
                {
                    case PieceType.Pawn: return whitePawnSprite;
                    case PieceType.Rook: return whiteRookSprite;
                    case PieceType.Knight: return whiteKnightSprite;
                    case PieceType.Bishop: return whiteBishopSprite;
                    case PieceType.Queen: return whiteQueenSprite;
                    case PieceType.King: return whiteKingSprite;
                }
            }
            else // PieceColor.Black
            {
                switch (type)
                {
                    case PieceType.Pawn: return blackPawnSprite;
                    case PieceType.Rook: return blackRookSprite;
                    case PieceType.Knight: return blackKnightSprite;
                    case PieceType.Bishop: return blackBishopSprite;
                    case PieceType.Queen: return blackQueenSprite;
                    case PieceType.King: return blackKingSprite;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the sprite name for this piece
        /// </summary>
        /// <returns>The sprite filename</returns>
        private string GetSpriteName()
        {
            string colorPrefix = color == PieceColor.White ? "white" : "black";
            string pieceName = type.ToString().ToLower();
            
            // Handle special case for queen (queen1)
            if (pieceName == "queen")
                pieceName = "queen1";
                
            return $"{colorPrefix}_{pieceName}";
        }

        /// <summary>
        /// Sets the queen sprite for pawn promotion
        /// </summary>
        /// <param name="sprite">The queen sprite to use</param>
        public void SetQueenSprite(Sprite sprite)
        {
            queenSprite = sprite;
        }
        #endregion

        #region Pawn Promotion
        /// <summary>
        /// Promotes a pawn to a queen when it reaches the last rank
        /// </summary>
        public void PromoteToQueen()
        {
            if (type == PieceType.Pawn)
            {
                type = PieceType.Queen;
                LoadSprite(); // This will now use the queenSprite field if assigned
                Debug.Log($"Pawn promoted to Queen at position ({currentTile!.X}, {currentTile!.Y})");
            }
        }

        /// <summary>
        /// Promotes a pawn to the specified piece type
        /// </summary>
        /// <param name="newPieceType">The piece type to promote to</param>
        public void PromoteToPiece(PieceType newPieceType)
        {
            if (type == PieceType.Pawn)
            {
                type = newPieceType;
                LoadSprite();
                Debug.Log($"Pawn promoted to {newPieceType} at position ({currentTile!.X}, {currentTile!.Y})");
            }
        }

        /// <summary>
        /// Checks if a pawn should be promoted (reached the last rank)
        /// </summary>
        /// <returns>True if the pawn should be promoted</returns>
        public bool ShouldPromote()
        {
            if (type != PieceType.Pawn || currentTile == null)
                return false;
            
            // White pawns promote on rank 8 (y = 7), black pawns promote on rank 1 (y = 0)
            return (color == PieceColor.White && currentTile.Y == 7) || 
                   (color == PieceColor.Black && currentTile.Y == 0);
        }
        #endregion

        #region Move Generation
        /// <summary>
        /// Gets all legal moves for this piece
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <param name="forCheck">If true, skips king-in-check filtering to avoid recursion</param>
        /// <returns>List of legal destination tiles</returns>
        public List<Tile> GetLegalMoves(Tile[,] board, bool forCheck = false)
        {
            List<Tile> legalMoves = new List<Tile>();
            
            switch (type)
            {
                case PieceType.Pawn:
                    legalMoves.AddRange(GetPawnMoves(board));
                    break;
                case PieceType.Rook:
                    legalMoves.AddRange(GetRookMoves(board));
                    break;
                case PieceType.Knight:
                    legalMoves.AddRange(GetKnightMoves(board));
                    break;
                case PieceType.Bishop:
                    legalMoves.AddRange(GetBishopMoves(board));
                    break;
                case PieceType.Queen:
                    legalMoves.AddRange(GetQueenMoves(board));
                    break;
                case PieceType.King:
                    legalMoves.AddRange(GetKingMoves(board, forCheck));
                    break;
            }
            
            // Only filter out moves that leave own king in check if not simulating for check
            if (!forCheck)
                legalMoves.RemoveAll(move => !MoveValidator.IsMoveLegal(this, move, board));
            
            return legalMoves;
        }
        #endregion

        #region Piece-Specific Move Generation
        /// <summary>
        /// Gets legal moves for a pawn
        /// </summary>
        private List<Tile> GetPawnMoves(Tile[,] board)
        {
            List<Tile> moves = new List<Tile>();
            int direction = color == PieceColor.White ? 1 : -1;
            int startRank = color == PieceColor.White ? 1 : 6;
            
            int x = currentTile!.X;
            int y = currentTile!.Y;
            
            // Single move forward
            if (IsValidPosition(x, y + direction) && board[x, y + direction].IsEmpty())
            {
                moves.Add(board[x, y + direction]);
                
                // Double move from starting position
                if (y == startRank && board[x, y + 2 * direction].IsEmpty())
                {
                    moves.Add(board[x, y + 2 * direction]);
                }
            }
            
            // Captures (including en passant)
            int[] captureX = { x - 1, x + 1 };
            foreach (int captureXPos in captureX)
            {
                if (IsValidPosition(captureXPos, y + direction))
                {
                    Tile targetTile = board[captureXPos, y + direction];
                    GameManager? gameManager = FindObjectOfType<GameManager>();
                    // Normal capture
                    if (!targetTile.IsEmpty() && targetTile.GetPiece()!.Color != color)
                    {
                        moves.Add(targetTile);
                    }
                    // En passant capture
                    else if (gameManager != null && gameManager.EnPassantTarget == targetTile)
                    {
                        moves.Add(targetTile);
                    }
                }
            }
            
            return moves;
        }

        /// <summary>
        /// Gets legal moves for a rook
        /// </summary>
        private List<Tile> GetRookMoves(Tile[,] board)
        {
            List<Tile> moves = new List<Tile>();
            int x = currentTile!.X;
            int y = currentTile!.Y;
            
            // Horizontal and vertical directions
            int[][] directions = { new int[] { 0, 1 }, new int[] { 0, -1 }, new int[] { 1, 0 }, new int[] { -1, 0 } };
            
            foreach (int[] direction in directions)
            {
                moves.AddRange(GetSlidingMoves(board, x, y, direction[0], direction[1]));
            }
            
            return moves;
        }

        /// <summary>
        /// Gets legal moves for a knight
        /// </summary>
        private List<Tile> GetKnightMoves(Tile[,] board)
        {
            List<Tile> moves = new List<Tile>();
            int x = currentTile!.X;
            int y = currentTile!.Y;
            
            // All possible knight moves
            int[][] knightMoves = {
                new int[] { -2, -1 }, new int[] { -2, 1 }, new int[] { -1, -2 }, new int[] { -1, 2 },
                new int[] { 1, -2 }, new int[] { 1, 2 }, new int[] { 2, -1 }, new int[] { 2, 1 }
            };
            
            foreach (int[] move in knightMoves)
            {
                int newX = x + move[0];
                int newY = y + move[1];
                
                if (IsValidPosition(newX, newY))
                {
                    Tile targetTile = board[newX, newY];
                    if (targetTile.IsEmpty() || targetTile.GetPiece()!.Color != color)
                    {
                        moves.Add(targetTile);
                    }
                }
            }
            
            return moves;
        }

        /// <summary>
        /// Gets legal moves for a bishop
        /// </summary>
        private List<Tile> GetBishopMoves(Tile[,] board)
        {
            List<Tile> moves = new List<Tile>();
            int x = currentTile!.X;
            int y = currentTile!.Y;
            
            // Diagonal directions
            int[][] directions = { new int[] { 1, 1 }, new int[] { 1, -1 }, new int[] { -1, 1 }, new int[] { -1, -1 } };
            
            foreach (int[] direction in directions)
            {
                moves.AddRange(GetSlidingMoves(board, x, y, direction[0], direction[1]));
            }
            
            return moves;
        }

        /// <summary>
        /// Gets legal moves for a queen
        /// </summary>
        private List<Tile> GetQueenMoves(Tile[,] board)
        {
            List<Tile> moves = new List<Tile>();
            moves.AddRange(GetRookMoves(board));
            moves.AddRange(GetBishopMoves(board));
            return moves;
        }

        /// <summary>
        /// Gets legal moves for a king
        /// </summary>
        private List<Tile> GetKingMoves(Tile[,] board, bool forCheck = false)
        {
            List<Tile> moves = new List<Tile>();
            int x = currentTile!.X;
            int y = currentTile!.Y;
            
            // All 8 directions around the king
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    if (dx == 0 && dy == 0) continue;
                    
                    int newX = x + dx;
                    int newY = y + dy;
                    
                    if (IsValidPosition(newX, newY))
                    {
                        Tile targetTile = board[newX, newY];
                        if (targetTile.IsEmpty() || targetTile.GetPiece()!.Color != color)
                        {
                            moves.Add(targetTile);
                        }
                    }
                }
            }
            
            // Castling moves (only when not checking for check to avoid recursion)
            if (!forCheck)
            {
                GameManager? gameManager = FindObjectOfType<GameManager>();
                if (gameManager != null)
                {
                    // Kingside castling
                    if (x == 4 && (y == 0 || y == 7)) // King on e1 or e8
                    {
                        if (gameManager.IsCastlingLegal(color, true))
                        {
                            // Add castling move (king moves 2 squares towards rook)
                            int castlingX = x + 2;
                            if (IsValidPosition(castlingX, y))
                            {
                                moves.Add(board[castlingX, y]);
                            }
                        }
                    }
                    
                    // Queenside castling
                    if (x == 4 && (y == 0 || y == 7)) // King on e1 or e8
                    {
                        if (gameManager.IsCastlingLegal(color, false))
                        {
                            // Add castling move (king moves 2 squares towards rook)
                            int castlingX = x - 2;
                            if (IsValidPosition(castlingX, y))
                            {
                                moves.Add(board[castlingX, y]);
                            }
                        }
                    }
                }
            }
            
            return moves;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Gets sliding moves for pieces that move in straight lines
        /// </summary>
        private List<Tile> GetSlidingMoves(Tile[,] board, int startX, int startY, int dx, int dy)
        {
            List<Tile> moves = new List<Tile>();
            int x = startX + dx;
            int y = startY + dy;
            
            while (IsValidPosition(x, y))
            {
                Tile targetTile = board[x, y];
                
                if (targetTile.IsEmpty())
                {
                    moves.Add(targetTile);
                }
                else
                {
                    if (targetTile.GetPiece()!.Color != color)
                    {
                        moves.Add(targetTile);
                    }
                    break; // Stop sliding when we hit a piece
                }
                
                x += dx;
                y += dy;
            }
            
            return moves;
        }

        /// <summary>
        /// Checks if a position is valid on the board
        /// </summary>
        private bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }
        #endregion

        #region Movement Tracking
        /// <summary>
        /// Records that this piece has moved
        /// </summary>
        /// <param name="turnNumber">The current turn number</param>
        public void RecordMove(int turnNumber)
        {
            hasMoved = true;
            lastMoveTurn = turnNumber;
        }
        #endregion
    }
} 