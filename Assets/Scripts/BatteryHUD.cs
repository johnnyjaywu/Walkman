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
            int index = (int)Mathf.Min(percent * chargeImages.Count, chargeImages.Count - 1);
            batteryImage.sprite = chargeImages[index];
        }
    }
}