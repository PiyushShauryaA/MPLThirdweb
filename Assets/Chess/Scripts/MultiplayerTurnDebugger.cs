using UnityEngine;
using Photon.Pun;
using TMPro;

namespace CanvasChess
{
    /// <summary>
    /// Debug script to display multiplayer turn information
    /// </summary>
    public class MultiplayerTurnDebugger : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI debugText = null!;
        
        [Header("Settings")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private float updateInterval = 1f;
        
        private GameManager gameManager = null;
        private float lastUpdateTime = 0f;
        
        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
        }
        
        private void Update()
        {
            if (!showDebugInfo || debugText == null) return;
            
            if (Time.time - lastUpdateTime >= updateInterval)
            {
                UpdateDebugText();
                lastUpdateTime = Time.time;
            }
        }
        
        private void UpdateDebugText()
        {
            if (!PhotonNetwork.InRoom)
            {
                debugText.text = "Not in a room";
                return;
            }
            
            string debugInfo = $"Room: {PhotonNetwork.CurrentRoom.Name}\n";
            debugInfo += $"Players: {PhotonNetwork.CurrentRoom.PlayerCount}/2\n";
            debugInfo += $"IsMasterClient: {PhotonNetwork.IsMasterClient}\n";
            
            if (gameManager != null)
            {
                debugInfo += $"Current Turn: {gameManager.CurrentTurn}\n";
                debugInfo += $"Turn Number: {gameManager.TurnNumber}\n";
            }
            
            
            debugText.text = debugInfo;
        }
        
        [ContextMenu("Toggle Debug Info")]
        public void ToggleDebugInfo()
        {
            showDebugInfo = !showDebugInfo;
            if (debugText != null)
            {
                debugText.gameObject.SetActive(showDebugInfo);
            }
        }
    }
} 