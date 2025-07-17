using System.Collections;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class UIController : MonoBehaviour
{
    [SerializeField] private Button logOutButton;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private GameObject messageToPlayerPanel;
    [SerializeField] private Sprite warningSprite;
    [SerializeField] private Sprite successSprite;

    [SerializeField] private TextMeshProUGUI pauseHintText;

    [SerializeField] private GameObject interactablePanel;

    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private Toggle interactableToggle;
    [SerializeField] private GameObject floatingScorePrefab;
    [SerializeField] private Transform floatingScoreParent;
    [SerializeField] private TextMeshProUGUI totalScoreText;


    private bool interactableEnabled = true;

    private Image panelImage;


    private Coroutine messageCoroutine;

    void Start()
    {
        logOutButton.onClick.AddListener(Logout);
        interactableToggle.onValueChanged.AddListener(OnInteractableToggleChanged);
        Messenger<string, bool>.AddListener(GameEvent.SHOW_MESSAGE, ShowMessage);
        Messenger.AddListener(GameEvent.SHOW_CHAT, new Action(ShowChat));
        Messenger.AddListener(GameEvent.CLOSE_CHAT, new Action(CloseChat));
        Messenger<bool>.AddListener(GameEvent.INTERACTABLE, ShowInteract);
        Messenger<int>.AddListener(GameEvent.SCORE_FEEDBACK, ShowScoreFeedback);
        panelImage = messageToPlayerPanel.GetComponent<Image>();
        messageToPlayerPanel.gameObject.SetActive(false);
        interactablePanel.gameObject.SetActive(false);
        pauseHintText.gameObject.SetActive(false);
        totalScoreText.text = $"Score: {ScoreManager.Instance.TotalScore}";

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Μόνο αν δεν είμαστε σε chat επιτρέπουμε το pause toggle
            if (!Managers.GameState.IsChatOpen)
            {
                TogglePause();
            }
            else
            {
                // Αν είμαστε σε chat, το Esc κλείνει το chat
                CloseChat();
            }
        }
        bool showPauseHint = !Managers.GameState.IsPaused && !Managers.GameState.IsChatOpen;
        pauseHintText.gameObject.SetActive(showPauseHint);
    }


    void OnDestroy()
    {
        Messenger<string, bool>.RemoveListener(GameEvent.SHOW_MESSAGE, ShowMessage);
        Messenger.RemoveListener(GameEvent.SHOW_CHAT, new Action(ShowChat));
        Messenger.RemoveListener(GameEvent.CLOSE_CHAT, new Action(CloseChat));
        Messenger<bool>.RemoveListener(GameEvent.INTERACTABLE, ShowInteract);
        Messenger<int>.RemoveListener(GameEvent.SCORE_FEEDBACK, ShowScoreFeedback);
    }


    public void Logout()
    {
        PlayerPrefs.DeleteKey("token");
        PlayerPrefs.DeleteKey("sessionId");
        PlayerPrefs.DeleteKey("userId");
        PlayerPrefs.Save();
        
        SceneManager.LoadScene("LoginScene");
    }

    public void ShowChat()
    {
        chatPanel.gameObject.SetActive(true);
        ShowInteract(false);
        Managers.GameState.IsChatOpen = true;
    }

    public void CloseChat()
    {
        if (!Managers.GameState.IsChatOpen) return; // ήδη κλειστό

        chatPanel.gameObject.SetActive(false);
        ShowInteract(false);
        Managers.GameState.IsChatOpen = false;
    }

    public void ShowMessage(string message, bool warning)
    {
        if (messageCoroutine != null)
            StopCoroutine(messageCoroutine);

        messageCoroutine = StartCoroutine(ShowMessageRoutine(message, warning, 3f));
    }

    private IEnumerator ShowMessageRoutine(string message, bool warning, float duration)
    {
        // Χρώμα για το panel (με 0.5 opacity)
        Color panelBaseColor = warning ? new Color(1f, 0.4f, 0.4f, 0.5f) : new Color(0.4f, 1f, 0.4f, 0.5f);
        panelImage.color = panelBaseColor;

        // Χρώμα για το text (πλήρες opacity)
        Color textBaseColor = messageText.color;
        messageText.color = new Color(textBaseColor.r, textBaseColor.g, textBaseColor.b, 1f);

        messageText.text = message;

        // Υπολογισμός μεγέθους
        Vector2 preferredSize = messageText.GetPreferredValues(message);
        float paddingX = 40f;
        float paddingY = 20f;

        RectTransform panelRect = messageToPlayerPanel.GetComponent<RectTransform>();
        panelRect.sizeDelta = new Vector2(preferredSize.x + paddingX, preferredSize.y + paddingY);

        messageToPlayerPanel.SetActive(true);

        yield return new WaitForSeconds(duration);

        // Fading out
        float fadeDuration = 0.3f;
        float panelStartAlpha = panelImage.color.a; // 0.5f
        float textStartAlpha = 1f;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            float normalizedTime = t / fadeDuration;

            float panelAlpha = Mathf.Lerp(panelStartAlpha, 0f, normalizedTime);
            float textAlpha = Mathf.Lerp(textStartAlpha, 0f, normalizedTime * 1.5f); // ταχύτερο fade για το text

            panelImage.color = new Color(panelBaseColor.r, panelBaseColor.g, panelBaseColor.b, panelAlpha);
            messageText.color = new Color(textBaseColor.r, textBaseColor.g, textBaseColor.b, Mathf.Clamp01(textAlpha));

            yield return null;
        }

        // Εξασφάλιση πλήρους εξαφάνισης
        panelImage.color = new Color(panelBaseColor.r, panelBaseColor.g, panelBaseColor.b, 0f);
        messageText.color = new Color(textBaseColor.r, textBaseColor.g, textBaseColor.b, 0f);

        messageToPlayerPanel.SetActive(false);
    }
    private void OnInteractableToggleChanged(bool value)
    {
        interactableEnabled = value;

        if (!value)
            interactablePanel.SetActive(false);
    }

    public void ShowInteract(bool interact)
    {

        if (!interactableEnabled)
        {
            interactablePanel.SetActive(false);
            return;
        }

        interactablePanel.SetActive(interact);
    }


    private void TogglePause()
    {
        bool paused = !Managers.GameState.IsPaused;
        Managers.GameState.SetPaused(paused);
        pauseMenuPanel.SetActive(paused);

        if (paused)
            interactableToggle.isOn = interactableEnabled;

        ShowInteract(false);
    }
    private void ShowScoreFeedback(int value)
    {
        totalScoreText.text = $"Score: {ScoreManager.Instance.TotalScore}";
        Vector3 centerScreen = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        ShowFloatingScore(centerScreen, value); 
    }
    public void ShowFloatingScore(Vector3 screenPosition, int scoreValue)
    {
        GameObject floatingScore = Instantiate(floatingScorePrefab, floatingScoreParent);
        TextMeshProUGUI text = floatingScore.GetComponent<TextMeshProUGUI>();

        string prefix = scoreValue >= 0 ? "+" : "-";
        text.text = $"{prefix}{Mathf.Abs(scoreValue)}";


        Material newMat = new Material(text.fontMaterial);
        text.fontMaterial = newMat;
        
        Color targetColor = scoreValue >= 0 ? new Color(0.2f, 1f, 0.2f, 1f) : new Color(1f, 0.2f, 0.2f, 1f);
        text.fontMaterial.SetColor(TMPro.ShaderUtilities.ID_FaceColor, targetColor);
        
        floatingScore.transform.position = screenPosition;

        StartCoroutine(AnimateFloatingScore(floatingScore, 3f));
    }
    
        private IEnumerator AnimateFloatingScore(GameObject scoreObject, float duration)
    {
        RectTransform rect = scoreObject.GetComponent<RectTransform>();
        TextMeshProUGUI text = scoreObject.GetComponent<TextMeshProUGUI>();

        Vector3 startPos = rect.position;
        Vector3 endPos = startPos + new Vector3(0f, 80f, 0f);

        Color startColor = text.color;
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            rect.position = Vector3.Lerp(startPos, endPos, t);
            text.color = Color.Lerp(startColor, endColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(scoreObject);
    }



}
