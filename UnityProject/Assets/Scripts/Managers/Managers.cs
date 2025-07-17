using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(MissionManager))]

public class Managers : MonoBehaviour
{
    public static MissionManager Mission { get; private set; }
    public static GameStateManager GameState { get; private set; }
    public static ProgressManager Progress { get; private set; }


    private List<IGameManager> startSequence;

    void Awake()
    {
        if (FindObjectsOfType<Managers>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }
    
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("🧠 Managers Awake called in scene: " + gameObject.scene.name);
    
        Mission = GetComponent<MissionManager>();
        GameState = GetComponent<GameStateManager>();
        Progress = GetComponent<ProgressManager>();
    
        startSequence = new List<IGameManager> { Mission, GameState, Progress };
    
        StartCoroutine(StartupManagers());
    }
    
    public static void SetProgressManager(ProgressManager pm)
    {
        Progress = pm;
    }

     private IEnumerator StartupManagers()
    {
        foreach (IGameManager manager in startSequence)
        {
            manager.Startup();
        }
        yield return null;
        int numModules = startSequence.Count;
        int numReady = 0;
        while (numReady < numModules)
        {
            int lastReady = numReady;
            numReady = 0;

            foreach (IGameManager manager in startSequence)
            {
                if (manager.status == ManagerStatus.Started)
                {
                    numReady++;
                }
            }
            if (numReady > lastReady)
            {
                Debug.Log($"Progress: {numReady}/{numModules}");
                Messenger<int, int>.Broadcast(StartupEvent.MANAGERS_PROGRESS, numReady, numModules);
            }
            yield return null;
        }
        Debug.Log("All managers started up");
        Messenger.Broadcast(StartupEvent.MANAGERS_STARTED, MessengerMode.DONT_REQUIRE_LISTENER);
    }
}
