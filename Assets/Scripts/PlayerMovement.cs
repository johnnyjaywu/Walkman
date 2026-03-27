using NaughtyAttributes;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);

    private Rigidbody2D rb;
    private bool wasGrounded;

    [ShowNativeProperty] public bool IsGrounded { get; private set; }
    [ShowNativeProperty] public bool IsAirborne { get; private set; }
    
    // State flags for the controller to consume and reset
    public bool HasJustJumped { get; private set; }
    public bool HasJustLanded { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        CheckGroundedState();
    }

    public void ApplyMovement(float horizontalInput)
    {
        // Direct velocity assignment provides the snappy, immediate response expected in 2D platformers
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public void ExecuteJump()
    {
        if (IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            HasJustJumped = true;
        }
    }

    public void ResetJumpFlag()
    {
        HasJustJumped = false;
    }

    public void ResetLandedFlag()
    {
        HasJustLanded = false;
    }

    private void CheckGroundedState()
    {
        IsGrounded = Physics2D.OverlapBox(groundCheckPoint.position, groundCheckSize, 0f, groundLayer);
        IsAirborne = !IsGrounded;

        if (IsGrounded && !wasGrounded)
        {
            HasJustLanded = true;
        }

        wasGrounded = IsGrounded;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheckPoint.position, groundCheckSize);
        }
    }
}