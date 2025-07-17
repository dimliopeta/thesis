using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QueryImageQuiz : MonoBehaviour
{
    [System.Serializable]
    public class QueryImagePair
    {
        public Sprite resultImage;
        public string correctQuery;
        public List<string> options;
        public string explanation;
    }

    [SerializeField] private List<QueryImagePair> quizItems;
    [SerializeField] private Image resultDisplayImage;
    [SerializeField] private List<Button> queryButtons;
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private TextMeshProUGUI resultExplanationText; 

    private int currentIndex = 0;
    private bool isFinished = false;

    void Start()
    {
        ShowCurrentQuiz();
    }

    void ShowCurrentQuiz()
    {
        if (currentIndex >= quizItems.Count)
        {
            Debug.Log(" Όλο το quiz ολοκληρώθηκε.");
            isFinished = true;
            Messenger.Broadcast(GameEvent.QUERY_IMAGE_GAME_COMPLETED);
            return;
        }

        var item = quizItems[currentIndex];
        resultDisplayImage.sprite = item.resultImage;
        resultExplanationText.text = item.explanation; 

        
        var shuffled = new List<string>(item.options);
        shuffled.Sort((a, b) => Random.Range(-1, 2));

        for (int i = 0; i < queryButtons.Count; i++)
        {
            if (i < shuffled.Count)
            {
                string queryText = shuffled[i];
                queryButtons[i].gameObject.SetActive(true);
                queryButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = queryText;

                queryButtons[i].onClick.RemoveAllListeners();
                queryButtons[i].onClick.AddListener(() => OnQuerySelected(queryText, item.correctQuery));

                var nav = queryButtons[i].navigation;
                nav.mode = Navigation.Mode.None;
                queryButtons[i].navigation = nav;
            }
            else
            {
                queryButtons[i].gameObject.SetActive(false);
            }
        }

        feedbackText.text = "";
    }

        void OnQuerySelected(string selected, string correct)
    {
        if (isFinished)
        return;
        int level = Managers.Mission.curLevel;
        bool isCorrect = selected == correct;

        if (isCorrect)
        {
            feedbackText.text = " Right!";
            GameLogger.Log("QueryImageGame",
            new QueryImageLogData(selected, correct, "success", currentIndex),
            level);
            ScoreManager.Instance.AddScore("QueryImageGame", 50);
            Messenger<int>.Broadcast(GameEvent.SCORE_FEEDBACK, 50);
            StartCoroutine(NextQuizAfterDelay());
        }
        else
        {
            feedbackText.text = " Wrong.";
            GameLogger.Log("QueryImageGame",
            new QueryImageLogData(selected, correct, "fail", currentIndex),
            level);
            ScoreManager.Instance.AddScore("QueryImageGame", -20);
            Messenger<int>.Broadcast(GameEvent.SCORE_FEEDBACK, -20);
        }
    }

    IEnumerator NextQuizAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);
        currentIndex++;
        ShowCurrentQuiz();
    }
}
