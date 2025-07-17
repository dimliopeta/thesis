using UnityEngine;

[System.Serializable]
public class SlideData
{
    [TextArea(2, 5)]
    public string slideText; // Κείμενο του slide
    public Sprite slideImage; // Εικόνα του slide (αν χρειάζεται)
    public string requiredEventName;
}
