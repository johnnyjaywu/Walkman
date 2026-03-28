using System;
using NaughtyAttributes;
using UnityEngine;

public class BatteryController : MonoBehaviour
{
    [SerializeField] private FullscreenMaskController fullscreenMaskController;

    [OnValueChanged(nameof(UpdateBatteryLevel))]
    [Range(0, 100)]
    [SerializeField] private float currentBatteryLevel = 0;

    [Tooltip("Rate of which battery level depletes per second")]
    [SerializeField] private float depletionRate = 1f;

    [Tooltip("Rate of which battery level replenishes per second when charging")]
    [SerializeField] private float chargeRate = 25f;

    public float CurrentBatteryLevel => currentBatteryLevel;
    public event Action OnBatteryLevelChanged;

    private float targetBatteryLevel;

    private void OnEnable()
    {
        targetBatteryLevel = currentBatteryLevel;
        SetBatteryLevel(currentBatteryLevel);
    }

    private void Update()
    {
        ProcessBatteryState();
    }

    private void ProcessBatteryState()
    {
        // Intercept standard depletion if we have a target to charge towards
        if (currentBatteryLevel < targetBatteryLevel)
        {
            currentBatteryLevel = Mathf.MoveTowards(currentBatteryLevel, targetBatteryLevel, chargeRate * Time.deltaTime);
            UpdateBatteryLevel();
        }
        else if (currentBatteryLevel > 0)
        {
            // Standard linear depletion
            currentBatteryLevel -= depletionRate * Time.deltaTime;
            currentBatteryLevel = Mathf.Max(currentBatteryLevel, 0);
            
            // Keep target synced so AddBattery evaluates from the correct baseline
            targetBatteryLevel = currentBatteryLevel; 
            
            UpdateBatteryLevel();
        }
    }

    private void UpdateBatteryLevel()
    {
        if (fullscreenMaskController != null)
        {
            fullscreenMaskController.SetMaskAmount(currentBatteryLevel / 100f);
        }
        
        // WARNING: Invoking this per-frame during Update can cause severe performance drops 
        // if listeners execute heavy logic (e.g., Canvas rebuilds). 
        OnBatteryLevelChanged?.Invoke();
    }

    public void SetBatteryLevel(float level)
    {
        currentBatteryLevel = Mathf.Clamp(level, 0, 100);
        targetBatteryLevel = currentBatteryLevel;
        UpdateBatteryLevel();
    }

    public void AddBattery(float amount)
    {
        // Push the target up; the Update loop will handle the interpolation
        targetBatteryLevel = Mathf.Clamp(currentBatteryLevel + amount, 0, 100);
    }
}