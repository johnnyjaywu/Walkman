using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using UnityEngine.UI;

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

    public bool IsFirstTime { get; private set; } = true;
    public event Action OnHeadphonesOn;
    public event Action OnHeadphonesOff;

    public void SetInputEnabled(bool enable)
    {
        if (enable)
        {
            moveAction.action.Enable();
            jumpAction.action.Enable();
        }
        else
        {
            moveAction.action.Disable();
            jumpAction.action.Disable();
        }
    }

    public void SetJumpEnabled(bool enable)
    {
        if (enable)
        {
            jumpAction.action.Enable();
        }
        else
        {
            jumpAction.action.Disable();
        }
    }

    public void PutOnHeadphones() => animationController.TriggerPutOnHeadphones(IsFirstTime);

    public void TakeOffHeadphones() => animationController.TriggerTakeOffHeadphones();

    private void OnEnable()
    {
        SetInputEnabled(true);
        SetJumpEnabled(false);

        jumpAction.action.performed += HandleJumpInput;
        jumpAction.action.canceled += HandleJumpCanceled;
        animationController.OnStateEntered += HandleAnimationStateEntered;
        animationController.OnStateExited += HandleAnimationStateExited;
    }

    private void OnDisable()
    {
        SetInputEnabled(false);

        jumpAction.action.performed -= HandleJumpInput;
        jumpAction.action.canceled -= HandleJumpCanceled;
        animationController.OnStateEntered -= HandleAnimationStateEntered;
        animationController.OnStateExited -= HandleAnimationStateExited;
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

    private void HandleAnimationStateEntered(string stateName)
    {
        switch (stateName)
        {
            case "HeadphonesOn":
                OnHeadphonesOn?.Invoke();
                SetJumpEnabled(true);
                break;
            case "HeadphonesOff":
                OnHeadphonesOff?.Invoke();
                SetJumpEnabled(false);
                break;
        }
    }

    private void HandleAnimationStateExited(string stateName)
    {
        switch (stateName)
        {
            case "Listening":
                SetInputEnabled(true);
                break;
            case "HeadphonesOn":
                IsFirstTime = false;
                break;
            case "HeadphonesOff":
                Debug.Log("End Game here!");
                break;
        }
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