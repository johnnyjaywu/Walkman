using System;
using UnityEngine;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onGameplayStarted;
    
    private BatteryController batteryController;

    private void Awake()
    {
        batteryController = FindAnyObjectByType<BatteryController>();
        
        Invoke("StartGameplay", 3f);
    }

    private void StartGameplay() => onGameplayStarted?.Invoke();
}
