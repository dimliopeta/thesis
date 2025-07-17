using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class CellInteraction : MonoBehaviour, IPointerClickHandler
{
    private QueryColorGame controller;
    private Image image;

    public void Init(QueryColorGame gameController)
    {
        controller = gameController;
        image = GetComponentInChildren<Image>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Cell clicked: " + gameObject.name);
        if (controller == null) return;

        controller.CheckCell(this);
    }

    public void SetSprite(Sprite sprite)
    {
        if (image != null) image.sprite = sprite;
    }

    public IEnumerator FlashWrong(Sprite wrongSprite, Sprite defaultSprite, float delay)
    {
        Messenger.Broadcast("WRONG_ANSWER");
        SetSprite(wrongSprite);
        yield return new WaitForSeconds(delay);
        SetSprite(defaultSprite);
    }
}