using System;
using NaughtyAttributes;
using UnityEngine;

public class Headphones : MonoBehaviour
{
    private FullscreenMaskController fullscreenMaskController;

    [OnValueChanged(nameof(OnBatteryUpdated))]
    [Range(0, 100)]
    [SerializeField] private float currentBatteryLevel = 1;

    [Tooltip("Rate of which battery level depletes per second")]
    [SerializeField] private float depletionRate = 1f;

    [Tooltip("Rate of which battery level replenishes per second when charging")]
    [SerializeField] private float chargeRate = 25f;

    public float CurrentBatteryLevel => currentBatteryLevel;
    public event Action OnBatteryLevelChanged;
    public event Action OnBatteryDepleted;

    private float targetBatteryLevel;

    private void Awake()
    {
        fullscreenMaskController = FindAnyObjectByType<FullscreenMaskController>();
    }

    private void Update()
    {
        ProcessBatteryState();
    }

    private void ProcessBatteryState()
    {
        // If charging
        if (currentBatteryLevel < targetBatteryLevel)
        {
            currentBatteryLevel = Mathf.MoveTowards(currentBatteryLevel, targetBatteryLevel, chargeRate * Time.deltaTime);
        }
        else if (currentBatteryLevel > 0)
        {
            // Standard linear depletion
            currentBatteryLevel -= depletionRate * Time.deltaTime;
            currentBatteryLevel = Mathf.Max(currentBatteryLevel, 0);
            
            // Keep target synced so AddBattery evaluates from the correct baseline
            targetBatteryLevel = currentBatteryLevel; 
            
        }
        OnBatteryUpdated();
    }

    private void OnBatteryUpdated()
    {
        if (fullscreenMaskController != null)
        {
            fullscreenMaskController.SetMaskAmount(currentBatteryLevel / 100f);
        }
        
        // WARNING: Invoking this per-frame during Update can cause severe performance drops 
        // if listeners execute heavy logic (e.g., Canvas rebuilds). 
        OnBatteryLevelChanged?.Invoke();
        if (currentBatteryLevel <= 0f) OnBatteryDepleted?.Invoke();
    }

    public void Charge(float amount)
    {
        // Push the target up; the Update loop will handle the interpolation
        targetBatteryLevel = Mathf.Clamp(currentBatteryLevel + amount, 0, 100);
    }
}