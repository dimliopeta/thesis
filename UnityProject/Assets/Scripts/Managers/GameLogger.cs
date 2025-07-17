using UnityEngine;

public class GameLogger : MonoBehaviour
{
    public static void Log<T>(string type, T dataObject, int level)
    {
        Debug.Log($"GameLogger: Logging event of type {type}");
        Debug.Log($"🔍 GameLogger sees Progress = {Managers.Progress}, status = {(Managers.Progress != null ? Managers.Progress.status.ToString() : "null")}");
        var progressManager = Managers.Progress;
        if (progressManager == null || progressManager.status != ManagerStatus.Started)
        {
            Debug.LogWarning("GameLogger: ProgressManager not initialized.");
            return;
        }

        progressManager.ReportProgress(type, dataObject, level);
    }
}
