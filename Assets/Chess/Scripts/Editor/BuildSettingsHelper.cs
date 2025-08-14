#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;

namespace CanvasChess.Editor
{
    /// <summary>
    /// Helper utility to set up build settings for the chess game
    /// </summary>
    public class BuildSettingsHelper : EditorWindow
    {
        [MenuItem("CanvasChess/Setup Build Settings")]
        public static void ShowWindow()
        {
            GetWindow<BuildSettingsHelper>("Build Settings Helper");
        }

        private void OnGUI()
        {
            GUILayout.Label("Build Settings Setup", EditorStyles.boldLabel);
            
            GUILayout.Space(10);
            GUILayout.Label("This will configure the build settings with the correct scene order:");
            GUILayout.Label("1. MainMenu (Startup Scene)");
            GUILayout.Label("2. ChessGame");
            GUILayout.Label("3. ChessGameVS_AI");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Setup Build Settings"))
            {
                SetupBuildSettings();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Set MainMenu as Startup Scene"))
            {
                SetMainMenuAsStartup();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Open MainMenu Scene"))
            {
                OpenMainMenuScene();
            }
            
            if (GUILayout.Button("Open ChessGame Scene"))
            {
                OpenChessGameScene();
            }
            
            if (GUILayout.Button("Open ChessGame VS AI Scene"))
            {
                OpenChessAIScene();
            }
        }

        /// <summary>
        /// Sets up the build settings with the correct scene order
        /// </summary>
        private static void SetupBuildSettings()
        {
            // Get all scenes in the project
            string[] scenePaths = {
                "Assets/Scenes/MainMenu.unity",
                "Assets/Scenes/ChessGame.unity",
                "Assets/Scenes/ChessGameVS_AI.unity"
            };

            // Check if scenes exist
            foreach (string scenePath in scenePaths)
            {
                if (!File.Exists(scenePath))
                {
                    EditorUtility.DisplayDialog("Build Settings Setup", 
                        $"Scene not found: {scenePath}\n\nPlease create the MainMenu scene first using:\nCanvasChess > Auto-Build Main Menu Scene", 
                        "OK");
                    return;
                }
            }

            // Clear existing scenes
            EditorBuildSettings.scenes = new EditorBuildSettingsScene[0];

            // Add scenes to build settings
            var buildScenes = new EditorBuildSettingsScene[scenePaths.Length];
            for (int i = 0; i < scenePaths.Length; i++)
            {
                buildScenes[i] = new EditorBuildSettingsScene(scenePaths[i], true);
            }

            EditorBuildSettings.scenes = buildScenes;

            EditorUtility.DisplayDialog("Build Settings Setup", 
                "Build settings configured successfully!\n\n" +
                "Scene order:\n" +
                "1. MainMenu (Index 0)\n" +
                "2. ChessGame (Index 1)\n" +
                "3. ChessGameVS_AI (Index 2)\n\n" +
                "You can now build your game.", "OK");
        }

        /// <summary>
        /// Sets MainMenu as the startup scene
        /// </summary>
        private static void SetMainMenuAsStartup()
        {
            string mainMenuPath = "Assets/Scenes/MainMenu.unity";
            
            if (!File.Exists(mainMenuPath))
            {
                EditorUtility.DisplayDialog("Startup Scene Setup", 
                    "MainMenu scene not found!\n\nPlease create it first using:\nCanvasChess > Auto-Build Main Menu Scene", 
                    "OK");
                return;
            }

            EditorSceneManager.OpenScene(mainMenuPath);
            EditorUtility.DisplayDialog("Startup Scene Setup", 
                "MainMenu scene opened and set as current scene.\n\n" +
                "This will be the startup scene when you build the game.", "OK");
        }

        /// <summary>
        /// Opens the MainMenu scene
        /// </summary>
        private static void OpenMainMenuScene()
        {
            string mainMenuPath = "Assets/Scenes/MainMenu.unity";
            
            if (!File.Exists(mainMenuPath))
            {
                EditorUtility.DisplayDialog("Open Scene", 
                    "MainMenu scene not found!\n\nPlease create it first using:\nCanvasChess > Auto-Build Main Menu Scene", 
                    "OK");
                return;
            }

            EditorSceneManager.OpenScene(mainMenuPath);
        }

        /// <summary>
        /// Opens the ChessGame scene
        /// </summary>
        private static void OpenChessGameScene()
        {
            string chessGamePath = "Assets/Scenes/ChessGame.unity";
            
            if (!File.Exists(chessGamePath))
            {
                EditorUtility.DisplayDialog("Open Scene", 
                    "ChessGame scene not found!\n\nPlease create it first using:\nCanvasChess > Auto-Build Chess Scene", 
                    "OK");
                return;
            }

            EditorSceneManager.OpenScene(chessGamePath);
        }

        /// <summary>
        /// Opens the ChessGame VS AI scene
        /// </summary>
        private static void OpenChessAIScene()
        {
            string chessAIPath = "Assets/Scenes/ChessGameVS_AI.unity";
            
            if (!File.Exists(chessAIPath))
            {
                EditorUtility.DisplayDialog("Open Scene", 
                    "ChessGame VS AI scene not found!\n\nPlease create it first using:\nCanvasChess > Auto-Build Chess VS AI Scene", 
                    "OK");
                return;
            }

            EditorSceneManager.OpenScene(chessAIPath);
        }
    }
}
#endif 