using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartupController : MonoBehaviour
{
    [SerializeField] Slider progressBar;

    void OnEnable()
    {
        Debug.Log("StartupController OnEnable");

        Messenger<int, int>.AddListener(StartupEvent.MANAGERS_PROGRESS, OnManagersProgress);
        Messenger.AddListener(StartupEvent.MANAGERS_STARTED, OnManagersStarted);
    }

    void OnDisable()
    {
        Messenger<int, int>.RemoveListener(StartupEvent.MANAGERS_PROGRESS, OnManagersProgress);
        Messenger.RemoveListener(StartupEvent.MANAGERS_STARTED, OnManagersStarted);
    }

    private void OnManagersProgress(int numReady, int numModules)
    {
        float progress = (float)numReady / numModules;
        progressBar.value = progress;
    }

    private void OnManagersStarted()
    {
        Debug.Log("✅ StartupController: All managers started — waiting for ProgressManager to finish login...");
    }

}
