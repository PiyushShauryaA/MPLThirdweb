using UnityEngine;
using UnityEditor;

namespace CanvasChess
{
    /// <summary>
    /// Debug utility to check last move highlight object assignments
    /// </summary>
    public class LastMoveHighlightDebugger : EditorWindow
    {
        [MenuItem("Tools/Chess/Debug Last Move Highlights")]
        public static void DebugLastMoveHighlights()
        {
            // Find all tile prefabs
            string[] tilePrefabPaths = {
                "Assets/Prefabs/LightTile.prefab",
                "Assets/Prefabs/DarkTile.prefab"
            };

            foreach (string prefabPath in tilePrefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab != null)
                {
                    DebugTilePrefab(prefab, prefabPath);
                }
            }

            // Also check tiles in the current scene
            Tile[] tilesInScene = FindObjectsOfType<Tile>();
            Debug.Log($"Found {tilesInScene.Length} tiles in scene");
            
            foreach (Tile tile in tilesInScene)
            {
                Debug.Log($"Tile at {tile.X},{tile.Y}: lastMoveHighlightObject = {tile.lastMoveHighlightObject != null}");
                if (tile.lastMoveHighlightObject == null)
                {
                    Debug.LogWarning($"Tile at {tile.X},{tile.Y} has null lastMoveHighlightObject!");
                }
            }
        }

        private static void DebugTilePrefab(GameObject prefab, string prefabPath)
        {
            Debug.Log($"Checking prefab: {prefabPath}");
            
            Tile tileComponent = prefab.GetComponent<Tile>();
            if (tileComponent != null)
            {
                Debug.Log($"Tile component found, lastMoveHighlightObject = {tileComponent.lastMoveHighlightObject != null}");
                
                if (tileComponent.lastMoveHighlightObject == null)
                {
                    Debug.LogWarning($"Tile component in {prefabPath} has null lastMoveHighlightObject!");
                }
                else
                {
                    Debug.Log($"lastMoveHighlightObject is assigned and active: {tileComponent.lastMoveHighlightObject.activeInHierarchy}");
                }
            }
            else
            {
                Debug.LogError($"No Tile component found in {prefabPath}");
            }

            // Check if the LastMoveHighlightObject exists as a child
            Transform lastMoveHighlight = prefab.transform.Find("LastMoveHighlightObject");
            if (lastMoveHighlight != null)
            {
                Debug.Log($"LastMoveHighlightObject child found in {prefabPath}");
            }
            else
            {
                Debug.LogWarning($"No LastMoveHighlightObject child found in {prefabPath}");
            }
        }
    }
} 