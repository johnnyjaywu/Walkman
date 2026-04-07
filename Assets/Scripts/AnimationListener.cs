using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AnimationEventDispatcher))]
public class AnimationListener : MonoBehaviour
{
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private AnimationEventDispatcher eventDispatcher;

    private void Awake()
    {
        if (targetAnimator == null)
        {
            targetAnimator = GetComponent<Animator>();
        }

        if (eventDispatcher == null)
        {
            eventDispatcher = GetComponent<AnimationEventDispatcher>();
        }

        AnimationStateReporter[] reporters = targetAnimator.GetBehaviours<AnimationStateReporter>();
        
        foreach (AnimationStateReporter reporter in reporters)
        {
            reporter.OnStateEntered += HandleStateEntered;
            reporter.OnStateExited += HandleStateExited;
        }

        eventDispatcher.OnCustomEventTriggered += HandleCustomEvent;
    }

    private void OnDestroy()
    {
        if (targetAnimator == null || eventDispatcher == null)
        {
            return;
        }

        AnimationStateReporter[] reporters = targetAnimator.GetBehaviours<AnimationStateReporter>();
        
        foreach (AnimationStateReporter reporter in reporters)
        {
            reporter.OnStateEntered -= HandleStateEntered;
            reporter.OnStateExited -= HandleStateExited;
        }

        eventDispatcher.OnCustomEventTriggered -= HandleCustomEvent;
    }

    private void HandleStateEntered(int stateHash)
    {
        // Route to state machine initialization
    }

    private void HandleStateExited(int stateHash)
    {
        // Route to state machine cleanup
    }

    private void HandleCustomEvent(string eventName)
    {
        // Route discrete timeline events (e.g., "Footstep_Left", "Hitbox_Active")
    }
}