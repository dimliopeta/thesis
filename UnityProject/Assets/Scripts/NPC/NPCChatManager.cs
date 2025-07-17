using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NPCChatManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField playerInputField;
    public Button sendButton;

    [Header("Chat UI")]
    public Transform chatContent; // Content του ScrollView
    public GameObject chatMessagePrefab;

    [Header("Colors")]
    public Color playerColor = new Color(0.4f, 0.6f, 1f, 0.6f); // pastel blue με χαμηλό opacity
    public Color npcColor = new Color(0.6f, 0.9f, 0.6f, 0.6f);  // pastel green

     
    

    [Header("Logger Info")]
    private float lastMessageTime = -100f;
    private float chatCooldown = 30f; 


    private string npcUrl = "http://localhost:3000/npc-message";

    private void Start()
    {
        sendButton.onClick.AddListener(SendMessageToNPC);
    }

    public void SendMessageToNPC()
    {
        string message = playerInputField.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        AddMessageToUI($"You: {message}", TextAlignmentOptions.Right, playerColor);
        StartCoroutine(SendRequest(message));
        playerInputField.text = "";
    }
    public void SendMessageToNPCWithGameContext(string gameName, string gameContextJson)
    {
        string message = playerInputField.text.Trim();
        if (string.IsNullOrEmpty(message)) return;

        AddMessageToUI($"You: {message}", TextAlignmentOptions.Right, playerColor);
        StartCoroutine(SendRequest(message, gameName, gameContextJson));
        playerInputField.text = "";
    }  
    private IEnumerator SendRequest(string message, string game = null, string gameContextJson = null)
    {
        string sessionId = PlayerPrefs.GetString("sessionId");
        int userId = PlayerPrefs.GetInt("userId");
        bool isNewConversation = Time.time - lastMessageTime > chatCooldown;
        lastMessageTime = Time.time;
    
        string checkpoint = Managers.GameState.CurrentCheckpoint;
        bool isInGame = !string.IsNullOrEmpty(checkpoint);

        if (string.IsNullOrEmpty(game) && !string.IsNullOrEmpty(ChallengeContextHolder.GameName))
        {
            game = ChallengeContextHolder.GameName;
            gameContextJson = ChallengeContextHolder.GameContextJson;
        }
    
        NPCRequest requestData = new NPCRequest
        {
            playerMessage = message,
            sessionId = sessionId,
            userId = userId,
            checkpoint = checkpoint,
            cheatsRemaining = Managers.GameState.CheatCount,
    
            game = isInGame ? game : null,
            gameContextJson = isInGame ? gameContextJson : null
        };
    
        string json = JsonUtility.ToJson(requestData);
    
        UnityWebRequest request = new UnityWebRequest(npcUrl, "POST");
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
    
        yield return request.SendWebRequest();
    
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Error contacting NPC: " + request.error);
            AddMessageToUI("Error talking to NPC.", TextAlignmentOptions.Left, Color.red);
            Debug.LogError("Request Body: " + json);
        }
        else
        {
            var response = JsonUtility.FromJson<NPCResponse>(request.downloadHandler.text);
            AddMessageToUI($"NPC: {response.reply}", TextAlignmentOptions.Left, npcColor);
            if (isInGame && response.cheatsRemaining >= 0)
            {
                Managers.GameState.UpdateCheatCount(response.cheatsRemaining);
            }
            if (isNewConversation)
            {
                var logData = new NpcChatStartData(response.messageId);
                GameLogger.Log("NPC_CHAT_START", logData, Managers.Mission.curLevel);
            }
        }
    }
    
    private void AddMessageToUI(string message, TextAlignmentOptions alignment, Color color)
    {
        GameObject newMessage = Instantiate(chatMessagePrefab, chatContent);
        TMP_Text textComponent = newMessage.GetComponent<TMP_Text>();
        textComponent.text = message;
        textComponent.alignment = alignment;

        var mat = new Material(textComponent.fontMaterial);
        mat.shader = Shader.Find("TextMeshPro/Distance Field Overlay");
        mat.SetColor(ShaderUtilities.ID_FaceColor, color);
        mat.SetFloat(ShaderUtilities.ID_FaceDilate, 0);
        mat.SetFloat(ShaderUtilities.ID_OutlineSoftness, 0);
        mat.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
        textComponent.fontMaterial = mat;

    }

   [System.Serializable]
    private class NPCRequest
    {
        public string playerMessage;
        public string sessionId;
        public int userId;
        public string checkpoint;
        public int cheatsRemaining;

        // Optional – only when in mini-game
        public string game; // e.g. "query_result_challenge"
        public string gameContextJson;
    }



    [System.Serializable]
private class NPCResponse
    {
        public string reply;
        public int messageId;
        public int cheatsRemaining;
    }


}
