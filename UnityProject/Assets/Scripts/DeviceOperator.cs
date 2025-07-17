using UnityEngine;

public class DeviceOperator : MonoBehaviour
{
    public float maxDistance = 12.0f;

    private RaycastHit lastHit;
    private bool isInteractableInView = false;

    [SerializeField] private float eyeHeight = 1.2f;
    

    void Update()
    {
        if (Managers.GameState.IsPaused || Managers.GameState.IsChatOpen)
            return;
        
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 direction = transform.forward;

        Debug.DrawRay(origin, direction * maxDistance, Color.red);

        bool raycastHit = Physics.Raycast(origin, direction, out lastHit, maxDistance);

        isInteractableInView = false;

        if (raycastHit)
        {
            var collider = lastHit.collider;
            if (collider.GetComponent<NPCChat>() ||
                collider.GetComponent<HologramScreen>() ||
                collider.GetComponent<DeviceTrigger>() || 
                collider.GetComponent<StepController>())
            {
                isInteractableInView = true;
            }
        }

        // Μόνο τώρα το broadcast έχει σωστή σημασία
        Messenger<bool>.Broadcast("INTERACTABLE", isInteractableInView);

        if (Input.GetKeyDown(KeyCode.E) && isInteractableInView)
        {
            if (lastHit.collider.TryGetComponent<NPCChat>(out var npc))
            {
                npc.Interact(transform);
            }
            else if (lastHit.collider.TryGetComponent<HologramScreen>(out var holo))
            {
                holo.Interact();
            }
            else if (lastHit.collider.TryGetComponent<DeviceTrigger>(out var dev))
            {
                dev.Interact();
            }
            else if (lastHit.collider.TryGetComponent<StepController>(out var step))
            {
                step.Interact(transform);
            }

        }
    }
}
