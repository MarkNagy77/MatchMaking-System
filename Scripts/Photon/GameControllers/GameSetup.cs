using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class GameSetup : MonoBehaviourPunCallbacks
{
    public static GameSetup GS;

    public Text scoreText;
    public Text endGame;

    private int score;
    public int scoreToWin;

    public string[] playerNames;
    public Text[] playersNamesText;
    public int[] playersScores;
    public int scoreTotal;
    public Text[] playersScoresText;
    public Transform[] scoreOrder;

    public override void OnEnable()
    {
        if (GameSetup.GS == null)
            GameSetup.GS = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        endGame.gameObject.SetActive(false);
    }

    public void OnScoreClick()
    {
        score++;
        scoreText.text = score.ToString();
        GameEventManager.gameEventManager.ScoreChanged();
        if(score == scoreToWin)
        {
            endGame.gameObject.SetActive(true);
            endGame.text = "you win!";
        }
    }

    public void OnDisconectClick()
    {
        GameEventManager.gameEventManager.Playerleft();
        PhotonNetwork.LeaveRoom();
        Destroy(PhotonNetworkRoom.photonNetworkRoom.gameObject);
    }


    // Update is called once per frame
    void Update()
    {
        ScoreBoardUpdate();
    }

    private void ScoreBoardUpdate()
    {
        int tempTotal = 0;

        for(int i = 0;i <playersScores.Length; i++)
        {
            tempTotal += playersScores[i];
        }

        if(tempTotal != scoreTotal)
        {
            OrderUpdate();
            scoreTotal = tempTotal;
            for(int i = 0; i < playersScores.Length; i++)
            {
                playersScoresText[i].text = playersScores[i].ToString();
            }
        }
    }

    private void OrderUpdate()
    {
        Transform[] order = scoreOrder;
        int[] scores = playersScores;
        int[] place = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
        for(int i = 0; i < scores.Length; i++)
        {
            for(int j = 0; j < scores.Length; j++)
            {
                if(scores[i] < scores[j])
                {
                    place[i]++;
                }
            }
        }
        for(int i = 0; i < order.Length; i++)
        {
            order[i].SetSiblingIndex(place[i]);
        }
    }
}
