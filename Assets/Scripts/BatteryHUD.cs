using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BatteryHUD : MonoBehaviour
{
    [SerializeField] private Image batteryImage;
    [SerializeField] private TextMeshProUGUI batteryText;
    [SerializeField] private List<Sprite> chargeImages = new();

    private BatteryController batteryController;

    private void Awake()
    {
        batteryController = FindAnyObjectByType<BatteryController>();
    }

    private void OnEnable()
    {
        if (!batteryController) return;
        batteryController.OnBatteryLevelChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDisable()
    {
        if (!batteryController) return;
        batteryController.OnBatteryLevelChanged += UpdateUI;
    }

    private void UpdateUI()
    {
        if (!batteryController) return;

        float percent = batteryController.CurrentBatteryLevel / 100f;
        if (batteryImage)
            batteryImage.fillAmount = percent;
        if (batteryText)
            batteryText.SetText($"{Mathf.RoundToInt(batteryController.CurrentBatteryLevel)}%");
        if (chargeImages is { Count: > 0 })
        {
            int index = percent >= 0.85f ? chargeImages.Count - 1 : Mathf.CeilToInt(Mathf.Min(percent * (chargeImages.Count - 1), chargeImages.Count - 2));
            // int index = Mathf.CeilToInt(Mathf.Min(percent * chargeImages.Count, chargeImages.Count - 1));
            // Debug.Log($"Percent: {percent} at index: {index}");
            batteryImage.sprite = chargeImages[index];
        }
    }
}