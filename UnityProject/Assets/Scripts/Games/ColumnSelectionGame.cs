using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ColumnSelectionGame : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public string questionText;
        public List<string> correctTableKeys;
    }

    [Header("UI References")]
    public TextMeshProUGUI questionText;
    public TextMeshProUGUI feedbackText;

    [Header("Question Data")]
    public List<Question> questions;

    private int currentQuestionIndex = 0;
    private bool isFinished = false;
    private HashSet<string> selectedTables = new HashSet<string>();
    private Dictionary<string, Button> tableButtons = new Dictionary<string, Button>();

    void Start()
    {
        RegisterAllTableButtons();
        ShowQuestion(0);
        feedbackText.text = "";

        questionText.gameObject.SetActive(false); 
        Messenger.AddListener(GameEvent.HOLOGRAM_4_COMPLETED, OnHologram4Completed); 
    }

    void OnDestroy()
    {
        Messenger.RemoveListener(GameEvent.HOLOGRAM_4_COMPLETED, OnHologram4Completed);
    }

    void OnHologram4Completed()
    {
        questionText.gameObject.SetActive(true); 
    }

    void RegisterAllTableButtons()
    {
        foreach (Button btn in GetComponentsInChildren<Button>())
        {
            string key = btn.gameObject.name;
            tableButtons[key] = btn;
            btn.onClick.AddListener(() => OnTableClicked(key));
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None;
            btn.navigation = nav;
        }
    }

    void ShowQuestion(int index)
    {
        currentQuestionIndex = index;
        selectedTables.Clear();
        questionText.text = questions[index].questionText;
        feedbackText.text = "";

        // Reset all button visuals
        foreach (var btn in tableButtons.Values)
        {
            Color c = btn.image.color;
            c.a = 0f;
            btn.image.color = c;
        }
    }

    void OnTableClicked(string key)
    {
        if (isFinished)
        return;
        
        int level = Managers.Mission.curLevel;
        var question = questions[currentQuestionIndex];
        if (question.correctTableKeys.Contains(key))
        {
            if (selectedTables.Add(key))
            {
                tableButtons[key].image.color = new Color(0f, 1f, 0f, 0.5f);
            }
        }
        else
        {
            StartCoroutine(FlashRed(tableButtons[key]));
            GameLogger.Log("TableSelectionGame",
                new ColumnSelectionLogData(
                    question.questionText,
                    selectedTables.ToList(),
                    "wrong"), level);
            SetFeedback("Oops! That's not part of the right answer.", Color.red);
            ScoreManager.Instance.AddScore("TableSelectionGame", -20);
            Messenger<int>.Broadcast(GameEvent.SCORE_FEEDBACK, -20);
        }

        if (selectedTables.SetEquals(question.correctTableKeys))
        {
            GameLogger.Log("TableSelectionGame",
                new ColumnSelectionLogData(
                    question.questionText,
                    selectedTables.ToList(),
                    "correct"), level);
            ScoreManager.Instance.AddScore("TableSelectionGame", 50);
            Messenger<int>.Broadcast(GameEvent.SCORE_FEEDBACK, 50);
            StartCoroutine(LoadNextAfterDelay());
        }
    }

    IEnumerator FlashRed(Button btn)
    {
        Color original = btn.image.color;
        btn.image.color = new Color(1f, 0f, 0f, 0.5f);
        yield return new WaitForSeconds(0.2f);
        btn.image.color = original;
    }

    IEnumerator LoadNextAfterDelay()
    {
        SetFeedback("Correct! Loading next question in 3...", Color.green);
        yield return new WaitForSeconds(1f);
        SetFeedback("Correct! Loading next question in 2...", Color.green);
        yield return new WaitForSeconds(1f);
        SetFeedback("Correct! Loading next question in 1...", Color.green);
        yield return new WaitForSeconds(1f);

        if (currentQuestionIndex + 1 < questions.Count)
        {
            ShowQuestion(currentQuestionIndex + 1);
        }
        else
        {
            Messenger.Broadcast(GameEvent.COLUMN_SELECTION_DONE);
            questionText.text = "Well done! You finished this challenge.";
            SetFeedback("Great work! You connected all the right tables.", Color.green);
            isFinished = true;
        }
    }

    void SetFeedback(string message, Color color)
    {
        feedbackText.text = message;
        Material newMat = new Material(feedbackText.fontMaterial);
        feedbackText.fontMaterial = newMat;
        feedbackText.fontMaterial.SetColor(TMPro.ShaderUtilities.ID_FaceColor, color);
    }
}
