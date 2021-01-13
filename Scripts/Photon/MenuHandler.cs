using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviourPunCallbacks
{
    public static MenuHandler menu;

    public GameObject menuGO;
    public GameObject deathMatchLobby;
    public GameObject battleRoyalLobby;
    public Button DMButton;
    public Button RBButton;
    public GameObject offlineText;
    public string myName;
    public Text changeName;


    private void Awake()
    {
        menu = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
        DMButton.interactable = false;
        RBButton.interactable = false;
        offlineText.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Player has connected to the Photon master server");
        PhotonNetwork.AutomaticallySyncScene = true;
        DMButton.interactable = true;
        RBButton.interactable = true;
        offlineText.SetActive(false);
    }

    public void OnDeathMatchButtonClick()
    {
        MultiplayerSettings.multiplayerSettings.delayStart = false;
        menuGO.SetActive(false);
        deathMatchLobby.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnBattleRoyalButtonClick()
    {
        MultiplayerSettings.multiplayerSettings.delayStart = true;
        menuGO.SetActive(false);
        battleRoyalLobby.SetActive(true);
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("Tried to join a random game but failed. There must be no open games avaible");
        CreateRoom();
    }

    void CreateRoom()
    {
        Debug.Log("Trying to create a new room");
        int randomRoomName = Random.Range(0, 1000);
        RoomOptions roomOps = new RoomOptions() { IsVisible = true, IsOpen = true, MaxPlayers = (byte)MultiplayerSettings.multiplayerSettings.maxPlayers};
        PhotonNetwork.CreateRoom("Room" + randomRoomName, roomOps);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("Tried to create a new room but failed, there must already be a room with the same name");
        CreateRoom();
    }

    public void OnCancelClicked()
    {
        deathMatchLobby.SetActive(false);
        menuGO.SetActive(true);
        PhotonNetwork.LeaveRoom();
    }

    public void SetMyName(string nameIn)
    {
        myName = changeName.text;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
