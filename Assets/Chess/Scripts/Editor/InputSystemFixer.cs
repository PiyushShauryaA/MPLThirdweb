#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CanvasChess.Editor
{
    /// <summary>
    /// Utility to fix input system issues in chess scenes
    /// </summary>
    public class InputSystemFixer : EditorWindow
    {
        [MenuItem("CanvasChess/Fix Input System")]
        public static void ShowWindow()
        {
            GetWindow<InputSystemFixer>("Input System Fixer");
        }

        private void OnGUI()
        {
            GUILayout.Label("Input System Fixer", EditorStyles.boldLabel);
            GUILayout.Label("This will fix common input issues in your scenes.");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Fix Current Scene Input"))
            {
                FixCurrentSceneInput();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Add EventSystem to Scene"))
            {
                AddEventSystemToScene();
            }
            
            if (GUILayout.Button("Add TileClickHandlers to All Tiles"))
            {
                AddTileClickHandlers();
            }
            
            if (GUILayout.Button("Check Scene Input Setup"))
            {
                CheckSceneInputSetup();
            }
        }

        /// <summary>
        /// Fixes input issues in the current scene
        /// </summary>
        private static void FixCurrentSceneInput()
        {
            // Check and add EventSystem
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
                Debug.Log("EventSystem added to scene");
            }
            else
            {
                Debug.Log("EventSystem already exists in scene");
            }

            // Check Canvas setup
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    canvas.gameObject.AddComponent<GraphicRaycaster>();
                    Debug.Log("GraphicRaycaster added to Canvas");
                }
                else
                {
                    Debug.Log("GraphicRaycaster already exists on Canvas");
                }
            }
            else
            {
                Debug.LogWarning("No Canvas found in scene!");
            }

            // Check TileClickHandlers
            AddTileClickHandlers();

            EditorUtility.DisplayDialog("Input System Fix", 
                "Input system has been checked and fixed!\n\n" +
                "• EventSystem: " + (FindObjectOfType<EventSystem>() != null ? "✓ Present" : "✗ Missing") + "\n" +
                "• GraphicRaycaster: " + (FindObjectOfType<GraphicRaycaster>() != null ? "✓ Present" : "✗ Missing") + "\n" +
                "• TileClickHandlers: Added to all tiles\n\n" +
                "Try clicking on tiles now.", "OK");
        }

        /// <summary>
        /// Adds EventSystem to the scene if missing
        /// </summary>
        private static void AddEventSystemToScene()
        {
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
                Debug.Log("EventSystem added to scene");
                EditorUtility.DisplayDialog("EventSystem Added", "EventSystem has been added to the scene.", "OK");
            }
            else
            {
                Debug.Log("EventSystem already exists in scene");
                EditorUtility.DisplayDialog("EventSystem Check", "EventSystem already exists in the scene.", "OK");
            }
        }

        /// <summary>
        /// Adds TileClickHandler components to all tiles
        /// </summary>
        private static void AddTileClickHandlers()
        {
            Tile[] tiles = FindObjectsOfType<Tile>();
            int addedCount = 0;
            
            foreach (Tile tile in tiles)
            {
                TileClickHandler clickHandler = tile.GetComponent<TileClickHandler>();
                if (clickHandler == null)
                {
                    tile.gameObject.AddComponent<TileClickHandler>();
                    addedCount++;
                }
            }
            
            Debug.Log($"Added TileClickHandler to {addedCount} tiles");
            EditorUtility.DisplayDialog("TileClickHandlers Added", 
                $"Added TileClickHandler components to {addedCount} tiles.", "OK");
        }

        /// <summary>
        /// Checks the current scene's input setup
        /// </summary>
        private static void CheckSceneInputSetup()
        {
            EventSystem eventSystem = FindObjectOfType<EventSystem>();
            GraphicRaycaster raycaster = FindObjectOfType<GraphicRaycaster>();
            Tile[] tiles = FindObjectsOfType<Tile>();
            TileClickHandler[] clickHandlers = FindObjectsOfType<TileClickHandler>();
            
            string report = "Input System Check Report:\n\n";
            report += $"EventSystem: {(eventSystem != null ? "✓ Present" : "✗ Missing")}\n";
            report += $"GraphicRaycaster: {(raycaster != null ? "✓ Present" : "✗ Missing")}\n";
            report += $"Tiles: {tiles.Length}\n";
            report += $"Tiles with ClickHandlers: {clickHandlers.Length}\n";
            
            if (eventSystem == null)
                report += "\n⚠️ EventSystem is missing! This will prevent all input.";
            
            if (raycaster == null)
                report += "\n⚠️ GraphicRaycaster is missing! This will prevent UI input.";
            
            if (clickHandlers.Length < tiles.Length)
                report += "\n⚠️ Some tiles are missing TileClickHandler components.";
            
            EditorUtility.DisplayDialog("Input System Check", report, "OK");
        }
    }
}
#endif 