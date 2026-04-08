using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Animator))]
public class Oldman : MonoBehaviour
{
    [SerializeField] private Animator animator;

    [AnimatorParam("animator")]
    [SerializeField] private int giveParam;

    [AnimatorParam("animator")]
    [SerializeField] private int idleParam;

    public bool IsFacingRight { get; private set; }
    private GameManager gameManager;
    private Collider2D triggerCollider;

    private void Reset()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        gameManager = FindAnyObjectByType<GameManager>();
        triggerCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!gameManager || !other.CompareTag("Player")) return;
        
        if (other.transform.position.x > transform.position.x && !IsFacingRight || other.transform.position.x < transform.position.x && IsFacingRight)
            Flip();

        gameManager.StartGameplay();
        triggerCollider.enabled = false;
    }

    private void Flip()
    {
        IsFacingRight = !IsFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    public void ResetGameplay()
    {
        triggerCollider.enabled = true;
    }

    public void GiveWalkman()
    {
        if (animator == null) return;
        animator.SetTrigger(giveParam);
    }

    public void Idle()
    {
        if (animator == null) return;
        animator.SetTrigger(idleParam);
    }
}