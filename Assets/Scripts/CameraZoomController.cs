using PrimeTween;
using Unity.Cinemachine;
using UnityEngine;

public class CameraZoomController : MonoBehaviour
{
    [SerializeField] private float targetDistance = 25f;
    [SerializeField] private float transitionDuration = 3f;

    [SerializeField] private CinemachinePositionComposer cameraPositionComposer;
    [SerializeField] private Camera altCamera;
    private float initialDistance;
    private float initialFov;
    private Tween zoomTween;
    // private Tween zoomAltTween;

    private void Awake()
    {
        initialDistance = cameraPositionComposer.CameraDistance;
        // initialFov = mainCamera.Lens.FieldOfView;
    }

    public void ZoomOut()
    {
        if (zoomTween.isAlive) zoomTween.Stop();
        zoomTween = Tween.Custom(
            startValue: cameraPositionComposer.CameraDistance,
            endValue: targetDistance,
            duration: transitionDuration,
            ease: Ease.InOutSine,
            onValueChange: ApplyZoom);

        // if (zoomAltTween.isAlive) zoomAltTween.Stop();
        // zoomAltTween = Tween.CameraFieldOfView(
        //     target: altCamera,
        //     startValue: altCamera.fieldOfView,
        //     endValue: targetFov,
        //     duration: transitionDuration,
        //     ease: Ease.InOutSine);
    }


    public void ResetZoom()
    {
        if (zoomTween.isAlive) zoomTween.Stop();
        zoomTween = Tween.Custom(
            startValue: cameraPositionComposer.CameraDistance,
            endValue: initialFov,
            duration: transitionDuration,
            ease: Ease.InOutSine,
            onValueChange: ApplyZoom);
        
        // if (zoomAltTween.isAlive) zoomAltTween.Stop();
        // zoomAltTween = Tween.CameraFieldOfView(
        //     target: altCamera,
        //     startValue: altCamera.fieldOfView,
        //     endValue: initialFov,
        //     duration: transitionDuration,
        //     ease: Ease.InOutSine);
    }

    private void ApplyZoom(float value)
    {
        // var lens = mainCamera.Lens;
        // lens.FieldOfView = value;
        // mainCamera.Lens = lens;
        cameraPositionComposer.CameraDistance = value;
    }
}