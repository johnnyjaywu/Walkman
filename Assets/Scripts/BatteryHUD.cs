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

    private Headphones headphones;

    private void Awake()
    {
        headphones = FindAnyObjectByType<Headphones>();
    }

    private void OnEnable()
    {
        if (!headphones) return;
        headphones.OnBatteryLevelChanged += UpdateUI;
        UpdateUI();
    }

    private void OnDisable()
    {
        if (!headphones) return;
        headphones.OnBatteryLevelChanged += UpdateUI;
    }

    private void UpdateUI()
    {
        if (!headphones) return;

        float percent = headphones.CurrentBatteryLevel / 100f;
        if (batteryImage)
            batteryImage.fillAmount = percent;
        if (batteryText)
            batteryText.SetText($"{Mathf.RoundToInt(headphones.CurrentBatteryLevel)}%");
        if (chargeImages is { Count: > 0 })
        {
            int index = percent >= 0.85f ? chargeImages.Count - 1 : Mathf.CeilToInt(Mathf.Min(percent * (chargeImages.Count - 1), chargeImages.Count - 2));
            // int index = Mathf.CeilToInt(Mathf.Min(percent * chargeImages.Count, chargeImages.Count - 1));
            // Debug.Log($"Percent: {percent} at index: {index}");
            batteryImage.sprite = chargeImages[index];
        }
    }
}