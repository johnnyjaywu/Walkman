using System;
using UnityEngine;

public class AnimationStateReporter : StateMachineBehaviour
{
    public event Action<int> OnStateEntered;
    public event Action<int> OnStateExited;

    public override void OnStateEnter(Animator targetAnimator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Broadcasts immediately when the blending transitions into this state
        OnStateEntered?.Invoke(stateInfo.shortNameHash);
    }

    public override void OnStateExit(Animator targetAnimator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        // Broadcasts deterministically when exiting, ensuring state logic can cleanup safely
        OnStateExited?.Invoke(stateInfo.shortNameHash);
    }
}