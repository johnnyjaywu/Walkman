using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Header("Ground Checks")]
    [Tooltip("Minimum Y value of a collision normal to be considered ground. 0.7 is roughly a 45-degree slope.")]
    [SerializeField] private float minGroundNormalY = 0.7f;

    [Tooltip("How far below the feet to check for ground. Creates a magnetic stick for descending slopes/platforms.")]
    [SerializeField] private float groundCheckTolerance = 0.05f;

    [Header("Forgiveness Mechanics")]
    [SerializeField] private float coyoteTime = 0.1f;

    [SerializeField] private float jumpBufferTime = 0.1f;
    [SerializeField] private float jumpCooldown = 0.1f;

    [Header("Impact Mechanics")]
    [Tooltip("The exact duration in seconds the character is forced to hold the squashed impact pose.")]
    [SerializeField] private float landingImpactDuration = 0.15f;

    [Tooltip("The time it takes to get up AFTER the impact hold has finished.")]
    [SerializeField] private float gettingUpBufferTime = 0.25f;

    private const int kMaxContacts = 16;
    private const float kDropOverlapRadius = 0.05f;

    private Rigidbody2D rb;
    private CapsuleCollider2D capsuleCollider;
    private Collider2D currentGroundCollider;

    private readonly HashSet<Collider2D> ignoredPlatforms = new HashSet<Collider2D>();
    private readonly List<Collider2D> platformsToRemove = new List<Collider2D>();

    private readonly RaycastHit2D[] groundHits = new RaycastHit2D[kMaxContacts];
    private readonly Collider2D[] dropBuffer = new Collider2D[kMaxContacts];

    // Platform Tethering State
    private Transform currentPlatform;
    private Vector3 previousPlatformPosition;
    private bool wasGrounded;
    private bool isDropIntentActive;

    private float coyoteTimer;
    private float jumpBufferTimer;
    private float jumpCooldownTimer;
    private float impactTimer;
    private float recoveryTimer;
    private float spawnGraceTimer = 0.2f;

    public bool IsGrounded { get; private set; }
    public bool IsAirborne { get; private set; }
    public bool HasJustJumped { get; private set; }
    public bool HasJustLanded { get; private set; }
    public bool IsFacingRight { get; private set; } = true;

    // The player cannot move if EITHER timer is active
    public bool IsMovementFrozen => impactTimer > 0f || recoveryTimer > 0f;

    // The animator only holds the impact pose while this specific timer is active
    public bool IsHoldingImpact => impactTimer > 0f;

    public void SetGravity(float gravity)
    {
        rb.gravityScale = gravity;
        rb.linearVelocity = Vector2.zero;
    }
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
    }

    private void FixedUpdate()
    {
        UpdateTimers();
        CheckGroundedState();
        StickToMovingPlatform();
        ProcessDropIntent();
        ManageIgnoredPlatforms();
        HandleJumpLogic();
    }

    public void ApplyMovement(float horizontalInput)
    {
        // Use the combined freeze check
        if (IsMovementFrozen)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
        CheckFacingDirection(horizontalInput);
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

    private void UpdateTimers()
    {
        if (spawnGraceTimer > 0f)
        {
            spawnGraceTimer -= Time.fixedDeltaTime;
        }

        // Run the timers sequentially. 
        if (impactTimer > 0f)
        {
            impactTimer -= Time.fixedDeltaTime;

            // The exact frame the impact hold finishes, start the recovery block
            if (impactTimer <= 0f)
            {
                recoveryTimer = gettingUpBufferTime;
            }
        }
        else if (recoveryTimer > 0f)
        {
            recoveryTimer -= Time.fixedDeltaTime;
        }

        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.fixedDeltaTime;
        }
    }

    private void CheckFacingDirection(float horizontalInput)
    {
        if (horizontalInput > 0f && !IsFacingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0f && IsFacingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        IsFacingRight = !IsFacingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    private void ProcessDropIntent()
    {
        // Block platform phasing if the character is frozen from a hard landing
        if (!isDropIntentActive || IsMovementFrozen) return;

        Vector2 capsuleBottom = new Vector2(capsuleCollider.bounds.center.x, capsuleCollider.bounds.min.y);

        int hitCount = Physics2D.OverlapCircle(capsuleBottom, kDropOverlapRadius, ContactFilter2D.noFilter, dropBuffer);
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = dropBuffer[i];

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
        int hitCount = Physics2D.CapsuleCast(
            capsuleCollider.bounds.center,
            capsuleCollider.size,
            capsuleCollider.direction,
            0f,
            Vector2.down,
            ContactFilter2D.noFilter,
            groundHits,
            groundCheckTolerance);

        bool foundGround = false;
        currentGroundCollider = null;

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D col = groundHits[i].collider;

            // Ignore ourselves and any trigger volumes
            if (col == capsuleCollider || col.isTrigger) continue;

            if (groundHits[i].normal.y >= minGroundNormalY)
            {
                foundGround = true;
                currentGroundCollider = col;
                break;
            }
        }

        if (spawnGraceTimer > 0f)
        {
            foundGround = true;
        }

        IsGrounded = foundGround;
        IsAirborne = !IsGrounded;

        if (spawnGraceTimer <= 0f && IsGrounded && !wasGrounded && rb.linearVelocity.y <= 0.1f)
        {
            HasJustLanded = true;
            impactTimer = landingImpactDuration;
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

        // PLATFORM TRACKING
        // If we are standing on a platform, cache its transform to read its movement next frame.
        if (IsGrounded && currentGroundCollider != null && currentGroundCollider.GetComponent<PlatformEffector2D>())
        {
            if (currentPlatform != currentGroundCollider.transform)
            {
                currentPlatform = currentGroundCollider.transform;
                previousPlatformPosition = currentPlatform.position;
            }
        }
        else
        {
            currentPlatform = null;
        }
    }

    private void StickToMovingPlatform()
    {
        // If we are locked onto a platform, calculate exactly how far it moved this frame
        // and physically inject that delta into the player's position.
        if (currentPlatform != null)
        {
            Vector3 platformDelta = currentPlatform.position - previousPlatformPosition;
            rb.position += (Vector2)platformDelta;
            previousPlatformPosition = currentPlatform.position;
        }
    }

    private void HandleJumpLogic()
    {
        jumpBufferTimer -= Time.fixedDeltaTime;

        // Block execution if the character is locked in the landing recovery phase
        if (IsMovementFrozen) return;

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