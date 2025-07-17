using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class QuerySelector : MonoBehaviour
{
    [System.Serializable]
    public class SQLQuery
    {
        public string queryTitle;
        public string queryExplanation;
        public List<GameObject> cellsToHighlight;
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
    public float fadeDuration = 0.5f;

    [Header("Highlight Sprites")]
    public Sprite highlightSprite;
    public Sprite defaultSprite;

    private Coroutine fadeCoroutine;

    private List<bool> buttonClickedFlags;
    private bool tutorialCompleteBroadcasted = false;


    private void Start()
    {
        Debug.Log("QuerySelector Start() called");

        ResetAllHighlights();

        for (int i = 0; i < queryButtonPairs.Count; i++)
        {
            int index = i;
            var pair = queryButtonPairs[index];
            if (pair.button != null)
            {
                pair.button.onClick.AddListener(() => HighlightQuery(index));
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


        // Hide text fields initially
        if (queryTitleText != null)
            queryTitleText.canvasRenderer.SetAlpha(0f);

        if (queryExplanationText != null)
            queryExplanationText.canvasRenderer.SetAlpha(0f);

    }

    void HighlightQuery(int index)
    {
        int level = Managers.Mission.curLevel;
        Debug.Log($"HighlightQuery called for index: {index}");
        ResetAllHighlights();

        if (index >= 0 && index < queryButtonPairs.Count)
        {
            var query = queryButtonPairs[index].query;
            DisplayQueryText(query.queryTitle, query.queryExplanation);

            Debug.Log($"Valid query index: {index} - Highlighting {query.cellsToHighlight.Count} cells");

            foreach (var cell in query.cellsToHighlight)
            {
                if (cell == null)
                {
                    Debug.LogWarning("A cell in cellsToHighlight is null.");
                    continue;
                }

                var img = cell.GetComponentInChildren<Image>();
                if (img && highlightSprite != null)
                {
                    img.sprite = highlightSprite;
                    Debug.Log($"Highlighted: {cell.name}");
                }
                else
                {
                    Debug.LogWarning($"Missing Image component or highlightSprite on {cell.name}");
                }
            }
        }
        else
        {
            Debug.LogWarning("Invalid query index!");
        }

        // Mark this query as clicked
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

    void ResetAllHighlights()
    {
        Debug.Log("ResetAllHighlights called");

        foreach (var pair in queryButtonPairs)
        {
            foreach (var cell in pair.query.cellsToHighlight)
            {
                if (cell == null) continue;

                var img = cell.GetComponentInChildren<Image>();
                if (img && defaultSprite != null)
                {
                    img.sprite = defaultSprite;
                    Debug.Log($"Reset: {cell.name}");
                }
            }
        }
    }

    void DisplayQueryText(string title, string explanation)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeInQueryText(title, explanation));
    }

    IEnumerator FadeInQueryText(string title, string explanation)
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

        yield return new WaitForSeconds(fadeDuration);
    }

    void CheckIfTutorialComplete()
    {
        int level = Managers.Mission.curLevel;
        if (tutorialCompleteBroadcasted) return;

        foreach (var clicked in buttonClickedFlags)
        {
            if (!clicked) return; // not all clicked yet
        }

        // All buttons have been clicked
        Debug.Log("All queries clicked! Broadcasting SELECT_TUTORIAL_COMPLETE");
        BroadcastMessage("SELECT_TUTORIAL_COMPLETE", SendMessageOptions.DontRequireReceiver);
        GameLogger.Log("SELECT_TUTORIAL_COMPLETED", new TutorialCompletedData("SelectQueryTutorial"), level);
        tutorialCompleteBroadcasted = true;
    }
    
}
