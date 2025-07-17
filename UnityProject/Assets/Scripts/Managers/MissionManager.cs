using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionManager : MonoBehaviour, IGameManager
{
    public ManagerStatus status { get; private set; }
    
    public int curLevel { get; private set; }
    public int maxLevel { get; private set; }

    public void Startup()
    {
        Debug.Log("Mission manager starting...");
        StartCoroutine(Initialize());
    }

        void OnEnable()
    {
        Messenger.AddListener(GameEvent.LEVEL_COMPLETE, OnLevelComplete);
    }

    void OnDisable()
    {
        Messenger.RemoveListener(GameEvent.LEVEL_COMPLETE, OnLevelComplete);
    }

    private void OnLevelComplete()
    {
        GoToNext(); 
    }


    private IEnumerator Initialize()
    {
        curLevel = 1;
        maxLevel = 2;

        yield return null;

        status = ManagerStatus.Started;
        
        yield return 3f;

        Managers.Mission.StartGame();
    }

    
        public void StartGame()
    {
        Debug.Log($"[StartGame] curLevel = {curLevel}");
        string sceneName = $"Level{curLevel}";
        Debug.Log($"StartGame: Loading {sceneName}");
        SceneManager.LoadScene(sceneName);
    }



    public void GoToNext()
    {
        if (curLevel < maxLevel)
        {
            int previousLevel = curLevel;
            curLevel++;
            string nextSceneName = $"Level{curLevel}";

            // Στείλε μήνυμα με το congrats
            string msg = $"Congrats! Loading {nextSceneName}";

            GameLogger.Log("LEVEL_ADVANCE", new LevelAdvanceData(previousLevel, curLevel), curLevel);


            Messenger<string, bool>.Broadcast(GameEvent.SHOW_MESSAGE, msg, false);

            // Καθυστέρησε τη μετάβαση 3 δευτερόλεπτα
            StartCoroutine(LoadLevelAfterDelay(nextSceneName, 3f));
        }
        else
        {
            string msg = $"Congrats! You finished the game!";
            Messenger<string, bool>.Broadcast(GameEvent.SHOW_MESSAGE, msg, false);

            // Καθυστέρηση πριν το logout
            StartCoroutine(LogoutAndReturnToLogin(3f));
        }
    }

    private IEnumerator LoadLevelAfterDelay(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
    private IEnumerator LogoutAndReturnToLogin(float delay)
    {
        yield return new WaitForSeconds(delay);

        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.DeleteKey("sessionId");
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.Save();

        SceneManager.LoadScene("LoginScene");
    }



    public void RestartCurrent()
    {
        string name = $"Level{curLevel}";
        Debug.Log($"Reloading {name}");
        SceneManager.LoadScene(name);
    }


}
