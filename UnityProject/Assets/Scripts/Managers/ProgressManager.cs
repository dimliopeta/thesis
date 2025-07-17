using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class ProgressManager : MonoBehaviour, IGameManager
{
    public ManagerStatus status { get; private set; } = ManagerStatus.Shutdown;

    private string progressUrl = "http://localhost:3000/progress";
    private string devLoginUrl = "http://localhost:3000/login/dev";

    private string token;
    private string sessionId;
    private int userId;

        void Awake()
    {
        if (Managers.Progress == null)
        {
            Debug.LogWarning("🛠 ProgressManager self-registering (late load)");
            Managers.SetProgressManager(this);
        }
    }


    public void Startup()
    {
        Debug.Log("ProgressManager starting...");
        status = ManagerStatus.Initializing; // Προσωρινό στάδιο για debugging
        StartCoroutine(Initialize());
    }

    private IEnumerator Initialize()
    {
        if (PlayerPrefs.HasKey("token") && PlayerPrefs.HasKey("sessionId") && PlayerPrefs.HasKey("userId"))
        {
            token = PlayerPrefs.GetString("token");
            sessionId = PlayerPrefs.GetString("sessionId");
            userId = PlayerPrefs.GetInt("userId");
            Debug.Log($"Loaded saved credentials: sessionId: {sessionId}, userId: {userId}");
        }
        else
        {
            Debug.Log("No credentials found. Attempting dev login...");
            yield return StartCoroutine(DevLogin());
        }

        status = ManagerStatus.Started;
        Debug.Log("ProgressManager is now ready (status = Started)");


    }

    private IEnumerator DevLogin()
    {
        UnityWebRequest request = UnityWebRequest.Get(devLoginUrl);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Dev login failed: " + request.error);
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;
        DevLoginResponse data = JsonUtility.FromJson<DevLoginResponse>(jsonResponse);

        token = data.token;
        sessionId = data.sessionId;
        userId = data.userId;

        PlayerPrefs.SetString("token", token);
        PlayerPrefs.SetString("sessionId", sessionId);
        PlayerPrefs.SetInt("userId", userId);
        PlayerPrefs.Save();

        Debug.Log("Dev login successful. Credentials stored.");
    }

    public void UpdateAuth(string token, string sessionId, int userId)
    {
        Debug.Log("ProgressManager: Updating authentication credentials.");
        this.token = token;
        this.sessionId = sessionId;
        this.userId = userId;

        PlayerPrefs.SetString("token", token);
        PlayerPrefs.SetString("sessionId", sessionId);
        PlayerPrefs.SetInt("userId", userId);
        PlayerPrefs.Save();

        Debug.Log("ProgressManager: Auth credentials updated and saved.");
    }

    public void ReportProgress(string type, object dataObject, int level)
    {
        if (this.status != ManagerStatus.Started)
        {
            Debug.LogWarning("ProgressManager not ready yet.");
            return;
        }

        Debug.Log($"Reporting progress: Type: {type}, SessionId: {sessionId}");
        StartCoroutine(SendProgress(type, dataObject, level));
    }

    private IEnumerator SendProgress(string type, object dataObject, int level)
    {
        ProgressData data = new ProgressData
        {
            type = type,
            sessionId = sessionId,
            userId = userId,
            data = JsonUtility.ToJson(dataObject),
            level = level
        };

        string json = JsonUtility.ToJson(data);
        Debug.Log($"Sending JSON -> {json}");

        UnityWebRequest request = new UnityWebRequest(progressUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {token}");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("Progress submission failed: " + request.error);
        }
        else
        {
            Debug.Log("Progress submitted successfully.");
        }
    }

    [System.Serializable]
    private class DevLoginResponse
    {
        public string token;
        public string role;
        public int userId;
        public string sessionId;
    }

    [System.Serializable]
    private class ProgressData
    {
        public string type;
        public string sessionId;
        public int userId;
        public string data;
        public int level;
    }
}
