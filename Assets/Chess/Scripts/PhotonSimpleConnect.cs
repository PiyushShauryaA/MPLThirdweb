using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using ExitGames.Client.Photon; // For Hashtable

public class PhotonSimpleConnect : MonoBehaviourPunCallbacks
{
    public Button connectButton;
    public TMP_Text statusText;
    public TMP_InputField usernameInput;
    
    [Header("Timer Selection")]
    public Toggle rapidToggle;
    public Toggle blitzToggle;
    public Toggle bulletToggle;
    public TMP_Text selectedTimerText;
    public ToggleGroup timerToggleGroup;

    private string desiredUserId;
    private bool isConnecting = false;
    private bool autoSwitchEnabled = true;
    private bool isSearchingForRooms = false; // Add back search mode flag
    private float roomWaitTimer = 0f;
    private float searchTimer = 0f; // Add back search timer
    private const float ROOM_WAIT_TIME = 10f; // 10 seconds to wait for players
    private const float SEARCH_TIMEOUT = 15f; // 15 seconds to search for rooms

    void Start()
    {
        Debug.Log("[PhotonSimpleConnect] Start() called");
        statusText.text = "Enter username and press Connect.";
        connectButton.onClick.AddListener(OnConnectClicked);
        
        SetupTimerSelection();
        
        // Enable automatic scene synchronization
        PhotonNetwork.AutomaticallySyncScene = true;
        
        // Reset connection state when returning to menu
        isConnecting = false;
        isSearchingForRooms = false;
        roomWaitTimer = 0f;
        searchTimer = 0f;
        
        // If we're already connected, disconnect first to start fresh
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("[PhotonSimpleConnect] Already connected, disconnecting to start fresh");
            PhotonNetwork.Disconnect();
        }
        
