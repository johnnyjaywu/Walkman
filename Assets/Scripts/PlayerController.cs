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
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        jumpAction.action.Disable();

        jumpAction.action.performed -= HandleJumpInput;
    }

    private void Update()
    {
        ReadMovementInput();
        HandleStateTransitions();
    }

    private void FixedUpdate()
    {
        // Physics-affecting logic must remain in FixedUpdate for deterministic execution
        movementLogic.ApplyMovement(horizontalInput);
    }

    private void ReadMovementInput()
    {
        horizontalInput = moveAction.action.ReadValue<Vector2>().x;
    }

    private void HandleStateTransitions()
    {
        animationController.SetMoveSpeed(horizontalInput);
        animationController.SetIsAirborne(movementLogic.IsAirborne);

        // Consume flags to ensure triggers fire exactly once per state change
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
        movementLogic.ExecuteJump();
    }
}