using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerMovement movementLogic;
    [SerializeField] private PlayerAnimationController animationController;
    
    [Header("Feedback")]
    [SerializeField] private CinemachineImpulseSource landingImpulse;
    
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
        bool isJumpHeld = jumpAction.action.IsPressed(); 

        movementLogic.SetDropIntent(isDownHeld && isJumpHeld);
    }

    private void HandleStateTransitions()
    {
        // 1. Tell the Animator if it should be holding the squash pose
        animationController.SetIsLandingFrozen(movementLogic.IsHoldingImpact);

        // 2. Suppress the Run animation cycle if the player is in EITHER phase
        float animatedSpeed = movementLogic.IsMovementFrozen ? 0f : horizontalInput;
        
        animationController.SetMoveSpeed(animatedSpeed);
        animationController.SetIsAirborne(movementLogic.IsAirborne);

        if (movementLogic.HasJustJumped)
        {
            animationController.TriggerJump();
            movementLogic.ResetJumpFlag();
        }

        if (movementLogic.HasJustLanded)
        {
            animationController.TriggerLand();
            
            if (landingImpulse != null)
            {
                landingImpulse.GenerateImpulse();
            }
            
            movementLogic.ResetLandedFlag();
        }
    }

    private void HandleJumpInput(InputAction.CallbackContext context)
    {
        float verticalInput = moveAction.action.ReadValue<Vector2>().y;

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