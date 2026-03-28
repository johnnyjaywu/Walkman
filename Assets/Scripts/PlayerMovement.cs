using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;
    
    [Tooltip("Minimum Y value of a collision normal to be considered ground. 0.7 is roughly a 45-degree slope.")]
    [SerializeField] private float minGroundNormalY = 0.7f;

    [Header("Forgiveness Mechanics")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float jumpCooldown = 0.1f;

    private const int kMaxContacts = 16;
    private const float kDropOverlapRadius = 0.05f;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Collider2D currentGroundCollider;
    
    private readonly HashSet<Collider2D> ignoredPlatforms = new HashSet<Collider2D>();
    private readonly List<Collider2D> platformsToRemove = new List<Collider2D>();
    
    private readonly ContactPoint2D[] contactBuffer = new ContactPoint2D[kMaxContacts];
    private readonly Collider2D[] dropBuffer = new Collider2D[kMaxContacts];
    
    private bool wasGrounded;
    private bool isDropIntentActive;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private float jumpCooldownTimer;

    public bool IsGrounded { get; private set; }
    public bool IsAirborne { get; private set; }
    public bool HasJustJumped { get; private set; }
    public bool HasJustLanded { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        CheckGroundedState();
        ProcessDropIntent();
        ManageIgnoredPlatforms();
        HandleJumpLogic();
    }

    public void ApplyMovement(float horizontalInput)
    {
        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    public void BufferJumpInput()
    {
        jumpBufferTimer = jumpBufferTime;
    }

    public void CancelJump()
    {
        if (rb.linearVelocity.y > 0f && !IsGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    public void SetDropIntent(bool isIntendingToDrop)
    {
        isDropIntentActive = isIntendingToDrop;
    }

    public void ResetJumpFlag()
    {
        HasJustJumped = false;
    }

    public void ResetLandedFlag()
    {
        HasJustLanded = false;
    }

    private void ProcessDropIntent()
    {
        if (!isDropIntentActive) return;

        // Forcefully query the exact bottom tip of the capsule.
        // This bypasses the Box2D contact cache, guaranteeing we find the platform.
        Vector2 capsuleBottom = new Vector2(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y);
        
        int hitCount = Physics2D.OverlapCircleNonAlloc(capsuleBottom, kDropOverlapRadius, dropBuffer);
        
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = dropBuffer[i];
            
            // Ignore self-intersections
            if (col == capsuleCollider) continue;

            if (col.GetComponent<PlatformEffector2D>() != null && !ignoredPlatforms.Contains(col))
            {
                Physics2D.IgnoreCollision(capsuleCollider, col, true);
                ignoredPlatforms.Add(col);
            }
        }
    }

    private void ManageIgnoredPlatforms()
    {
        if (ignoredPlatforms.Count == 0) return;

        platformsToRemove.Clear();

        float playerTopY = capsuleCollider.bounds.max.y;

        foreach (Collider2D platform in ignoredPlatforms)
        {
            // Instead of AABB intersections, we use strict vertical thresholds.
            // If the highest point of the player's head is mathematically lower than 
            // the lowest point of the platform, the player has completely passed through it.
            if (playerTopY < platform.bounds.min.y)
            {
                Physics2D.IgnoreCollision(capsuleCollider, platform, false);
                platformsToRemove.Add(platform);
            }
        }

        foreach (Collider2D platform in platformsToRemove)
        {
            ignoredPlatforms.Remove(platform);
        }
    }

    private void CheckGroundedState()
    {
        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.fixedDeltaTime;
        }

        int contactCount = capsuleCollider.GetContacts(contactBuffer);
        
        bool foundGround = false;
        currentGroundCollider = null;

        for (int i = 0; i < contactCount; i++)
        {
            if (contactBuffer[i].normal.y >= minGroundNormalY)
            {
                foundGround = true;
                currentGroundCollider = contactBuffer[i].collider;
                break;
            }
        }

        IsGrounded = foundGround;
        IsAirborne = !IsGrounded;

        if (IsGrounded && !wasGrounded)
        {
            HasJustLanded = true;
        }

        wasGrounded = IsGrounded;

        if (IsGrounded)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.fixedDeltaTime;
        }
    }

    private void HandleJumpLogic()
    {
        jumpBufferTimer -= Time.fixedDeltaTime;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f && jumpCooldownTimer <= 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            HasJustJumped = true;

            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
            jumpCooldownTimer = jumpCooldown;
        }
    }
}