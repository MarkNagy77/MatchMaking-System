using Photon.Pun;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PhotonPlayer : MonoBehaviourPun
{
    public GameObject myAvatar;

    public int myScore;
    private int playerNumber;
    private string myName;


    // Start is called before the first frame update
    void Start()
    {
        GameEventManager.gameEventManager.onScoreChanged += SendScore;
        GameEventManager.gameEventManager.onPlayerLeft += PlayerLeft;

        if (this.photonView.IsMine)
        {
            myName = PhotonNetwork.NickName;
            this.photonView.RPC("RPC_GetMeNumber", RpcTarget.MasterClient, myName);
            myScore = 0;
        }        
    }

    [PunRPC]
    void RPC_GetMeNumber(string newNameIn)
    {
        int j = 0;
        while (GameSetup.GS.playerNames[j] != "")
        {
            j++;
        }
        int newNumberInRoom = j;
        GameSetup.GS.playerNames[newNumberInRoom] = newNameIn;
        GameSetup.GS.playersScores[newNumberInRoom] = 0;
        this.photonView.RPC("RPC_NewPlayerNumber", RpcTarget.All, newNameIn, newNumberInRoom);
    }

    [PunRPC]
    void RPC_NewPlayerNumber(string myNameBack, int myNewPlayerNumber)
    {
        if (myName == myNameBack)
        {
            playerNumber = myNewPlayerNumber;
            this.photonView.RPC("RPC_SpawnHeads", RpcTarget.MasterClient, myName, playerNumber);
        }
    }

    [PunRPC]
    void RPC_SpawnHeads(string nameIn, int playerNumberIn)
    {    
        
        for (int i = 0; i < GameSetup.GS.playerNames.Length; ++i)
        {
            GameSetup.GS.playerNames[playerNumberIn] = nameIn;
            GameSetup.GS.playersScores[playerNumberIn] = 0;
            this.photonView.RPC("RPC_SyncHeads", RpcTarget.All, GameSetup.GS.playerNames[i], i, GameSetup.GS.playersScores[i]);
        }
    }

    [PunRPC]
    void RPC_SyncHeads(string nameIn, int playerNumberIn, int scoreIn)
    {
        GameSetup.GS.playerNames[playerNumberIn] = nameIn;       
        GameSetup.GS.playersNamesText[playerNumberIn].text = nameIn;
        GameSetup.GS.playersScores[playerNumberIn] = scoreIn;
        GameSetup.GS.playersScoresText[playerNumberIn].text = scoreIn.ToString();
    }



    public void SendScore()
    {
        if (this.photonView.IsMine)
        {
            bool win = false;
            myScore++;
            if (myScore == GameSetup.GS.scoreToWin)
                win = true;
            this.photonView.RPC("RPC_SendScore", RpcTarget.All, myScore, playerNumber, win);
        }
    }

    [PunRPC]
    void RPC_SendScore(int score, int playerNumberIn, bool winIn)
    {
        GameSetup.GS.playersScores[playerNumberIn] = score;
        if (winIn)
        {
            GameSetup.GS.endGame.gameObject.SetActive(true);
            GameSetup.GS.endGame.text = "You loose!";
        }
    }

    private void PlayerLeft()
    {
        if (this.photonView.IsMine)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                GameSetup.GS.playerNames[playerNumber] = "";
                GameSetup.GS.playersScores[playerNumber] = 0;
                for(int i = 0; i < GameSetup.GS.playerNames.Length; ++i)
                {
                    this.photonView.RPC("RPC_SyncHeads", RpcTarget.All, GameSetup.GS.playerNames[i], i, GameSetup.GS.playersScores[i]);
                }
            }
            else
            {
                this.photonView.RPC("RPC_DestroyPlayersData", RpcTarget.All, "", playerNumber);
            }
        }
    }

    [PunRPC]
    void RPC_DestroyPlayersData(string destroyName,int playerNumberIn)
    {
        GameSetup.GS.playerNames[playerNumberIn] = destroyName;
        GameSetup.GS.playersScores[playerNumberIn] = 0;
        GameSetup.GS.playersNamesText[playerNumberIn].text = GameSetup.GS.playerNames[playerNumberIn];
        GameSetup.GS.playersScoresText[playerNumberIn].text = GameSetup.GS.playersScores[playerNumberIn].ToString();
    }


    private void OnDestroy()
    {
        GameEventManager.gameEventManager.onScoreChanged -= SendScore;
        GameEventManager.gameEventManager.onPlayerLeft -= PlayerLeft;
        if (this.photonView.IsMine)
        {
            SceneManager.LoadScene(MultiplayerSettings.multiplayerSettings.menuScene);
        }
    }
}
