using System;
using NaughtyAttributes;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [Header("Animator Params")]
    [AnimatorParam("animator")]
    [SerializeField] private string moveSpeedParam;

    [AnimatorParam("animator")]
    [SerializeField] private string jumpTriggerParam;

    [AnimatorParam("animator")]
    [SerializeField] private string isAirborneParam;

    [AnimatorParam("animator")]
    [SerializeField] private string landTriggerParam;
    
    [AnimatorParam("animator")]
    [SerializeField] private string isLandingFrozenParam;

    public void SetMoveSpeed(float speed)
    {
        if (!animator || string.IsNullOrEmpty(moveSpeedParam)) return;
        animator.SetFloat(moveSpeedParam, Mathf.Abs(speed));
    }

    public void TriggerJump()
    {
        if (!animator || string.IsNullOrEmpty(jumpTriggerParam)) return;
        animator.SetTrigger(jumpTriggerParam);
    }

    public void SetIsAirborne(bool isAirborne)
    {
        if (!animator || string.IsNullOrEmpty(isAirborneParam)) return;
        animator.SetBool(isAirborneParam, isAirborne);
    }

    public void TriggerLand()
    {
        if (!animator || string.IsNullOrEmpty(landTriggerParam)) return;
        animator.SetTrigger(landTriggerParam);
    }
    
    public void SetIsLandingFrozen(bool isLandingFrozen)
    {
        if (!animator || string.IsNullOrEmpty(isLandingFrozenParam)) return;
        animator.SetBool(isLandingFrozenParam, isLandingFrozen);
    }
}