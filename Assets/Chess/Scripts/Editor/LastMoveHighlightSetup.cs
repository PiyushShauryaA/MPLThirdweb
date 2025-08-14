using UnityEngine;
using UnityEditor;

namespace CanvasChess
{
    /// <summary>
    /// Editor utility to help set up last move highlight objects on tiles
    /// </summary>
    public class LastMoveHighlightSetup : EditorWindow
    {
        [MenuItem("Tools/Chess/Setup Last Move Highlights")]
        public static void SetupLastMoveHighlights()
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
                    SetupTilePrefab(prefab, prefabPath);
                }
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Last move highlight setup completed!");
        }

        private static void SetupTilePrefab(GameObject prefab, string prefabPath)
        {
            bool modified = false;

            // Check if last move highlight object already exists
            Transform lastMoveHighlight = prefab.transform.Find("LastMoveHighlightObject");
            if (lastMoveHighlight == null)
            {
                // Create last move highlight object
                GameObject lastMoveHighlightObj = new GameObject("LastMoveHighlightObject");
                lastMoveHighlightObj.transform.SetParent(prefab.transform);
                lastMoveHighlightObj.transform.localPosition = Vector3.zero;
                lastMoveHighlightObj.transform.localScale = Vector3.one;

                // Add RectTransform
                RectTransform rectTransform = lastMoveHighlightObj.AddComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.anchoredPosition = Vector2.zero;
                rectTransform.sizeDelta = Vector2.zero;

                // Add CanvasRenderer
                lastMoveHighlightObj.AddComponent<CanvasRenderer>();

                // Add Image component with yellow highlight color
                UnityEngine.UI.Image image = lastMoveHighlightObj.AddComponent<UnityEngine.UI.Image>();
                image.color = new Color(1f, 1f, 0f, 0.3f); // Semi-transparent yellow
                image.sprite = null; // No sprite needed for color overlay

                // Set as inactive by default
                lastMoveHighlightObj.SetActive(false);

                modified = true;
                Debug.Log($"Created last move highlight object for {prefabPath}");
            }

            // Update the Tile component to reference the last move highlight object
            Tile tileComponent = prefab.GetComponent<Tile>();
            if (tileComponent != null)
            {
                Transform lastMoveHighlightTransform = prefab.transform.Find("LastMoveHighlightObject");
                if (lastMoveHighlightTransform != null)
                {
                    // Use reflection to set the lastMoveHighlightObject field
                    var field = typeof(Tile).GetField("lastMoveHighlightObject", 
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(tileComponent, lastMoveHighlightTransform.gameObject);
                        modified = true;
                        Debug.Log($"Updated Tile component for {prefabPath}");
                    }
                }
            }

            if (modified)
            {
                PrefabUtility.SavePrefabAsset(prefab);
            }
        }
    }
} 