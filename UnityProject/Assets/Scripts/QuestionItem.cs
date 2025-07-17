using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QuestionItem : MonoBehaviour
{
    public Button questionButton;
    public Image arrowImage;
    public Sprite arrowDown;
    public Sprite arrowUp;

    public CanvasGroup answerCanvasGroup;
    public RectTransform answerRect; // αναθέτεις το RectTransform του Panel

    public float fadeDuration = 0.2f;

    [HideInInspector] public bool isOpen = false;

    private void Start()
    {
        answerCanvasGroup.alpha = 0;
        answerCanvasGroup.interactable = false;
        answerCanvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeIn()
    {
        float t = 0f;
        answerCanvasGroup.interactable = true;
        answerCanvasGroup.blocksRaycasts = true;

        while (t < fadeDuration)
        {
            answerCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        answerCanvasGroup.alpha = 1f;
    }

    public IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            answerCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            t += Time.deltaTime;
            yield return null;
        }
        answerCanvasGroup.alpha = 0f;

        answerCanvasGroup.interactable = false;
        answerCanvasGroup.blocksRaycasts = false;
    }

        public float AnswerHeight
    {
        get
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(answerRect);
            return answerRect.rect.height; // top + bottom
        }
    }

}
