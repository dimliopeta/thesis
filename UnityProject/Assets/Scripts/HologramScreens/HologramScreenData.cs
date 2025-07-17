using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewHologramScreen", menuName = "Tutorial/HologramScreen")]
public class HologramScreenData : ScriptableObject
{
    public string screenName; 
    public List<SlideData> slides; 
    public List<QuizData> quizzes;
}
