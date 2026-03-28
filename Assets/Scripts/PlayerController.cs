using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement movementLogic;
    [SerializeField] private PlayerAnimationController animationController;
    
    [Header("Input Actions")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;

    private float horizontalInput;

    private void OnEnable()
    {
        moveAction.action.Enable();
        jumpAction.action.Enable();

        jumpAction.action.performed += HandleJumpInput;
        jumpAction.action.canceled += HandleJumpCanceled;
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();

        jumpAction.action.performed -= HandleJumpInput;
        jumpAction.action.canceled -= HandleJumpCanceled;
    }

    private void Update()
    {
        ReadMovementInput();
        ReadDropIntent();
        HandleStateTransitions();
    }

    private void FixedUpdate()
    {
        movementLogic.ApplyMovement(horizontalInput);
    }

    private void ReadMovementInput()
    {
        horizontalInput = moveAction.action.ReadValue<Vector2>().x;
    }

    private void ReadDropIntent()
    {
        bool isDownHeld = moveAction.action.ReadValue<Vector2>().y < -0.5f;
        
        // IsPressed() evaluates the continuous held state rather than the single frame trigger
        bool isJumpHeld = jumpAction.action.IsPressed(); 

        movementLogic.SetDropIntent(isDownHeld && isJumpHeld);
    }

    private void HandleStateTransitions()
    {
        animationController.SetMoveSpeed(horizontalInput);
        animationController.SetIsAirborne(movementLogic.IsAirborne);

        if (movementLogic.HasJustJumped)
        {
            animationController.TriggerJump();
            movementLogic.ResetJumpFlag();
        }

        if (movementLogic.HasJustLanded)
        {
            animationController.TriggerLand();
            movementLogic.ResetLandedFlag();
        }
    }

    private void HandleJumpInput(InputAction.CallbackContext context)
    {
        float verticalInput = moveAction.action.ReadValue<Vector2>().y;

        // If holding down, suppress the jump buffer. The drop intent polling will handle the mechanics.
        if (verticalInput >= -0.5f)
        {
            movementLogic.BufferJumpInput();
        }
    }

    private void HandleJumpCanceled(InputAction.CallbackContext context)
    {
        movementLogic.CancelJump();
    }
}