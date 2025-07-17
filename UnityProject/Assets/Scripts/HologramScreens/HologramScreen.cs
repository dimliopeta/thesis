using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class HologramScreen : MonoBehaviour
{
    [SerializeField] private HologramScreenData hologramScreenData;
    [SerializeField] private TextMeshProUGUI slideText;
    [SerializeField] private Image slideImage;

    [Header("Quiz")]
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private Button[] answerButtons;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Navigation")]
    [SerializeField] private bool allowBackNavigation = false;
    [SerializeField] private Button backButton;

    [Header("Events & Messages")]
    [SerializeField] private string completedEvent = "TUTORIAL_COMPLETED";
    [SerializeField] [TextArea] private string lockedSlideMessage = "You must complete the current action before proceeding!";

    private int currentSlideIndex = 0;
    private int currentQuizIndex = 0;
    private QuizData currentQuiz;

    private Dictionary<int, System.Action> slideListeners = new Dictionary<int, System.Action>();
    private HashSet<int> unlockedSlides = new HashSet<int>();
    private static HashSet<string> completedTutorials = new HashSet<string>();

    void Start()
    {
        ShowSlide(currentSlideIndex);

        var currentSlide = hologramScreenData.slides[currentSlideIndex];

        if (string.IsNullOrEmpty(currentSlide.requiredEventName))
        {
            unlockedSlides.Add(currentSlideIndex);
        }
        else
        {
            Messenger.AddListener(currentSlide.requiredEventName, () => UnlockSlide(currentSlideIndex));
        }

        if (backButton != null)
        {
            Debug.Log("✅ Back Button found, adding listener");
            backButton.onClick.AddListener(() =>
            {
                Debug.Log("✅ Back Button clicked");
                PreviousSlide();
            });
        }
        else
        {
            Debug.Log("❌ Back Button reference is null");
        }
    }

    public void Interact()
    {
        if (unlockedSlides.Contains(currentSlideIndex) ||
            string.IsNullOrEmpty(hologramScreenData.slides[currentSlideIndex].requiredEventName))
        {
            NextSlide();
        }
        else
        {
            Debug.Log("You must complete the current action before proceeding!");
            Messenger<string, bool>.Broadcast(GameEvent.SHOW_MESSAGE, lockedSlideMessage, true);
        }
    }

    void ShowSlide(int index)
    {
        if (index < hologramScreenData.slides.Count)
        {
            SlideData currentSlide = hologramScreenData.slides[index];
            if (slideText != null)
                slideText.text = currentSlide.slideText;
            if (slideImage != null)
                slideImage.sprite = currentSlide.slideImage;

            if (slideListeners.ContainsKey(index))
            {
                Messenger.RemoveListener(currentSlide.requiredEventName, slideListeners[index]);
                slideListeners.Remove(index);
            }

            if (!string.IsNullOrEmpty(currentSlide.requiredEventName))
            {
                System.Action listener = () => UnlockSlide(index);
                slideListeners[index] = listener;
                Messenger.AddListener(currentSlide.requiredEventName, listener);
            }

            ToggleQuizUI(false);
            ToggleSlideUI(true);
            UpdateBackButton();
        }
    }

    void NextSlide()
    {
        int level = Managers.Mission.curLevel;

        GameLogger.Log("TUTORIAL_SLIDE_VIEWED", new TutorialSlideViewedData(
            hologramScreenData.screenName,
            currentSlideIndex,
            hologramScreenData.slides[currentSlideIndex].slideText
        ), level);

        if (currentSlideIndex < hologramScreenData.slides.Count - 1)
        {
            var prevSlide = hologramScreenData.slides[currentSlideIndex];

            if (!string.IsNullOrEmpty(prevSlide.requiredEventName) && slideListeners.ContainsKey(currentSlideIndex))
            {
                Messenger.RemoveListener(prevSlide.requiredEventName, slideListeners[currentSlideIndex]);
                slideListeners.Remove(currentSlideIndex);
            }

            currentSlideIndex++;
            ShowSlide(currentSlideIndex);
        }
        else
        {
            if (hologramScreenData.quizzes != null && hologramScreenData.quizzes.Count > 0)
            {
                ShowQuiz(0);
            }
            else
            {
                CompleteTutorial();
            }
        }
    }

    public void PreviousSlide()
    {
        if (!allowBackNavigation) return;

        if (currentSlideIndex > 0)
        {
            currentSlideIndex--;
            ShowSlide(currentSlideIndex);
        }
        else
        {
            Debug.Log("Already at the first slide.");
        }
    }

    private void ShowQuiz(int index)
    {
        if (index >= hologramScreenData.quizzes.Count) return;

        currentQuiz = hologramScreenData.quizzes[index];

        if (questionText != null)
            questionText.text = currentQuiz.question;

        if (feedbackText != null)
            feedbackText.text = "";

        if (answerButtons != null)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                if (i < currentQuiz.answers.Length && answerButtons[i] != null)
                {
                    answerButtons[i].gameObject.SetActive(true);
                    int answerIndex = i;
                    var txt = answerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (txt != null)
                        txt.text = currentQuiz.answers[i];

                    answerButtons[i].onClick.RemoveAllListeners();
                    answerButtons[i].onClick.AddListener(() => CheckAnswer(answerIndex, currentQuiz.correctAnswerIndex));

                    var navigation = answerButtons[i].navigation;
                    navigation.mode = Navigation.Mode.None;
                    answerButtons[i].navigation = navigation;
                }
                else if (answerButtons[i] != null)
                {
                    answerButtons[i].gameObject.SetActive(false);
                }
            }
        }

        ToggleSlideUI(false);
        ToggleQuizUI(true);

        if (backButton != null)
            backButton.gameObject.SetActive(false);
    }

    private void CheckAnswer(int selectedIndex, int correctIndex)
    {
        int level = Managers.Mission.curLevel;
        bool isCorrect = selectedIndex == correctIndex;

        if (feedbackText != null)
        {
            feedbackText.text = isCorrect ? "Correct!" : "Try again!";
            Material newMat = new Material(feedbackText.fontMaterial);
            feedbackText.fontMaterial = newMat;
            feedbackText.fontMaterial.SetColor(TMPro.ShaderUtilities.ID_FaceColor, isCorrect ? Color.green : Color.red);
        }

        Messenger<int>.Broadcast(GameEvent.SCORE_FEEDBACK, isCorrect ? 10 : -5);
        ScoreManager.Instance.AddScore("TUTORIAL_QUIZ", isCorrect ? 10 : -5);

        GameLogger.Log("TUTORIAL_QUIZ",
            new TutorialQuizData(
                hologramScreenData.screenName,
                currentQuiz.question,
                currentQuiz.answers[selectedIndex],
                isCorrect ? "correct" : "wrong"),
            level);

        if (isCorrect)
        {
            if (currentQuizIndex < hologramScreenData.quizzes.Count - 1)
            {
                currentQuizIndex++;
                ShowQuiz(currentQuizIndex);
            }
            else
            {
                CompleteTutorial();
                Debug.Log("Quiz Finished!");
            }
        }
    }

    void CompleteTutorial()
    {
        if (!completedTutorials.Contains(hologramScreenData.screenName))
        {
            GameLogger.Log("TUTORIAL_COMPLETED",
                new TutorialCompletedData(hologramScreenData.screenName),
                Managers.Mission.curLevel);

            Messenger.Broadcast(completedEvent);
            completedTutorials.Add(hologramScreenData.screenName);
            Debug.Log("Tutorial completed.");
        }
    }

    private void ToggleSlideUI(bool isActive)
    {
        if (slideText != null)
            slideText.gameObject.SetActive(isActive);
        if (slideImage != null)
            slideImage.gameObject.SetActive(isActive);
    }

    private void ToggleQuizUI(bool isActive)
    {
        if (questionText != null)
            questionText.gameObject.SetActive(isActive);

        if (feedbackText != null)
            feedbackText.gameObject.SetActive(isActive);

        if (answerButtons != null)
        {
            foreach (Button btn in answerButtons)
            {
                if (btn != null)
                    btn.gameObject.SetActive(isActive);
            }
        }
    }

    void CleanupListeners()
    {
        foreach (var kvp in slideListeners)
        {
            var slide = hologramScreenData.slides[kvp.Key];
            if (!string.IsNullOrEmpty(slide.requiredEventName))
            {
                Messenger.RemoveListener(slide.requiredEventName, kvp.Value);
            }
        }
        slideListeners.Clear();
    }

    void UnlockSlide(int index)
    {
        unlockedSlides.Add(index);
    }

    void UpdateBackButton()
    {
        if (backButton != null)
        {
            bool canGoBack = allowBackNavigation && currentSlideIndex > 0;
            backButton.gameObject.SetActive(canGoBack);
            backButton.interactable = canGoBack;
        }
    }
}
