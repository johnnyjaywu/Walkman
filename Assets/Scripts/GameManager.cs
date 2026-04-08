using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    // [SerializeField] private UnityEvent onIntroStarted;
    [SerializeField] private UnityEvent onGameplayStarted;

    private Headphones headphones;
    private PlayerController player;
    private AudioManager audioManager;
    private BatteryHUD batteryHUD;
    private Oldman oldman;
    private CameraZoomController cameraZoomController;
    private FullscreenMaskController maskController;

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerController>();
        headphones = FindAnyObjectByType<Headphones>();
        headphones.enabled = false;
        audioManager = FindAnyObjectByType<AudioManager>();
        batteryHUD = FindAnyObjectByType<BatteryHUD>();
        batteryHUD.gameObject.SetActive(false);
        oldman = FindAnyObjectByType<Oldman>();
        cameraZoomController = FindAnyObjectByType<CameraZoomController>();
        maskController = FindAnyObjectByType<FullscreenMaskController>();
    }

    private void OnEnable()
    {
        player.OnHeadphonesOn += OnPlayerHeadphonesOn;
        player.OnHeadphonesOff += OnPlayerHeadphonesOff;
        headphones.OnBatteryDepleted += OnHeadphonesBatteryDepleted;
    }

    private void OnDisable()
    {
        player.OnHeadphonesOn -= OnPlayerHeadphonesOn;
        player.OnHeadphonesOff -= OnPlayerHeadphonesOff;
        headphones.OnBatteryDepleted -= OnHeadphonesBatteryDepleted;
    }

    public void StartGameplay()
    {
        StartCoroutine(FirstTimeSequence());
    }

    public void StartEnding()
    {
        cameraZoomController.ZoomOut();
        // maskController.MaskSize = 20f;
        headphones.DepletionRate = 0;
        headphones.Charge(100);
        player.SetGravityEnabled(false);
        player.SetInputEnabled(false);
    }

    private void OnHeadphonesBatteryDepleted()
    {
        player.TakeOffHeadphones();
    }

    private void OnPlayerHeadphonesOn()
    {
        audioManager.TransitionToAlternate();
        batteryHUD.gameObject.SetActive(true);
        if (player.IsFirstTime)
        {
            headphones.enabled = true;
            headphones.Charge(100);
        }
    }

    private void OnPlayerHeadphonesOff()
    {
        audioManager.TransitionToReality();
        batteryHUD.gameObject.SetActive(false);
        headphones.enabled = false;
        player.ResetGameplay();
        oldman.ResetGameplay();
    }

    private IEnumerator FirstTimeSequence()
    {
        player.SetInputEnabled(false);
        oldman.GiveWalkman();
        yield return new WaitForSeconds(2f);
        player.PutOnHeadphones();
        oldman.Idle();
    }
}