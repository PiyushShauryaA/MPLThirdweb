using UnityEngine;
using System.Collections.Generic;

namespace CanvasChess
{
    /// <summary>
    /// Static helper methods for move validation and game state checking
    /// </summary>
    public static class MoveValidator
    {
        #region Move Validation
        /// <summary>
        /// Checks if a move is legal (doesn't leave own king in check)
        /// </summary>
        /// <param name="piece">The piece making the move</param>
        /// <param name="targetTile">The destination tile</param>
        /// <param name="board">The current board state</param>
        /// <returns>True if the move is legal</returns>
        public static bool IsMoveLegal(Piece piece, Tile targetTile, Tile[,] board)
        {
            // Simulate the move
            Tile originalTile = piece.CurrentTile!;
            Piece? capturedPiece = targetTile.GetPiece();
            
            // Make the move
            originalTile.RemovePiece();
            targetTile.SetPiece(piece);
            
            // Check if own king is in check
            bool isLegal = !IsKingInCheck(piece.Color, board);
            
            // Undo the move
            targetTile.RemovePiece();
            originalTile.SetPiece(piece);
            if (capturedPiece != null)
            {
                targetTile.SetPiece(capturedPiece);
            }
            
            return isLegal;
        }
        #endregion

        #region Check Detection
        /// <summary>
        /// Checks if a king of the specified color is in check
        /// </summary>
        /// <param name="kingColor">The color of the king to check</param>
        /// <param name="board">The current board state</param>
        /// <returns>True if the king is in check</returns>
        public static bool IsKingInCheck(PieceColor kingColor, Tile[,] board)
        {
            // Find the king
            Tile? kingTile = FindKing(kingColor, board);
            if (kingTile == null) return false;
            
            // Check if any opponent piece can attack the king
            PieceColor opponentColor = kingColor == PieceColor.White ? PieceColor.Black : PieceColor.White;
            
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece? piece = board[x, y].GetPiece();
                    if (piece != null && piece.Color == opponentColor)
                    {
                        // Use forCheck=true to avoid recursion
                        List<Tile> moves = piece.GetLegalMoves(board, true);
                        if (moves.Contains(kingTile))
                        {
                            return true;
                        }
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// Finds the tile containing the king of the specified color
        /// </summary>
        /// <param name="kingColor">The color of the king to find</param>
        /// <param name="board">The current board state</param>
        /// <returns>The tile containing the king, or null if not found</returns>
        public static Tile? FindKing(PieceColor kingColor, Tile[,] board)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece? piece = board[x, y].GetPiece();
                    if (piece != null && piece.Type == PieceType.King && piece.Color == kingColor)
                    {
                        return board[x, y];
                    }
                }
            }
            return null;
        }
        #endregion

        #region Game End Conditions
        /// <summary>
        /// Checks if a player is in checkmate
        /// </summary>
        /// <param name="playerColor">The color of the player to check</param>
        /// <param name="board">The current board state</param>
        /// <returns>True if the player is in checkmate</returns>
        public static bool IsCheckmate(PieceColor playerColor, Tile[,] board)
        {
            // Must be in check first
            if (!IsKingInCheck(playerColor, board))
                return false;
            
            // Check if any legal move can get out of check
            return !HasLegalMoves(playerColor, board);
        }

        /// <summary>
        /// Checks if a player is in stalemate
        /// </summary>
        /// <param name="playerColor">The color of the player to check</param>
        /// <param name="board">The current board state</param>
        /// <returns>True if the player is in stalemate</returns>
        public static bool IsStalemate(PieceColor playerColor, Tile[,] board)
        {
            // Must not be in check
            if (IsKingInCheck(playerColor, board))
                return false;
            
            // Check if any legal move is available
            return !HasLegalMoves(playerColor, board);
        }

        /// <summary>
        /// Checks if a player has any legal moves
        /// </summary>
        /// <param name="playerColor">The color of the player to check</param>
        /// <param name="board">The current board state</param>
        /// <returns>True if the player has legal moves</returns>
        public static bool HasLegalMoves(PieceColor playerColor, Tile[,] board)
        {
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece? piece = board[x, y].GetPiece();
                    if (piece != null && piece.Color == playerColor)
                    {
                        List<Tile> moves = piece.GetLegalMoves(board);
                        if (moves.Count > 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        #endregion

        #region Utility Methods
        /// <summary>
        /// Checks if a position is valid on the board
        /// </summary>
        /// <param name="x">X coordinate</param>
        /// <param name="y">Y coordinate</param>
        /// <returns>True if the position is valid</returns>
        public static bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }

        /// <summary>
        /// Checks if there's a clear line of sight between two positions
        /// </summary>
        /// <param name="startX">Starting X coordinate</param>
        /// <param name="startY">Starting Y coordinate</param>
        /// <param name="endX">Ending X coordinate</param>
        /// <param name="endY">Ending Y coordinate</param>
        /// <param name="board">The current board state</param>
        /// <returns>True if there's a clear line of sight</returns>
        public static bool HasLineOfSight(int startX, int startY, int endX, int endY, Tile[,] board)
        {
            int dx = endX - startX;
            int dy = endY - startY;
            
            // Check if it's a straight line
            if (dx != 0 && dy != 0 && Mathf.Abs(dx) != Mathf.Abs(dy))
                return false;
            
            // Normalize direction
            int stepX = dx == 0 ? 0 : dx / Mathf.Abs(dx);
            int stepY = dy == 0 ? 0 : dy / Mathf.Abs(dy);
            
            // Check each position along the path (excluding start and end)
            int x = startX + stepX;
            int y = startY + stepY;
            
            while (x != endX || y != endY)
            {
                if (!IsValidPosition(x, y) || !board[x, y].IsEmpty())
                    return false;
                
                x += stepX;
                y += stepY;
            }
            
            return true;
        }
        #endregion

        #region Board Analysis
        /// <summary>
        /// Calculates a simple hash for the current board position
        /// </summary>
        /// <param name="board">The current board state</param>
        /// <returns>A hash value representing the board position</returns>
        public static int CalculateBoardHash(Tile[,] board)
        {
            int hash = 0;
            for (int x = 0; x < 8; x++)
            {
                for (int y = 0; y < 8; y++)
                {
                    Piece? piece = board[x, y].GetPiece();
                    if (piece != null)
                    {
                        int pieceValue = (int)piece.Type + ((int)piece.Color * 6);
                        hash = hash * 31 + pieceValue + (x * 8 + y) * 100;
                    }
                }
            }
            return hash;
        }
        #endregion
    }
} 