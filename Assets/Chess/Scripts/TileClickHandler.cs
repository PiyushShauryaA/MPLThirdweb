using UnityEngine;
using UnityEngine.EventSystems;

namespace CanvasChess
{
    /// <summary>
    /// Handles click events on tiles and forwards them to the GameManager
    /// </summary>
    public class TileClickHandler : MonoBehaviour, IPointerClickHandler
    {
        #region Serialized Fields
        [SerializeField] private Tile tile = null!;
        [SerializeField] private GameManager gameManager = null!;
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            if (tile == null)
                tile = GetComponent<Tile>();
            
            if (gameManager == null)
                gameManager = FindObjectOfType<GameManager>();
        }
        #endregion

        #region Input Handling
        /// <summary>
        /// Handles pointer click events on the tile
        /// </summary>
        /// <param name="eventData">The pointer event data</param>
        public void OnPointerClick(PointerEventData eventData)
        {
            if (gameManager != null && tile != null)
            {
                gameManager.OnTileClicked(tile);
            }
        }
        #endregion
    }
} 