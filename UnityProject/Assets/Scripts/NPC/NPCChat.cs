using UnityEngine;

public class NPCChat : MonoBehaviour
{
    public void Interact(Transform playerTransform)
    {
        if (!Managers.GameState.IsChatOpen)
        {
            LookAtPlayer(playerTransform);
            Messenger.Broadcast(GameEvent.SHOW_CHAT);
        }
    }

    void Update()
    {
        if (Managers.GameState.IsChatOpen && Input.GetKeyDown(KeyCode.Escape))
        {
            Messenger.Broadcast(GameEvent.CLOSE_CHAT); // δεν αγγίζει IsChatOpen
        }
    }

    private void LookAtPlayer(Transform playerTransform)
    {
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = lookRotation;
        }
    }
}
