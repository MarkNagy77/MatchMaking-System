using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PhotonNetworkRoom : MonoBehaviourPunCallbacks, IInRoomCallbacks
{
    public static PhotonNetworkRoom photonNetworkRoom;
    private PhotonView PV;

    public bool isGameLoaded;
    public int currentScene;

    public Text playersInRoomText;
    public Text remainingTimeText;

    Player[] photonPlayers;
    public int playersInRoom;
    public int playerInGame;

    private bool readyToStart;
    private bool countingConnection;
    private float waitingTime;
    private float connectionTime;

    private void Awake()
    {
        if(photonNetworkRoom == null)
        {
            photonNetworkRoom = this;
        }
        else
        {
            if(photonNetworkRoom != this)
            {
                Destroy(photonNetworkRoom.gameObject);
                photonNetworkRoom = this;
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnEnable()
    {
        base.OnEnable();
        PhotonNetwork.AddCallbackTarget(this);
        SceneManager.sceneLoaded += OnSceneFinishedLoading;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        PhotonNetwork.RemoveCallbackTarget(this);
        SceneManager.sceneLoaded -= OnSceneFinishedLoading;
    }

    void OnSceneFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        currentScene = scene.buildIndex;
        if (currentScene == MultiplayerSettings.multiplayerSettings.multiplayerScene)
        {
            isGameLoaded = true;
            if(MenuHandler.menu.myName == "")
            {
                int i = Random.Range(0, 999);
                MenuHandler.menu.myName = "Player" + i;
            }
                PhotonNetwork.NickName = MenuHandler.menu.myName;
            if (MultiplayerSettings.multiplayerSettings.delayStart)
            {
                PV.RPC("RPC_LoadedGameScene", RpcTarget.MasterClient);
            }
            else
            {
                photonPlayers = PhotonNetwork.PlayerList;
                playersInRoom = photonPlayers.Length;
                
                RPC_CreatePlayer();
            }
        }
    }

    [PunRPC]
    private void RPC_LoadedGameScene()
    {
        playerInGame++;
        if (PhotonNetwork.IsMasterClient)
        {
            countingConnection = true;
        }
        if(playerInGame == MultiplayerSettings.multiplayerSettings.minPlayersToCreate)
        {
            PV.RPC("RPC_CreatePlayer", RpcTarget.All);
            countingConnection = false;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        PV = GetComponent<PhotonView>();
        readyToStart = false;
        countingConnection = false;
        waitingTime = 0;
        connectionTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (MultiplayerSettings.multiplayerSettings.delayStart)
        {
            if (!isGameLoaded)
            {
                if (readyToStart)
                {
                    StartGame();
                }
                else
                {
                    waitingTime += Time.deltaTime;
                }
                int i = (int)waitingTime;
                remainingTimeText.text = i.ToString();
                if(waitingTime >= MultiplayerSettings.multiplayerSettings.maxWaintingTime || playersInRoom >= MultiplayerSettings.multiplayerSettings.minPlayersToStart)
                {
                    StartGame();
                }
            }
            if (countingConnection)
            {
                connectionTime += Time.deltaTime; 
                if(connectionTime >= MultiplayerSettings.multiplayerSettings.maxConectingTime)
                {
                    countingConnection = false;
                    connectionTime = 0;
                    PV.RPC("RPC_CreatePlayer", RpcTarget.All);
                }
            }
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("We are now in a room");
        if (PhotonNetwork.IsMasterClient)
        {
            photonPlayers = PhotonNetwork.PlayerList;
            playersInRoom = photonPlayers.Length;
            if (MenuHandler.menu.myName == "")
            {
                int i = Random.Range(0, 999);
                MenuHandler.menu.myName = "Player" + i;
            }
            PhotonNetwork.NickName = MenuHandler.menu.myName;
        }
        if (MultiplayerSettings.multiplayerSettings.delayStart)
        {
            playersInRoomText.text = playersInRoom.ToString() + "/" + MultiplayerSettings.multiplayerSettings.maxPlayers.ToString();
            if(playersInRoom == MultiplayerSettings.multiplayerSettings.maxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            
        }
        else
        {
            StartGame();
        }
    }


    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log("A new player " + newPlayer.NickName + " has joined the room");
        photonPlayers = PhotonNetwork.PlayerList;
        playersInRoom++;
        if (MultiplayerSettings.multiplayerSettings.delayStart)
        {
            Debug.Log("Displayer players in room out of max players possible (" + playersInRoom + ":" + MultiplayerSettings.multiplayerSettings.maxPlayers + ")");
            if (playersInRoom == MultiplayerSettings.multiplayerSettings.maxPlayers)
            {
                readyToStart = true;
                if (!PhotonNetwork.IsMasterClient)
                    PhotonNetwork.CurrentRoom.IsOpen = false;
            }
            PV.RPC("RPC_SyncData", RpcTarget.All, playersInRoom, waitingTime, readyToStart);
        }
    }

    [PunRPC]
    void RPC_SyncData(int numberOfPlayers, float waitingTimeIn, bool newReadyToStart)
    {
        playersInRoomText.text = numberOfPlayers.ToString() + "/" + MultiplayerSettings.multiplayerSettings.maxPlayers.ToString();
        playersInRoom = numberOfPlayers;
        waitingTime = waitingTimeIn;
        readyToStart = newReadyToStart;
    }

    private void StartGame()
    {
        isGameLoaded = true;
        waitingTime = 0;
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (MultiplayerSettings.multiplayerSettings.delayStart)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
        }
        PhotonNetwork.LoadLevel(MultiplayerSettings.multiplayerSettings.multiplayerScene);
    }

    [PunRPC]
    private void RPC_CreatePlayer()
    {
        waitingTime = 0;
        MultiplayerSettings.multiplayerSettings.delayStart = false;
        PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "Player"), transform.position, Quaternion.identity, 0);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);
        playersInRoom--;
        Debug.Log("player " + otherPlayer.NickName + " has left the room");
    }
}
