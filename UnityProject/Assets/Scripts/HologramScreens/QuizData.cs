using UnityEngine;

[CreateAssetMenu(fileName = "NewQuiz", menuName = "Tutorial/Quiz")]
public class QuizData : ScriptableObject
{
    [TextArea(2, 5)]
    public string question;
    public string[] answers;
    public int correctAnswerIndex;
}
