using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeviceTrigger : MonoBehaviour
{
    [SerializeField] private Vector3 dPos;
    [SerializeField] private string message = "You must complete the tutorial first!";
    private bool open = false;

    void OnEnable()
    {
        Messenger.AddListener(GameEvent.TUTORIAL_COMPLETED, DeviceActivated);
    }

    void OnDisable()
    {
        Messenger.RemoveListener(GameEvent.TUTORIAL_COMPLETED, DeviceActivated);
    }

    private void DeviceActivated()
    {
        if (!open)
        {
            open = !open;
        }
    }
    public void Interact()
    {
        if (open)
        {
            Vector3 pos = transform.position - dPos; 
            transform.position = pos;
        }
        else
        {
            Messenger<string, bool>.Broadcast(GameEvent.SHOW_MESSAGE, message, true);
        }
    }

}
