using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;


public class LoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;

    [SerializeField] private Button loginButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private TextMeshProUGUI errorMessage;

    private string serverUrl = "http://localhost:3000/login/game"; // Άλλαξε αν ανεβάσεις τον server online

    void Start()
    {
        loginButton.onClick.AddListener(OnLoginButtonClicked);
        quitButton.onClick.AddListener(OnQuitButtonClicked);
    }

    public void OnLoginButtonClicked()
    {
        StartCoroutine(Login());
    }

    private IEnumerator Login()
    {
        string username = usernameField.text;
        string password = passwordField.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            errorMessage.text = "Συμπληρώστε όλα τα πεδία!";
            yield break;
        }

        string json = $"{{\"username\":\"{username}\", \"password\":\"{password}\"}}";
        UnityWebRequest request = new UnityWebRequest(serverUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            errorMessage.text = "Σφάλμα σύνδεσης!";
        }
        else
        {
            string response = request.downloadHandler.text;
            LoginResponse loginResponse = JsonUtility.FromJson<LoginResponse>(response);

            if (loginResponse.role == "student")
            {
                PlayerPrefs.SetString("token", loginResponse.token);
                PlayerPrefs.SetString("sessionId", loginResponse.sessionId);
                PlayerPrefs.SetInt("userId", loginResponse.userId);
                PlayerPrefs.Save();
                SceneManager.LoadScene("StartUp");
            }

            else
            {
                errorMessage.text = "Μόνο φοιτητές επιτρέπονται!";
            }
        }

    }
    public void OnQuitButtonClicked()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
}

[System.Serializable]
public class LoginResponse
{
    public string token;
    public string role;
    public int userId;
    public string sessionId;
}