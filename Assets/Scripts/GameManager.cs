using System;
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

    private void Awake()
    {
        player = FindAnyObjectByType<PlayerController>();
        headphones = FindAnyObjectByType<Headphones>();
        headphones.enabled = false;
        audioManager = FindAnyObjectByType<AudioManager>();
        batteryHUD = FindAnyObjectByType<BatteryHUD>();
        batteryHUD.gameObject.SetActive(false);
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
        if (player)
        {
            player.SetInputEnabled(false);
            player.PutOnHeadphones();
        }
    }

    private void OnHeadphonesBatteryDepleted()
    {
        if (player) player.TakeOffHeadphones();
    }
    
    private void OnPlayerHeadphonesOn()
    {
        if (audioManager) audioManager.TransitionToAlternate();
        if (batteryHUD) batteryHUD.gameObject.SetActive(true);
        if (player && player.IsFirstTime && headphones)
        {
            headphones.enabled = true;
            headphones.Charge(100);
        }
    }

    private void OnPlayerHeadphonesOff()
    {
        if (audioManager) audioManager.TransitionToReality();
        if (batteryHUD) batteryHUD.gameObject.SetActive(false);
        headphones.enabled = false;
    }
}