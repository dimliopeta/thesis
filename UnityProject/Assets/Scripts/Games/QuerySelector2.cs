using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class QuerySelector2 : MonoBehaviour
{
    [System.Serializable]
    public class SQLQuery
    {
        public string queryTitle;
        public string queryExplanation;           // γενική περιγραφή του join
        public string exampleExplanation;         // εξήγηση για το παράδειγμα της βιβλιοθήκης
        public Sprite queryImage;                 // εικόνα για το συγκεκριμένο JOIN
    }

    [System.Serializable]
    public class QueryButtonPair
    {
        public Button button;
        public SQLQuery query;
    }

    [Header("Query Button Bindings")]
    public List<QueryButtonPair> queryButtonPairs;

    [Header("Query Description Display")]
    public TMP_Text queryTitleText;
    public TMP_Text queryExplanationText;
    public TMP_Text exampleExplanationText;       // ✨ ΝΕΟ: επεξήγηση παραδείγματος
    public float fadeDuration = 0.5f;

    [Header("Image Display")]
    public Image displayImage;

    private Coroutine fadeCoroutine;
    private List<bool> buttonClickedFlags;
    private bool tutorialCompleteBroadcasted = false;

    private void Start()
    {
        Debug.Log("QuerySelector2 Start() called");

        for (int i = 0; i < queryButtonPairs.Count; i++)
        {
            int index = i;
            var pair = queryButtonPairs[index];
            if (pair.button != null)
            {
                pair.button.onClick.AddListener(() => SelectQuery(index));
                var navigation = pair.button.navigation;
                    navigation.mode = Navigation.Mode.None;
                    pair.button.navigation = navigation;
            }
            else
            {
                Debug.LogWarning($"Button at index {index} is not assigned.");
            }
        }

        buttonClickedFlags = new List<bool>(new bool[queryButtonPairs.Count]);

        if (queryTitleText != null)
            queryTitleText.canvasRenderer.SetAlpha(0f);

        if (queryExplanationText != null)
            queryExplanationText.canvasRenderer.SetAlpha(0f);

        if (exampleExplanationText != null)
            exampleExplanationText.canvasRenderer.SetAlpha(0f);
    }

    void SelectQuery(int index)
    {
        int level = Managers.Mission.curLevel;
        Debug.Log($"SelectQuery called for index: {index}");

        if (index >= 0 && index < queryButtonPairs.Count)
        {
            var query = queryButtonPairs[index].query;

            // Εμφάνιση κειμένων και εικόνας
            DisplayQueryText(query.queryTitle, query.queryExplanation, query.exampleExplanation);

            if (displayImage != null && query.queryImage != null)
            {
                displayImage.sprite = query.queryImage;
                Debug.Log("Changed display image.");
            }
        }

        if (!buttonClickedFlags[index])
        {
            buttonClickedFlags[index] = true;
            CheckIfTutorialComplete();
        }

        GameLogger.Log("SELECT_TUTORIAL_STEP", new TutorialStepData(
            tutorialName: "SelectQueryTutorial",
            stepIndex: index,
            queryTitle: queryButtonPairs[index].query.queryTitle
        ), level);
    }

    void DisplayQueryText(string title, string explanation, string example)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeInQueryText(title, explanation, example));
    }

    IEnumerator FadeInQueryText(string title, string explanation, string example)
    {
        if (queryTitleText != null)
        {
            queryTitleText.text = title;
            queryTitleText.canvasRenderer.SetAlpha(0f);
            queryTitleText.CrossFadeAlpha(1f, fadeDuration, false);
        }

        if (queryExplanationText != null)
        {
            queryExplanationText.text = explanation;
            queryExplanationText.canvasRenderer.SetAlpha(0f);
            queryExplanationText.CrossFadeAlpha(1f, fadeDuration, false);
        }

        if (exampleExplanationText != null)
        {
            exampleExplanationText.text = example;
            exampleExplanationText.canvasRenderer.SetAlpha(0f);
            exampleExplanationText.CrossFadeAlpha(1f, fadeDuration, false);
        }

        yield return new WaitForSeconds(fadeDuration);
    }

    void CheckIfTutorialComplete()
    {
        int level = Managers.Mission.curLevel;
        if (tutorialCompleteBroadcasted) return;

        foreach (var clicked in buttonClickedFlags)
        {
            if (!clicked) return;
        }

        Debug.Log("All queries clicked! Broadcasting SELECT_TUTORIAL_COMPLETE");
        Messenger.Broadcast(GameEvent.QUERY_SELECTOR_DONE);
        GameLogger.Log("SELECT_TUTORIAL_COMPLETED", new TutorialCompletedData("SelectQueryTutorial"), level);
        tutorialCompleteBroadcasted = true;
    }

}
