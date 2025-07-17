[System.Serializable]
public class ScoreData
{
    public string game;
    public int scoreDelta;
    public int newGameScore;
    public int newSessionScore;
    public int newTotalScore;

    public ScoreData(string game, int delta, int gameScore, int sessionScore, int totalScore)
    {
        this.game = game;
        this.scoreDelta = delta;
        this.newGameScore = gameScore;
        this.newSessionScore = sessionScore;
        this.newTotalScore = totalScore;
    }
}
