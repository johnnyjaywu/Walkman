using System;
using NaughtyAttributes;
using UnityEngine;

public class Headphones : MonoBehaviour
{
    private FullscreenMaskController fullscreenMaskController;

    [OnValueChanged(nameof(OnBatteryUpdated))]
    [Range(0, 100)]
    [SerializeField]
    private float currentBatteryLevel = 100f; // Initialized to 100 for safety, adjust as needed in inspector

    [Tooltip("Rate of which battery level depletes per second")]
    [SerializeField] private float depletionRate = 1f;

    [Tooltip("Rate of which battery level replenishes per second when charging")]
    [SerializeField] private float chargeRate = 25f;

    [SerializeField] private AudioSource targetAudioSource;

    [Tooltip("Optional: Add an Audio Distortion Filter component to this GameObject for added crunch.")]
    [SerializeField] private AudioDistortionFilter distortionFilter;

    [Header("Effect Curves")]
    [Tooltip("X: 0 (Normal) to 1 (Dead). Y: Pitch multiplier (e.g., 1.0 to 0.2).")]
    [SerializeField] private AnimationCurve pitchCurve = AnimationCurve.Linear(0f, 1f, 1f, 0.2f);

    [Tooltip("X: 0 (Normal) to 1 (Dead). Y: Distortion level (e.g., 0.0 to 0.8).")]
    [SerializeField] private AnimationCurve distortionCurve = AnimationCurve.Linear(0f, 0f, 1f, 0.8f);

    public float CurrentBatteryLevel => currentBatteryLevel;

    public float DepletionRate
    {
        get => depletionRate;
        set => depletionRate = value;
    }

    public event Action OnBatteryLevelChanged;
    public event Action OnBatteryDepleted;

    private float targetBatteryLevel;

    private void Awake()
    {
        fullscreenMaskController = FindAnyObjectByType<FullscreenMaskController>();
        if (targetAudioSource == null)
        {
            targetAudioSource = GetComponent<AudioSource>();
        }

        if (distortionFilter == null)
        {
            distortionFilter = GetComponent<AudioDistortionFilter>();
        }

        targetBatteryLevel = currentBatteryLevel;
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
            currentBatteryLevel =
                Mathf.MoveTowards(currentBatteryLevel, targetBatteryLevel, chargeRate * Time.deltaTime);
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

        UpdateBatteryAudioState();

        // WARNING: Invoking this per-frame during Update can cause severe performance drops 
        // if listeners execute heavy logic (e.g., Canvas rebuilds). 
        OnBatteryLevelChanged?.Invoke();
        if (currentBatteryLevel <= 0f) OnBatteryDepleted?.Invoke();
    }

    private void UpdateBatteryAudioState()
    {
        if (targetAudioSource == null) return;

        // Convert 0-100 battery level to 0.0-1.0 percentage
        float batteryPercentage = currentBatteryLevel / 100f;

        // Invert so 100% battery = 0.0 (Normal) and 0% battery = 1.0 (Dead)
        float deadState = 1f - batteryPercentage;
        float clampedState = Mathf.Clamp01(deadState);

        targetAudioSource.pitch = pitchCurve.Evaluate(clampedState);

        if (distortionFilter != null)
        {
            distortionFilter.distortionLevel = distortionCurve.Evaluate(clampedState);
        }
    }

    public void Charge(float amount)
    {
        // Push the target up; the Update loop will handle the interpolation
        targetBatteryLevel = Mathf.Clamp(currentBatteryLevel + amount, 0, 100);
    }
}