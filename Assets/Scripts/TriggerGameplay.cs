using System;
using UnityEngine;

public class TriggerGameplay : MonoBehaviour
{
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameManager || !other.CompareTag("Player")) return;
        gameManager.StartGameplay();
        gameObject.SetActive(false);
    }
}