        Debug.Log("[PhotonSimpleConnect] AutomaticallySyncScene enabled");
    }

    private void OnConnectClicked()
    {
        Debug.Log("[PhotonSimpleConnect] OnConnectClicked() called");
        string username = usernameInput.text.Trim();
        if (string.IsNullOrEmpty(username))
        {
            statusText.text = "Please enter a username.";
            return;
        }
        desiredUserId = username;
        connectButton.interactable = false;
        statusText.text = "Connecting...";
        isConnecting = true;

        // Timer-based matchmaking: join random room with same timer type
        string timerType = PlayerSettingsChess.SelectedTimerType.ToString();
        var expectedProperties = new Hashtable { { "timerType", timerType } };
        PhotonNetwork.NickName = username;
        PhotonNetwork.ConnectUsingSettings();
        // Wait for OnConnectedToMaster to call JoinRandomRoom
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("[PhotonSimpleConnect] Connected to Photon Master");
        
        // Check if we're already in a room and leave it first
        if (PhotonNetwork.InRoom)
        {
            Debug.Log("[PhotonSimpleConnect] Already in a room, leaving first");
            PhotonNetwork.LeaveRoom();
            return;
        }
        // Only join random room if on MasterServer
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMaster)
        {
            string timerType = PlayerSettingsChess.SelectedTimerType.ToString();
            var expectedProperties = new Hashtable { { "timerType", timerType } };
            PhotonNetwork.JoinRandomRoom(expectedProperties, 0);
            statusText.text = $"Looking for a {timerType} game...";
        }
        else
        {
            Debug.Log("[PhotonSimpleConnect] Not on MasterServer, waiting to retry JoinRandomRoom");
            Invoke(nameof(OnConnectedToMaster), 0.5f);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        // No room found, create one with the selected timer type
        string timerType = PlayerSettingsChess.SelectedTimerType.ToString();
        var roomOptions = new RoomOptions
        {
            MaxPlayers = 2,
            CustomRoomProperties = new Hashtable { { "timerType", timerType } },
            CustomRoomPropertiesForLobby = new string[] { "timerType" }
        };
        PhotonNetwork.CreateRoom(null, roomOptions);
        statusText.text = $"Creating a new {timerType} game...";
    }

    public override void OnJoinedRoom()
    {
        string timerType = PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("timerType")
            ? PhotonNetwork.CurrentRoom.CustomProperties["timerType"].ToString()
            : "Unknown";
        Debug.Log($"[PhotonSimpleConnect] Joined room with timer: {timerType}");
        statusText.text = $"Joined {timerType} game. Waiting for opponent...";
        // ... (rest of your logic, e.g., wait for 2 players then load game scene)
    }
    


    void Update()
    {
        // Check if UI elements are still valid (scene might have changed)
        if (statusText == null || connectButton == null)
        {
            Debug.LogWarning("[PhotonSimpleConnect] UI elements are null, likely scene changed. Disabling Update.");
            enabled = false;
            return;
        }

        // Handle the wait timer for when we're in a room alone
        if (isConnecting && PhotonNetwork.InRoom && PhotonNetwork.CurrentRoom.PlayerCount == 1)
        {
            roomWaitTimer += Time.deltaTime;
            float remainingTime = ROOM_WAIT_TIME - roomWaitTimer;
            
            if (remainingTime > 0)
            {
                statusText.text = $"Waiting for players... ({remainingTime:F1}s remaining)";
            }
            else if (!isSearchingForRooms)
            {
                // Time's up, start searching for rooms
                Debug.Log($"[PhotonSimpleConnect] Wait timer expired ({ROOM_WAIT_TIME}s). Starting search mode. Room: {PhotonNetwork.CurrentRoom.Name}");
                StartSearchMode();
            }
        }

        // Handle search timeout
        if (isSearchingForRooms)
        {
            searchTimer += Time.deltaTime;
            if (searchTimer >= SEARCH_TIMEOUT)
            {
                // Search timeout, go to AI game
                Debug.Log($"[PhotonSimpleConnect] Search timeout reached ({SEARCH_TIMEOUT}s). Starting AI game");
                if (statusText != null)
                {
                    statusText.text = "Search timeout. Starting AI game...";
                }
                StartAIGame();
            }
            else if (searchTimer % 5f < Time.deltaTime) // Log every 5 seconds
            {
                Debug.Log($"[PhotonSimpleConnect] Search in progress... {searchTimer:F1}s elapsed, {SEARCH_TIMEOUT - searchTimer:F1}s remaining");
            }
        }
    }

    private void StartAIGame()
    {
        Debug.Log("[PhotonSimpleConnect] StartAIGame() called");
        // Disable automatic scene sync before disconnecting to prevent SetProperties errors
        PhotonNetwork.AutomaticallySyncScene = false;
        // Disconnect from Photon and load AI game scene
        PhotonNetwork.Disconnect();
        Debug.Log("[PhotonSimpleConnect] Disconnected from Photon, loading AI game scene");
        // Only use PhotonNetwork.LoadLevel if connected and in a room
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LoadLevel("ChessGameVS_AI");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("ChessGameVS_AI");
        }
    }

    private void StartSearchMode()
    {
        Debug.Log("[PhotonSimpleConnect] StartSearchMode() called");
        isSearchingForRooms = true;
        searchTimer = 0f; // Reset search timer
        if (statusText != null)
        {
            statusText.text = "No players found. Searching for available rooms...";
        }
        
        // Leave current room and search for available rooms
        Debug.Log($"[PhotonSimpleConnect] Leaving room: {PhotonNetwork.CurrentRoom.Name}");
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Debug.Log($"[PhotonSimpleConnect] OnLeftRoom() called. isSearchingForRooms: {isSearchingForRooms}, IsConnectedAndReady: {PhotonNetwork.IsConnectedAndReady}");
        
        // If we're still trying to connect or search for rooms, continue with timer-based matchmaking
        if (isConnecting || isSearchingForRooms)
        {
            Debug.Log("[PhotonSimpleConnect] Scheduling room search after leaving room");
            // Use the existing SearchForAvailableRooms method which has proper state checking
            Invoke(nameof(SearchForAvailableRooms), 1f);
        }
    }

    private void SearchForAvailableRooms()
    {
        Debug.Log($"[PhotonSimpleConnect] SearchForAvailableRooms() called. isSearchingForRooms: {isSearchingForRooms}, IsConnectedAndReady: {PhotonNetwork.IsConnectedAndReady}, State: {PhotonNetwork.NetworkClientState}, SearchTimer: {searchTimer:F1}");
        if (!isSearchingForRooms)
            return;
        // Only try to join random room if on MasterServer
        if (PhotonNetwork.NetworkClientState == ClientState.ConnectedToMaster)
        {
            Debug.Log("[PhotonSimpleConnect] Client is on MasterServer, attempting to join random room with timer filter");
            string timerType = PlayerSettingsChess.SelectedTimerType.ToString();
            if (statusText != null)
            {
                statusText.text = $"Searching for {timerType} games...";
            }
            var expectedProperties = new Hashtable { { "timerType", timerType } };
            PhotonNetwork.JoinRandomRoom(expectedProperties, 0);
        }
        else
        {
            Debug.Log("[PhotonSimpleConnect] Not on MasterServer, waiting to reconnect...");
            // If not on MasterServer, try again in a moment
            Invoke(nameof(SearchForAvailableRooms), 0.5f);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"[PhotonSimpleConnect] OnJoinRoomFailed() called. ReturnCode: {returnCode}, Message: {message}");
        
        // Handle different failure reasons
        if (returnCode == 32760) // Room is full
        {
            Debug.Log("[PhotonSimpleConnect] Room is full - this shouldn't happen with JoinRandomOrCreateRoom, but going to AI game");
            if (statusText != null)
            {
                statusText.text = "All rooms are full. Starting AI game...";
            }
        }
        else
        {
            Debug.Log("[PhotonSimpleConnect] JoinRandomOrCreateRoom failed for other reason, redirecting to AI game");
            if (statusText != null)
            {
                statusText.text = "Failed to join or create room. Starting AI game...";
            }
        }
        
        Invoke(nameof(StartAIGame), 2f);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log($"[PhotonSimpleConnect] OnPlayerEnteredRoom() called. New player: {newPlayer.NickName}, Total players: {PhotonNetwork.CurrentRoom.PlayerCount}");
        UpdatePlayerList();
        
        // Check if we have 2 players and auto-switch is enabled
        if (PhotonNetwork.CurrentRoom.PlayerCount >= 2 && autoSwitchEnabled)
        {
            Debug.Log("[PhotonSimpleConnect] Second player joined, starting game in 2 seconds");
            if (statusText != null)
            {
                statusText.text = "2 players connected! Starting game...";
            }
            Invoke(nameof(SwitchToGameScene), 2f); // Give 2 seconds for players to see the message
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log($"[PhotonSimpleConnect] OnPlayerLeftRoom() called. Player left: {otherPlayer.NickName}, Remaining players: {PhotonNetwork.CurrentRoom.PlayerCount}");
        UpdatePlayerList();
        
        // If we're back to 1 player, update status
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2)
        {
            Debug.Log("[PhotonSimpleConnect] Back to 1 player, waiting for another player");
            if (statusText != null)
            {
                statusText.text = "Player left. Waiting for another player...";
            }
        }
    }

    void UpdatePlayerList()
    {
        if (statusText == null)
        {
            return;
        }
        
        if (!PhotonNetwork.InRoom)
        {
            statusText.text = "Not in a room.";
            return;
        }
        var players = PhotonNetwork.PlayerList;
        string playerNames = $"Room: {PhotonNetwork.CurrentRoom.Name}\nPlayers in room:";
        foreach (var p in players)
        {
            playerNames += $"\n- {p.NickName}";
        }
        
        if (players.Length >= 2)
        {
            playerNames += "\n\n✅ Ready to start!";
        }
        else
        {
            playerNames += "\n\n⏳ Waiting for more players...";
        }
        
        statusText.text = playerNames;
    }

    private void SwitchToGameScene()
    {
        Debug.Log($"[PhotonSimpleConnect] SwitchToGameScene() called. IsMasterClient: {PhotonNetwork.IsMasterClient}, PlayerCount: {PhotonNetwork.CurrentRoom.PlayerCount}");
        
        // Get the PhotonView component
        PhotonView pv = GetComponent<PhotonView>();
        Debug.Log($"[PhotonSimpleConnect] PhotonView found: {pv != null}");

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[PhotonSimpleConnect] Master client initiating scene switch");
            if (pv != null)
            {
                // Master client initiates the scene switch for all clients via RPC
                pv.RPC("SwitchToGameSceneRPC", RpcTarget.All);
                Debug.Log("[PhotonSimpleConnect] RPC sent to all clients");
            }
            else
            {
                // Fallback: use direct scene load for all clients
                Debug.LogWarning("[PhotonSimpleConnect] PhotonView not found, using fallback scene switch");
                SwitchToGameSceneFallback();
            }
        }
        else
        {
            // Non-master clients will follow when master loads the level
            Debug.Log("[PhotonSimpleConnect] Non-master client waiting for scene switch");
            statusText.text = "Master client is starting the game...";
        }
    }

    [PunRPC]
    private void SwitchToGameSceneRPC()
    {
        // All clients switch to the game scene
        Debug.Log($"[PhotonSimpleConnect] SwitchToGameSceneRPC() called. IsMasterClient: {PhotonNetwork.IsMasterClient}");
        // Only use PhotonNetwork.LoadLevel if connected and in a room
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LoadLevel("ChessGameMulti");
        }
        else
        {
            // Fallback to direct scene load
            UnityEngine.SceneManagement.SceneManager.LoadScene("ChessGameMulti");
        }
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log($"[PhotonSimpleConnect] OnDisconnected() called. Cause: {cause}");
        
        // Reset all connection states
        isConnecting = false;
        isSearchingForRooms = false;
        roomWaitTimer = 0f;
        searchTimer = 0f;
        
        if (statusText != null)
        {
            statusText.text = "Enter username and press Connect.";
        }
        if (connectButton != null)
        {
            connectButton.interactable = true;
        }
        
        Debug.Log("[PhotonSimpleConnect] Connection state reset");
    }

    // Public method to manually switch to game scene (for testing)
    public void ManualSwitchToGame()
    {
        Debug.Log("[PhotonSimpleConnect] Manual switch to game scene");
        if (PhotonNetwork.IsConnectedAndReady && PhotonNetwork.InRoom)
        {
            PhotonNetwork.LoadLevel("ChessGameMulti");
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("ChessGameMulti");
        }
    }

    // Public method to toggle auto-switch
    public void ToggleAutoSwitch()
    {
        autoSwitchEnabled = !autoSwitchEnabled;
        Debug.Log($"[PhotonSimpleConnect] Auto-switch toggled: {(autoSwitchEnabled ? "ON" : "OFF")}");
        statusText.text = $"Auto-switch: {(autoSwitchEnabled ? "ON" : "OFF")}";
    }

    // Fallback method that doesn't use RPCs
    private void SwitchToGameSceneFallback()
    {
        Debug.Log("[PhotonSimpleConnect] Using fallback scene switch method");
        UnityEngine.SceneManagement.SceneManager.LoadScene("ChessGameMulti");
    }
    
    // Public method to reset connection state (call this when returning to menu)
    public void ResetConnectionState()
    {
        Debug.Log("[PhotonSimpleConnect] ResetConnectionState() called");
        isConnecting = false;
        isSearchingForRooms = false;
        roomWaitTimer = 0f;
        searchTimer = 0f;
        
        if (statusText != null)
        {
            statusText.text = "Enter username and press Connect.";
        }
        if (connectButton != null)
        {
            connectButton.interactable = true;
        }
    }
    
    #region Timer Selection
    /// <summary>
    /// Sets up the timer selection UI and event listeners
    /// </summary>
    private void SetupTimerSelection()
    {
        // Assign toggles to group if group is set
        if (timerToggleGroup != null)
        {
            if (rapidToggle != null) rapidToggle.group = timerToggleGroup;
            if (blitzToggle != null) blitzToggle.group = timerToggleGroup;
            if (bulletToggle != null) bulletToggle.group = timerToggleGroup;
        }

        // Remove all listeners first to avoid duplicate calls
        if (rapidToggle != null) rapidToggle.onValueChanged.RemoveAllListeners();
        if (blitzToggle != null) blitzToggle.onValueChanged.RemoveAllListeners();
        if (bulletToggle != null) bulletToggle.onValueChanged.RemoveAllListeners();

        if (rapidToggle != null)
            rapidToggle.onValueChanged.AddListener((isOn) => { if (isOn) OnTimerToggleChanged(PlayerSettingsChess.TimerType.Rapid); });
        if (blitzToggle != null)
            blitzToggle.onValueChanged.AddListener((isOn) => { if (isOn) OnTimerToggleChanged(PlayerSettingsChess.TimerType.Blitz); });
        if (bulletToggle != null)
            bulletToggle.onValueChanged.AddListener((isOn) => { if (isOn) OnTimerToggleChanged(PlayerSettingsChess.TimerType.Bullet); });

        // Set initial state based on PlayerSettings
        if (rapidToggle != null) rapidToggle.isOn = PlayerSettingsChess.SelectedTimerType == PlayerSettingsChess.TimerType.Rapid;
        if (blitzToggle != null) blitzToggle.isOn = PlayerSettingsChess.SelectedTimerType == PlayerSettingsChess.TimerType.Blitz;
        if (bulletToggle != null) bulletToggle.isOn = PlayerSettingsChess.SelectedTimerType == PlayerSettingsChess.TimerType.Bullet;

        UpdateTimerDisplay();
    }
    
    /// <summary>
    /// Sets the initial toggle state based on PlayerSettings
    /// </summary>
    private void SetInitialToggleState()
    {
        // Turn off all toggles first
        if (rapidToggle != null) rapidToggle.isOn = false;
        if (blitzToggle != null) blitzToggle.isOn = false;
        if (bulletToggle != null) bulletToggle.isOn = false;
        
        // Set the correct one based on PlayerSettings
        switch (PlayerSettingsChess.SelectedTimerType)
        {
            case PlayerSettingsChess.TimerType.Rapid:
                if (rapidToggle != null) rapidToggle.isOn = true;
                break;
            case PlayerSettingsChess.TimerType.Blitz:
                if (blitzToggle != null) blitzToggle.isOn = true;
                break;
            case PlayerSettingsChess.TimerType.Bullet:
                if (bulletToggle != null) bulletToggle.isOn = true;
                break;
        }
    }
    
    /// <summary>
    /// Called when a timer toggle is changed
    /// </summary>
    /// <param name="timerType">The timer type that was selected</param>
    /// <param name="isOn">Whether the toggle is now on</param>
    private void OnTimerToggleChanged(PlayerSettingsChess.TimerType timerType)
    {
        PlayerSettingsChess.SelectedTimerType = timerType;
        UpdateTimerDisplay();
        Debug.Log($"[PhotonSimpleConnect] Timer type changed to: {timerType}");
    }
    
    /// <summary>
    /// Updates the timer display to show the currently selected timer
    /// </summary>
    private void UpdateTimerDisplay()
    {
        if (selectedTimerText != null)
        {
            string timerText = PlayerSettingsChess.SelectedTimerType switch
            {
                PlayerSettingsChess.TimerType.Rapid => "Rapid (10 minutes)",
                PlayerSettingsChess.TimerType.Blitz => "Blitz (5 minutes)",
                PlayerSettingsChess.TimerType.Bullet => "Bullet (3 minutes)",
                _ => "Rapid (10 minutes)"
            };
            
            selectedTimerText.text = $"Selected: {timerText}";
        }
    }
    #endregion
}
