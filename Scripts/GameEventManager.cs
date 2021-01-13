using System;
using UnityEngine;

public class GameEventManager : MonoBehaviour
{
    public static GameEventManager gameEventManager;
    public event Action onScoreChanged, onPlayerLeft;

    private void Awake()
    {
        if (gameEventManager == null)
            gameEventManager = this;
        else
        {
            if(gameEventManager != this)
            {
                Destroy(gameEventManager.gameObject);
                gameEventManager = this;
            }
        }
        DontDestroyOnLoad(this.gameObject);
    }

    public void ScoreChanged()
    {
        onScoreChanged?.Invoke();
    }

    public void Playerleft()
    {
        onPlayerLeft?.Invoke();
    }
}
