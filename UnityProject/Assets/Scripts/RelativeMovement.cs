using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelativeMovement : MonoBehaviour
{
    [SerializeField] Transform target;
    public float moveSpeed = 6.0f;
    private CharacterController charController;
    private Animator animator;
    public float rotSpeed = 15.0f;

    void Start()
    {
        Messenger.AddListener(GameEvent.RIGHT_ANSWER, SuccessAnimation);
        Messenger.AddListener(GameEvent.WRONG_ANSWER, DefeatAnimation);
        charController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void OnDestroy()
    { 
        Messenger.RemoveListener(GameEvent.RIGHT_ANSWER, SuccessAnimation);
        Messenger.RemoveListener(GameEvent.WRONG_ANSWER, DefeatAnimation);
    }

    void Update()
    {
        if (Managers.GameState.IsPaused || Managers.GameState.IsChatOpen)
            return;
        Vector3 movement = Vector3.zero;
        float horInput = Input.GetAxis("Horizontal");
        float verInput = Input.GetAxis("Vertical");

        if (horInput != 0 || verInput != 0)
        {

            Vector3 right = target.right;
            Vector3 forward = Vector3.Cross(right, Vector3.up);
            movement = (right * horInput) + (forward * verInput);
            movement *= moveSpeed;
            movement = Vector3.ClampMagnitude(movement, moveSpeed);


            Quaternion direction = Quaternion.LookRotation(movement);
            transform.rotation = Quaternion.Lerp(transform.rotation, direction, rotSpeed * Time.deltaTime);
            if (Messenger.HasListener("MOVE_ACTION"))
            {
                Messenger.Broadcast("MOVE_ACTION");
            }
        }

        animator.SetFloat("speed", movement.sqrMagnitude);
        movement *= Time.deltaTime;
        charController.Move(movement);
    }

    void SuccessAnimation() {  animator.SetTrigger("success"); }
    
    void DefeatAnimation() { animator.SetTrigger("defeat"); }
}
