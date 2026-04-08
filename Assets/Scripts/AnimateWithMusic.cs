using PrimeTween;
using UnityEngine;

public class AnimateWithMusic : MonoBehaviour
{
    [SerializeField] private bool animatePosition;
    [SerializeField] private float positionYDuration;
    [SerializeField] private float positionYOffset;

    [SerializeField] private bool animateScale;
    [SerializeField] private float scaleDuration;
    [SerializeField] private float targetScale;

    private AudioManager audioManager;
    private Tween positionTween;
    private Tween scaleTween;
    private float startY;

    private void Awake()
    {
        audioManager = FindAnyObjectByType<AudioManager>();
        startY = transform.localPosition.y;
        audioManager.OnMusicStarted += StartTweens;
        audioManager.OnMusicStopped += StopTweens;
    }

    private void OnDestroy()
    {
        audioManager.OnMusicStarted -= StartTweens;
        audioManager.OnMusicStopped -= StopTweens;
    }

    private void StartTweens()
    {
        if (animatePosition)
        {
            positionTween = Tween.LocalPositionY(transform, endValue: startY + positionYOffset,
                duration: positionYDuration,
                ease: Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        }

        if (animateScale)
        {
            scaleTween = Tween.Scale(transform, endValue: targetScale, duration: scaleDuration,
                ease: Ease.InOutSine, cycles: -1, cycleMode: CycleMode.Yoyo);
        }
    }

    private void StopTweens()
    {
        positionTween.Stop();
        scaleTween.Stop();
    }
}