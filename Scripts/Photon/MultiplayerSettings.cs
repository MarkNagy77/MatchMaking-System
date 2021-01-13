using UnityEngine;

public class MultiplayerSettings : MonoBehaviour
{
    public static MultiplayerSettings multiplayerSettings;

    public bool delayStart;
    public int maxPlayers;
    public int minPlayersToStart;
    public float maxWaintingTime;
    public float maxConectingTime;
    public float minPlayersToCreate;

    public int menuScene;
    public int multiplayerScene;

    private void Awake()
    {
        if (multiplayerSettings == null)
            multiplayerSettings = this;
        else
        {
            if (multiplayerSettings != this)
                Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
