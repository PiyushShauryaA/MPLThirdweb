using UnityEngine;
using UnityEngine.UI;

namespace CanvasChess
{
    /// <summary>
    /// Represents a single tile on the chess board
    /// </summary>
    public class Tile : MonoBehaviour
    {
        #region Serialized Fields
        [Header("Tile Properties")]
        [SerializeField] private int x;
        [SerializeField] private int y;
        [SerializeField] private Piece? occupyingPiece;
        
        [Header("Visual Components")]
        [SerializeField] private Image tileImage;
        [SerializeField] private Color lightColor = new Color(0.93f, 0.93f, 0.93f, 1f); // #EEE
        [SerializeField] private Color darkColor = new Color(0.33f, 0.33f, 0.33f, 1f); // #555
       
        [SerializeField] private GameObject highlightObject;
        public GameObject checkHighlightObject;
        public GameObject checkmateHighlightObject;
        public GameObject lastMoveHighlightObject;
        //[SerializeField] private Color checkHighlightColor = Color.red;
        
        private Color originalColor;
        private bool isHighlighted = false;
        #endregion

        #region Properties
        /// <summary>
        /// X coordinate of the tile (0-7)
        /// </summary>
        public int X => x;

        /// <summary>
        /// Y coordinate of the tile (0-7)
        /// </summary>
        public int Y => y;

        /// <summary>
        /// The piece currently occupying this tile, if any
        /// </summary>
        public Piece? OccupyingPiece
        {
            get => occupyingPiece;
            set => occupyingPiece = value;
        }

        /// <summary>
        /// Whether this tile is currently highlighted
        /// </summary>
        public bool IsHighlighted => isHighlighted;

        /// <summary>
        /// The color used for check/checkmate highlighting
        /// </summary>
        //public Color CheckHighlightColor => checkHighlightColor;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (tileImage == null)
                tileImage = GetComponent<Image>();
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes the tile with coordinates and sets its color
        /// </summary>
        /// <param name="xPos">X coordinate</param>
        /// <param name="yPos">Y coordinate</param>
        public void Initialize(int xPos, int yPos)
        {
            x = xPos;
            y = yPos;
            
            // Set tile color based on position (checkerboard pattern)
            bool isLightSquare = (x + y) % 2 == 0;
            originalColor = isLightSquare ? lightColor : darkColor;
            tileImage.color = originalColor;
        }
        #endregion

        #region Visual Management
        /// <summary>
        /// Highlights the tile with the specified color
        /// </summary>
        /// <param name="highlight">Whether to highlight the tile</param>
        /// <param name="color">The color to highlight the tile with, or null for default highlight color</param>
        public void Highlight(bool highlight, Color? color = null)
        {
            isHighlighted = highlight;
            if (tileImage == null)
                tileImage = GetComponent<Image>();
            if (highlight)
            {
                highlightObject.SetActive(true);
            }
            else
            {
                highlightObject.SetActive(false);
            }
        }

        /// <summary>
        /// Clears the highlight and restores the original color
        /// </summary>
        public void ClearHighlight()
        {
            Highlight(false);
        }

        /// <summary>
        /// Highlights the tile as the last move
        /// </summary>
        public void HighlightLastMove(bool highlight)
        {
            Debug.Log($"Tile.HighlightLastMove called: tile={X},{Y}, highlight={highlight}, lastMoveHighlightObject={lastMoveHighlightObject != null}");
            if (lastMoveHighlightObject != null)
            {
                lastMoveHighlightObject.SetActive(highlight);
                Debug.Log($"Set lastMoveHighlightObject active to {highlight}");
            }
            else
            {
                Debug.LogWarning($"lastMoveHighlightObject is null for tile at {X},{Y}");
            }
        }

        /// <summary>
        /// Clears the last move highlight
        /// </summary>
        public void ClearLastMoveHighlight()
        {
            Debug.Log($"Tile.ClearLastMoveHighlight called: tile={X},{Y}");
            HighlightLastMove(false);
        }
        #endregion

        #region Piece Management
        /// <summary>
        /// Checks if this tile is empty (no piece occupying it)
        /// </summary>
        /// <returns>True if the tile is empty</returns>
        public bool IsEmpty()
        {
            return occupyingPiece == null;
        }

        /// <summary>
        /// Checks if this tile contains a piece of the specified color
        /// </summary>
        /// <param name="color">The color to check for</param>
        /// <returns>True if the tile contains a piece of the specified color</returns>
        public bool ContainsPieceOfColor(PieceColor color)
        {
            return occupyingPiece != null && occupyingPiece.Color == color;
        }

        /// <summary>
        /// Gets the piece at this tile, if any
        /// </summary>
        /// <returns>The piece at this tile, or null if empty</returns>
        public Piece? GetPiece()
        {
            return occupyingPiece;
        }

        /// <summary>
        /// Sets the piece at this tile
        /// </summary>
        /// <param name="piece">The piece to place on this tile</param>
        public void SetPiece(Piece? piece)
        {
            occupyingPiece = piece;
            if (piece != null)
            {
                piece.CurrentTile = this;
                piece.transform.SetParent(this.transform, false);
                piece.transform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Removes the piece from this tile
        /// </summary>
        /// <returns>The piece that was removed, or null if tile was empty</returns>
        public Piece? RemovePiece()
        {
            Piece? piece = occupyingPiece;
            if (occupyingPiece != null)
            {
                occupyingPiece.CurrentTile = null;
                occupyingPiece = null;
            }
            return piece;
        }
        #endregion
    }
} 