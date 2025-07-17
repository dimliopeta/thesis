using UnityEngine;

public class GameStateManager : MonoBehaviour, IGameManager
{
    public ManagerStatus status { get; private set; } = ManagerStatus.Shutdown;

    public bool IsPaused { get; private set; } = false;
    public bool IsChatOpen { get; set; } = false;

    public string CurrentCheckpoint { get; private set; }

    public int CheatCount { get; private set; }
    public int CheatLimit = 3;

    public void Startup()
    {
        Debug.Log("GameStateManager starting...");
        status = ManagerStatus.Started;
    }

    public void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
    }

    public void SetCheckpoint(string checkpoint)
    {
        CurrentCheckpoint = checkpoint;
        Debug.Log($"Checkpoint set to: {checkpoint}");
    }

    public void ClearCheckpoint()
    {
        CurrentCheckpoint = null;
        Debug.Log("Checkpoint cleared – no active game.");
    }

    public bool CanCheat()
    {
        return CheatCount < CheatLimit;
    }

    public void UseCheat()
    {
        CheatCount++;
        Debug.Log($"Cheat used locally. Remaining: {CheatLimit - CheatCount}");
    }

    public void UpdateCheatCount(int remainingFromServer)
    {
        CheatCount = CheatLimit - remainingFromServer;
        Debug.Log($"[Server Sync] Cheat count updated. Server says remaining: {remainingFromServer}, Local count: {CheatCount}");
    }

    public void ResetCheats()
    {
        CheatCount = 0;
    }
}
