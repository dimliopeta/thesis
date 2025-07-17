using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int TotalScore { get; private set; }
    public int SessionScore { get; private set; }

    private Dictionary<string, int> gameScores = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void AddScore(string game, int delta)
    {
        SessionScore += delta;
        TotalScore += delta;

        if (!gameScores.ContainsKey(game))
            gameScores[game] = 0;

        gameScores[game] += delta;
        var data = new ScoreData(game, delta, gameScores[game], SessionScore, TotalScore);
        GameLogger.Log("SCORE_UPDATED", data, Managers.Mission.curLevel);
    }

    public int GetGameScore(string game)
    {
        return gameScores.ContainsKey(game) ? gameScores[game] : 0;
    }
}
