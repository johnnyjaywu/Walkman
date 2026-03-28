using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Battery : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField] private int chargeAmount = 15;

    private BatteryController batteryController;

    private void Awake()
    {
        batteryController = FindAnyObjectByType<BatteryController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!batteryController) return;
        batteryController.AddBattery(chargeAmount);
        Destroy(gameObject);
    }
}