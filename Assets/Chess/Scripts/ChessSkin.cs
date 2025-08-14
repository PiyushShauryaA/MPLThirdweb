using UnityEngine;

namespace CanvasChess
{
    /// <summary>
    /// ScriptableObject to hold references to all chess piece and tile prefabs for skinning
    /// </summary>
    [CreateAssetMenu(fileName = "ChessSkin", menuName = "CanvasChess/Chess Skin", order = 1)]
    public class ChessSkin : ScriptableObject
    {
        #region Piece Prefabs
        [Header("Piece Prefabs")]
        public GameObject whitePawn;
        public GameObject whiteRook;
        public GameObject whiteKnight;
        public GameObject whiteBishop;
        public GameObject whiteQueen;
        public GameObject whiteKing;
        public GameObject blackPawn;
        public GameObject blackRook;
        public GameObject blackKnight;
        public GameObject blackBishop;
        public GameObject blackQueen;
        public GameObject blackKing;
        #endregion

        #region Tile Prefabs
        [Header("Tile Prefabs")]
        public GameObject lightTile;
        public GameObject darkTile;
        #endregion
    }
} 