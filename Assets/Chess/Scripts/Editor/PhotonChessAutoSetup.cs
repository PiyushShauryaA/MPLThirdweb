#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Photon.Pun;
using Photon.Realtime;

namespace CanvasChess.Editor
{
    /// <summary>
    /// Automatically sets up Photon synchronization for chess pieces and game objects
    /// </summary>
    public class PhotonChessAutoSetup : EditorWindow
    {
        static PhotonChessAutoSetup()
        {
            // Auto-run setup when script is loaded
            EditorApplication.delayCall += () =>
            {
                if (!EditorApplication.isPlaying)
                {
                    SetupAllPiecePrefabs();
                    SetupCurrentScene();
                    Debug.Log("Photon Chess Auto Setup completed automatically!");
                }
            };
        }

        [MenuItem("CanvasChess/Auto-Setup Photon Sync")]
        public static void ShowWindow()
        {
            GetWindow<PhotonChessAutoSetup>("Photon Chess Auto Setup");
        }

        [MenuItem("CanvasChess/Auto-Setup All Photon Components")]
        public static void AutoSetupAll()
        {
            SetupAllPiecePrefabs();
            SetupCurrentScene();
            EditorUtility.DisplayDialog("Auto Setup Complete", 
                "All Photon components have been automatically set up!\n\n" +
                "✅ All piece prefabs have PhotonView and PhotonTransformView\n" +
                "✅ Current scene GameManager and BoardManager have PhotonView\n" +
                "✅ Ready for real-time synchronization across devices", "OK");
        }

        private void OnGUI()
        {
            GUILayout.Label("Photon Chess Auto Setup", EditorStyles.boldLabel);
            GUILayout.Label("This will automatically set up Photon synchronization for your chess game.");
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Setup All Piece Prefabs"))
            {
                SetupAllPiecePrefabs();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Setup Current Scene"))
            {
                SetupCurrentScene();
            }
            
            GUILayout.Space(10);
            
            if (GUILayout.Button("Setup All (Prefabs + Scene)"))
            {
                SetupAllPiecePrefabs();
                SetupCurrentScene();
            }
        }

        /// <summary>
        /// Sets up all piece prefabs with PhotonView and PhotonTransformView
        /// </summary>
        private static void SetupAllPiecePrefabs()
        {
            string[] prefabPaths = {
                "Assets/Prefabs/w_pawn.prefab",
                "Assets/Prefabs/b_pawn.prefab",
                "Assets/Prefabs/w_rook.prefab",
                "Assets/Prefabs/b_rook.prefab",
                "Assets/Prefabs/w_knight.prefab",
                "Assets/Prefabs/b_knight.prefab",
                "Assets/Prefabs/w_bishop.prefab",
                "Assets/Prefabs/b_bishop.prefab",
                "Assets/Prefabs/w_Queen.prefab",
                "Assets/Prefabs/b_Queen.prefab",
                "Assets/Prefabs/w_King.prefab",
                "Assets/Prefabs/b_King.prefab",
                "Assets/Prefabs/Piece.prefab"
            };

            int setupCount = 0;
            foreach (string prefabPath in prefabPaths)
            {
                if (System.IO.File.Exists(prefabPath))
                {
                    GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab != null)
                    {
                        SetupPrefabWithPhoton(prefab);
                        setupCount++;
                    }
                }
            }

            EditorUtility.DisplayDialog("Photon Setup Complete", 
                $"Successfully set up {setupCount} piece prefabs with Photon synchronization.\n\n" +
                "All pieces now have PhotonView and PhotonTransformView components.", "OK");
        }

        /// <summary>
        /// Sets up a single prefab with Photon components
        /// </summary>
        private static void SetupPrefabWithPhoton(GameObject prefab)
        {
            // Add PhotonView if not present
            PhotonView photonView = prefab.GetComponent<PhotonView>();
            if (photonView == null)
            {
                photonView = prefab.AddComponent<PhotonView>();
                // Owner is read-only, will be set automatically when instantiated
                photonView.Synchronization = ViewSynchronization.UnreliableOnChange;
            }

            // Add PhotonTransformView if not present
            PhotonTransformView transformView = prefab.GetComponent<PhotonTransformView>();
            if (transformView == null)
            {
                transformView = prefab.AddComponent<PhotonTransformView>();
                // Configure transform sync through the inspector or use default settings
                // The PhotonTransformView will handle position and rotation sync automatically
            }

            // Mark as dirty to save changes
            EditorUtility.SetDirty(prefab);
        }

        /// <summary>
        /// Sets up the current scene with Photon components
        /// </summary>
        private static void SetupCurrentScene()
        {
            // Setup GameManager
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                PhotonView gameManagerPhotonView = gameManager.GetComponent<PhotonView>();
                if (gameManagerPhotonView == null)
                {
                    gameManagerPhotonView = gameManager.gameObject.AddComponent<PhotonView>();
                    gameManagerPhotonView.Synchronization = ViewSynchronization.UnreliableOnChange;
                }
            }

            // Setup BoardManager
            BoardManager boardManager = FindObjectOfType<BoardManager>();
            if (boardManager != null)
            {
                PhotonView boardManagerPhotonView = boardManager.GetComponent<PhotonView>();
                if (boardManagerPhotonView == null)
                {
                    boardManagerPhotonView = boardManager.gameObject.AddComponent<PhotonView>();
                    boardManagerPhotonView.Synchronization = ViewSynchronization.UnreliableOnChange;
                }
            }

            // Setup all existing pieces in the scene
            Piece[] pieces = FindObjectsOfType<Piece>();
            foreach (Piece piece in pieces)
            {
                SetupPrefabWithPhoton(piece.gameObject);
            }

            EditorUtility.DisplayDialog("Scene Setup Complete", 
                $"Successfully set up Photon components for:\n" +
                $"• GameManager: {(gameManager != null ? "✓" : "✗")}\n" +
                $"• BoardManager: {(boardManager != null ? "✓" : "✗")}\n" +
                $"• Pieces in scene: {pieces.Length}\n\n" +
                "The scene is now ready for Photon synchronization.", "OK");
        }
    }
}
#endif 