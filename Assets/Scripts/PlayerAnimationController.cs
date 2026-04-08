using System;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Animator), typeof(AnimationEventDispatcher))]
public class PlayerAnimationController : MonoBehaviour
{
    public event Action<string> OnStateEntered;
    public event Action<string> OnStateExited;
    public event Action<string> OnCustomEventTriggered;

    [SerializeField] private Animator animator;
    [SerializeField] private AnimationEventDispatcher eventDispatcher;

    [Header("State Tracking")]
    [Tooltip("Add the exact string names of the Animator states you want to track via events.")]
    [SerializeField] private string[] trackedAnimatorStates;

    [Header("Animator Params")]
    [AnimatorParam("animator")]
    [SerializeField] private int moveSpeedParam;

    [AnimatorParam("animator")]
    [SerializeField] private int jumpTriggerParam;

    [AnimatorParam("animator")]
    [SerializeField] private int isAirborneParam;

    [AnimatorParam("animator")]
    [SerializeField] private int isLandingFrozenParam;

    [AnimatorParam("animator")]
    [SerializeField] private int putOnHeadphonesParam;

    [AnimatorParam("animator")]
    [SerializeField] private int takeOffHeadphonesParam;

    [AnimatorParam("animator")]
    [SerializeField] private int isFirstTimeParam;
    
    [AnimatorParam("animator")]
    [SerializeField] private int hasHeadphonesParam;

    private readonly Dictionary<int, string> stateHashLookup = new Dictionary<int, string>();

    private void Reset()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (eventDispatcher == null)
        {
            eventDispatcher = GetComponent<AnimationEventDispatcher>();
        }
    }

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (eventDispatcher == null)
        {
            eventDispatcher = GetComponent<AnimationEventDispatcher>();
        }

        InitializeStateLookup();
        SubscribeToAnimationEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromAnimationEvents();
    }

    private void InitializeStateLookup()
    {
        if (trackedAnimatorStates == null) return;

        foreach (string stateName in trackedAnimatorStates)
        {
            if (string.IsNullOrEmpty(stateName)) continue;

            int hash = Animator.StringToHash(stateName);
            stateHashLookup[hash] = stateName;
        }
    }

    private void SubscribeToAnimationEvents()
    {
        if (animator != null)
        {
            AnimationStateReporter[] reporters = animator.GetBehaviours<AnimationStateReporter>();

            foreach (AnimationStateReporter reporter in reporters)
            {
                reporter.OnStateEntered += HandleStateEntered;
                reporter.OnStateExited += HandleStateExited;
            }
        }

        if (eventDispatcher != null)
        {
            eventDispatcher.OnCustomEventTriggered += HandleCustomEvent;
        }
    }

    private void UnsubscribeFromAnimationEvents()
    {
        if (animator != null)
        {
            AnimationStateReporter[] reporters = animator.GetBehaviours<AnimationStateReporter>();

            foreach (AnimationStateReporter reporter in reporters)
            {
                reporter.OnStateEntered -= HandleStateEntered;
                reporter.OnStateExited -= HandleStateExited;
            }
        }

        if (eventDispatcher != null)
        {
            eventDispatcher.OnCustomEventTriggered -= HandleCustomEvent;
        }
    }

    private void HandleStateEntered(int stateHash)
    {
        if (stateHashLookup.TryGetValue(stateHash, out string stateName))
        {
            OnStateEntered?.Invoke(stateName);
        }
    }

    private void HandleStateExited(int stateHash)
    {
        if (stateHashLookup.TryGetValue(stateHash, out string stateName))
        {
            OnStateExited?.Invoke(stateName);
        }
    }

    private void HandleCustomEvent(string eventName)
    {
        OnCustomEventTriggered?.Invoke(eventName);
    }

    public void SetMoveSpeed(float speed)
    {
        if (animator == null) return;
        animator.SetFloat(moveSpeedParam, Mathf.Abs(speed));
    }

    public void TriggerJump()
    {
        if (animator == null) return;
        animator.SetTrigger(jumpTriggerParam);
    }

    public void SetIsAirborne(bool isAirborne)
    {
        if (animator == null) return;
        animator.SetBool(isAirborneParam, isAirborne);
    }

    public void SetIsLandingFrozen(bool isLandingFrozen)
    {
        if (animator == null) return;
        animator.SetBool(isLandingFrozenParam, isLandingFrozen);
    }

    public void TriggerPutOnHeadphones(bool isFirstTime)
    {
        if (animator == null) return;
        SetIsFirstTime(isFirstTime);
        animator.SetTrigger(putOnHeadphonesParam);
        animator.SetBool(hasHeadphonesParam, true);
    }

    public void TriggerTakeOffHeadphones()
    {
        if (animator == null) return;
        animator.SetTrigger(takeOffHeadphonesParam);
        animator.SetBool(hasHeadphonesParam, false);
    }

    private void SetIsFirstTime(bool value)
    {
        if (animator == null) return;
        animator.SetBool(isFirstTimeParam, value);
    }
}