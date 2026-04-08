using UnityEngine;

public class TriggerEnd : MonoBehaviour
{
    private Collider2D triggerCollider;
    private GameManager gameManager;

    private void Awake()
    {
        gameManager = FindAnyObjectByType<GameManager>();
        triggerCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        gameManager.StartEnding();
        triggerCollider.enabled = false;
    }
}