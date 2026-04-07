using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Battery : MonoBehaviour
{
    [Range(0, 100)]
    [SerializeField] private int chargeAmount = 15;

    private Headphones headphones;

    private void Awake()
    {
        headphones = FindAnyObjectByType<Headphones>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (!headphones) return;
        headphones.Charge(chargeAmount);
        Destroy(gameObject);
    }
}