using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonPlayerInfo : MonoBehaviourPunCallbacks
{
    public static PhotonPlayerInfo Instance;

    [Header("Player Usernames (Read Only)")]
    [SerializeField] private string hostUsername;
    [SerializeField] private string clientUsername;

    public string HostUsername => hostUsername;
    public string ClientUsername => clientUsername;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void OnJoinedRoom()
    {
        UpdateUsernames();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdateUsernames();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdateUsernames();
    }

    private void UpdateUsernames()
    {
        hostUsername = "";
        clientUsername = "";
        var players = PhotonNetwork.PlayerList;
        foreach (var p in players)
        {
            if (p.IsMasterClient)
                hostUsername = p.NickName;
            else
                clientUsername = p.NickName;
        }
    }
}
