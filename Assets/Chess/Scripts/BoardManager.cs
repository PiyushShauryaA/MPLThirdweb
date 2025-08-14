using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CanvasChess; // For PlayerSettings
using Photon.Pun;

namespace CanvasChess
{
    /// <summary>
    /// Manages the chess board generation and piece placement
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class BoardManager : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Board Settings")]
        [SerializeField] private GameObject tilePrefab = null!;
        [SerializeField] private GameObject piecePrefab = null!;
        [SerializeField] private Transform boardContainer = null!;
        [SerializeField] private Transform boardParent = null!;
        [SerializeField] private Transform whitePlayerTurnIndicator = null!;
        [SerializeField] private Transform blackPlayerTurnIndicator = null!;
        [SerializeField] private GridLayoutGroup gridLayout = null!;
        
        [Header("Board State")]
        [SerializeField] private Tile[,] board = new Tile[8, 8];
        [SerializeField] private List<Piece> allPieces = new List<Piece>();
        
        [Header("Initial Position")]
        [SerializeField] private string initialFEN = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        [Header("Skin")]
        public ChessSkin chessSkin;
        #endregion

        #region Properties
        /// <summary>
        /// The current board state as a 2D array of tiles
        /// </summary>
        public Tile[,] Board => board;

        /// <summary>
        /// All pieces currently on the board
        /// </summary>
        public List<Piece> AllPieces => allPieces;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Auto-assign missing references for scene auto-builder compatibility
            if (tilePrefab == null)
            {
                var tileObj = Resources.Load<GameObject>("Prefabs/Tile");
                if (tileObj != null) tilePrefab = tileObj;
                else
                {
                    var found = GameObject.Find("Tile");
                    if (found != null) tilePrefab = found;
                }
            }
            if (piecePrefab == null)
            {
                var pieceObj = Resources.Load<GameObject>("Prefabs/Piece");
                if (pieceObj != null) piecePrefab = pieceObj;
                else
                {
                    var found = GameObject.Find("Piece");
                    if (found != null) piecePrefab = found;
                }
            }
            if (boardContainer == null)
                boardContainer = transform;
            if (gridLayout == null)
                gridLayout = GetComponent<GridLayoutGroup>();
        }

        private void Start()
        {
            InitializeBoard();
            SetupInitialPosition();
            RotateBoardIfNeeded();
            AdjustPieceOrientations();
        }

        private void RotateBoardIfNeeded()
        {
            // For multiplayer: Master client plays white, others play black
            bool shouldRotate = false;
            
            if (PhotonNetwork.IsConnected)
            {
                // In multiplayer, rotate board if not master client (black player)
                shouldRotate = !PhotonNetwork.IsMasterClient;
            }
            else
            {
                // In single player, use PlayerSettings
                shouldRotate = PlayerSettingsChess.PlayerColor == PieceColor.Black;
            }
            
            if (shouldRotate)
            {
                boardParent.localRotation = Quaternion.Euler(0, 0, 180);
                whitePlayerTurnIndicator.localRotation = Quaternion.Euler(0, 0, 180);
                blackPlayerTurnIndicator.localRotation = Quaternion.Euler(0, 0, 180);
            }
            else
            {
                boardParent.localRotation = Quaternion.identity;
            }
        }

        private void AdjustPieceOrientations()
        {
            // For multiplayer: Master client plays white, others play black
            bool flip = false;
            
            if (PhotonNetwork.IsConnected)
            {
                // In multiplayer, flip pieces if not master client (black player)
                flip = !PhotonNetwork.IsMasterClient;
            }
            else
            {
                // In single player, use PlayerSettings
                flip = PlayerSettingsChess.PlayerColor == PieceColor.Black;
            }
            
            foreach (var piece in allPieces)
            {
                if (piece != null)
                {
                    piece.transform.localRotation = flip ? Quaternion.Euler(0, 0, 180) : Quaternion.identity;
                }
            }
        }
        #endregion

        #region Board Initialization
        /// <summary>
        /// Initializes the chess board with tiles
        /// </summary>
        private void InitializeBoard()
        {
            if (gridLayout == null)
            {
                gridLayout = boardContainer.GetComponent<GridLayoutGroup>();
                if (gridLayout == null)
                {
                    gridLayout = boardContainer.gameObject.AddComponent<GridLayoutGroup>();
                }
            }

            // Configure grid layout
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = 8;
            gridLayout.cellSize = new Vector2(145, 145);
            gridLayout.spacing = Vector2.zero;
            gridLayout.childAlignment = TextAnchor.MiddleCenter;

            // Use ChessSkin for tiles if assigned
            // Create tiles
            for (int y = 7; y >= 0; y--)
            {
                for (int x = 0; x < 8; x++)
                {
                    GameObject tilePrefabToUse = null;
                    if (chessSkin != null)
                    {
                        tilePrefabToUse = ((x + y) % 2 == 0) ? chessSkin.lightTile : chessSkin.darkTile;
                    }
                    if (tilePrefabToUse == null) tilePrefabToUse = tilePrefab;
                    GameObject tileGO = Instantiate(tilePrefabToUse, boardContainer);
                    Tile tile = tileGO.GetComponent<Tile>();
                    tile.Initialize(x, y);
                    board[x, y] = tile;
                }
            }
        }

        /// <summary>
        /// Sets up the initial chess position from FEN notation
        /// </summary>
        private void SetupInitialPosition()
        {
            string[] ranks = initialFEN.Split('/');
            
            for (int y = 7; y >= 0; y--)
            {
                int x = 0;
                string rank = ranks[7 - y]; // Convert from chess notation to array indices
                
                foreach (char c in rank)
                {
                    if (char.IsDigit(c))
                    {
                        // Empty squares
                        int emptyCount = int.Parse(c.ToString());
                        x += emptyCount;
                    }
                    else
                    {
                        // Piece
                        CreatePiece(c, x, y);
                        x++;
                    }
                }
            }
        }
        #endregion

        #region Piece Creation
        /// <summary>
        /// Creates a piece based on FEN character
        /// </summary>
        /// <param name="fenChar">The FEN character representing the piece</param>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        private void CreatePiece(char fenChar, int x, int y)
        {
            PieceType pieceType = GetPieceType(fenChar);
            PieceColor pieceColor = char.IsUpper(fenChar) ? PieceColor.White : PieceColor.Black;
            GameObject piecePrefabToUse = piecePrefab;
            if (chessSkin != null)
            {
                switch (pieceType)
                {
                    case PieceType.Pawn: piecePrefabToUse = (pieceColor == PieceColor.White) ? chessSkin.whitePawn : chessSkin.blackPawn; break;
                    case PieceType.Rook: piecePrefabToUse = (pieceColor == PieceColor.White) ? chessSkin.whiteRook : chessSkin.blackRook; break;
                    case PieceType.Knight: piecePrefabToUse = (pieceColor == PieceColor.White) ? chessSkin.whiteKnight : chessSkin.blackKnight; break;
                    case PieceType.Bishop: piecePrefabToUse = (pieceColor == PieceColor.White) ? chessSkin.whiteBishop : chessSkin.blackBishop; break;
                    case PieceType.Queen: piecePrefabToUse = (pieceColor == PieceColor.White) ? chessSkin.whiteQueen : chessSkin.blackQueen; break;
                    case PieceType.King: piecePrefabToUse = (pieceColor == PieceColor.White) ? chessSkin.whiteKing : chessSkin.blackKing; break;
                }
            }
            GameObject pieceGO = Instantiate(piecePrefabToUse, board[x, y].transform);
            Piece piece = pieceGO.GetComponent<Piece>();
            if (piece == null)
            {
                Debug.LogError("Piece prefab is missing the Piece script!");
            }
            else
            {
                piece.Initialize(pieceType, pieceColor);
                board[x, y].SetPiece(piece);
                allPieces.Add(piece);
            }
        }

        /// <summary>
        /// Converts FEN character to piece type
        /// </summary>
        /// <param name="fenChar">The FEN character</param>
        /// <returns>The corresponding piece type</returns>
        private PieceType GetPieceType(char fenChar)
        {
            char lowerChar = char.ToLower(fenChar);
            return lowerChar switch
            {
                'p' => PieceType.Pawn,
                'r' => PieceType.Rook,
                'n' => PieceType.Knight,
                'b' => PieceType.Bishop,
                'q' => PieceType.Queen,
                'k' => PieceType.King,
                _ => PieceType.Pawn
            };
        }
        #endregion

        #region Board Queries
        /// <summary>
        /// Gets a tile at the specified coordinates
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>The tile at the specified position</returns>
        public Tile? GetTile(int x, int y)
        {
            if (x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                return board[x, y];
            }
            return null;
        }

        /// <summary>
        /// Gets all pieces of a specific color
        /// </summary>
        /// <param name="color">The color of pieces to find</param>
        /// <returns>List of pieces of the specified color</returns>
        public List<Piece> GetPiecesOfColor(PieceColor color)
        {
            List<Piece> pieces = new List<Piece>();
            foreach (Piece piece in allPieces)
            {
                if (piece.Color == color)
                {
                    pieces.Add(piece);
                }
            }
            return pieces;
        }
        #endregion

        #region Piece Management
        /// <summary>
        /// Removes a piece from the board
        /// </summary>
        /// <param name="piece">The piece to remove</param>
        public void RemovePiece(Piece piece)
        {
            if (piece.CurrentTile != null)
            {
                piece.CurrentTile.RemovePiece();
            }
            allPieces.Remove(piece);
            Destroy(piece.gameObject);
        }
        #endregion

        #region Visual Management
        /// <summary>
        /// Clears all highlights from the board
        /// </summary>
        public void ClearAllHighlights()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    board[x, y].ClearHighlight();
                    // Don't clear last move highlights here - they should be managed separately
                }
            }
        }

        /// <summary>
        /// Clears all highlights including last move highlights from the board
        /// </summary>
        public void ClearAllHighlightsIncludingLastMove()
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    board[x, y].ClearHighlight();
                    board[x, y].ClearLastMoveHighlight();
                }
            }
        }

        /// <summary>
        /// Highlights legal moves for a piece
        /// </summary>
        /// <param name="piece">The piece to show moves for</param>
        public void HighlightLegalMoves(Piece piece)
        {
            ClearAllHighlights();
            List<Tile> legalMoves = piece.GetLegalMoves(board);
            
            foreach (Tile tile in legalMoves)
            {
                tile.Highlight(true);
            }
        }
        #endregion

        #region Board Control
        /// <summary>
        /// Resets the board to the initial position
        /// </summary>
        public void ResetBoard()
        {
            // Remove all pieces
            foreach (Piece piece in allPieces)
            {
                if (piece != null)
                {
                    Destroy(piece.gameObject);
                }
            }
            allPieces.Clear();
            
            // Clear all tiles
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    board[x, y].RemovePiece();
                    board[x, y].ClearHighlight();
                    board[x, y].ClearLastMoveHighlight();
                }
            }
            
            // Setup initial position
            SetupInitialPosition();
        }

        /// <summary>
        /// Forces board rotation update (useful for multiplayer)
        /// </summary>
        public void ForceBoardRotation()
        {
            RotateBoardIfNeeded();
            AdjustPieceOrientations();
        }
        #endregion

        #region FEN Notation
        /// <summary>
        /// Gets the current board state as FEN notation
        /// </summary>
        /// <returns>FEN string representing the current board state</returns>
        public string GetFEN()
        {
            string fen = "";
            
            for (int y = 7; y >= 0; y--)
            {
                int emptyCount = 0;
                
                for (int x = 0; x < 8; x++)
                {
                    Piece? piece = board[x, y].GetPiece();
                    
                    if (piece == null)
                    {
                        emptyCount++;
                    }
                    else
                    {
                        if (emptyCount > 0)
                        {
                            fen += emptyCount.ToString();
                            emptyCount = 0;
                        }
                        
                        char pieceChar = GetFENChar(piece);
                        fen += pieceChar;
                    }
                }
                
                if (emptyCount > 0)
                {
                    fen += emptyCount.ToString();
                }
                
                if (y > 0)
                {
                    fen += "/";
                }
            }
            
            return fen;
        }

        /// <summary>
        /// Converts a piece to its FEN character representation
        /// </summary>
        /// <param name="piece">The piece to convert</param>
        /// <returns>The FEN character</returns>
        private char GetFENChar(Piece piece)
        {
            char baseChar = piece.Type switch
            {
                PieceType.Pawn => 'p',
                PieceType.Rook => 'r',
                PieceType.Knight => 'n',
                PieceType.Bishop => 'b',
                PieceType.Queen => 'q',
                PieceType.King => 'k',
                _ => 'p'
            };
            
            return piece.Color == PieceColor.White ? char.ToUpper(baseChar) : baseChar;
        }
        #endregion
    }
} 