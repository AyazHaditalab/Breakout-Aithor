using UnityEngine;

public class PlayerAithor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Rigidbody2D rb;

    [Header("Movement")]
    [SerializeField] private float moveSpeed; // target horizontal speed
    [SerializeField] private float acceleration; // ground acceleration
    [SerializeField] private float airAcceleration; // air acceleration
    [SerializeField] private float groundFriction; // deceleration when no input (ground)

    [Header("Jump")]
    [SerializeField] private float jumpForce; // initial jump velocity
    [SerializeField] private float maxJumpTime; // extra time to hold for higher jump
    [SerializeField] private float cutJumpFactor; // short hop trim

    [Header("Feel")]
    [SerializeField] private float coyoteTime; // jump shortly after leaving ledge
    [SerializeField] private float jumpBufferTime;// queue jump just before landing

    [Header("Gravity")]
    [SerializeField] private float riseGravityMult; // slightly lighter going up
    [SerializeField] private float fallGravityMult; // heavier coming down
    [SerializeField] private float maxFallSpeed; // clamp terminal velocity

    // Input
    private float moveInput;
    private bool jumpPressedThisFrame;
    private bool jumpHeld;

    // Jump state
    private bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;
    private float coyoteTimer;
    private float bufferTimer;
    private bool ceilingBlocked;

    [Header("Grounding/Ceiling")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private Vector2 boxSizeOffset;
    [SerializeField] private float ceilingCheckDistance;

    private void Start() {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update() {
        moveInput = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) { bufferTimer = jumpBufferTime; }
        jumpHeld = Input.GetButton("Jump");

        CheckGrounded();
        CheckCeiling();

        if (isGrounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;

        if (bufferTimer > 0f) bufferTimer -= Time.deltaTime;

        // Queue a jump if valid
        if (bufferTimer > 0f && coyoteTimer > 0f) {
            jumpPressedThisFrame = true;
            bufferTimer = 0f;
        }

        // Sprite facing
        ControlFlip();
    }

    private void FixedUpdate() {
        float dt = Time.fixedDeltaTime;

        // Horizontal
        float targetVX = moveInput * moveSpeed;
        float accel = isGrounded ? acceleration : airAcceleration;

        float vx = Mathf.MoveTowards(rb.velocity.x, targetVX, accel * dt);

        // Ground friction when no input
        if (isGrounded && Mathf.Approximately(moveInput, 0f))
            vx = Mathf.MoveTowards(vx, 0f, groundFriction * dt);

        if (ceilingBlocked)
        {
            isJumping = false;
            jumpTimeCounter = 0f;
            jumpPressedThisFrame = false;
        }

        // Jump start, hold, cut
        if (jumpPressedThisFrame) {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            rb.velocity = new Vector2(vx, jumpForce);
            coyoteTimer = 0f;
            jumpPressedThisFrame = false;
        }
        else {
            if (isJumping && jumpHeld && jumpTimeCounter > 0f) {
                rb.velocity = new Vector2(vx, jumpForce);
                jumpTimeCounter -= dt;
            }
            else {
                isJumping = false;

                if (!jumpHeld && rb.velocity.y > 0f)
                    rb.velocity = new Vector2(vx, rb.velocity.y * (1f - cutJumpFactor));
                else
                    rb.velocity = new Vector2(vx, rb.velocity.y);
            }
        }

        // Gravity, terminal velocity
        float g = Physics2D.gravity.y * rb.gravityScale;
        if (rb.velocity.y > 0.01f && !jumpHeld)
            rb.velocity += Vector2.up * g * (riseGravityMult - 1f) * dt;
        else if (rb.velocity.y < -0.01f)
            rb.velocity += Vector2.up * g * (fallGravityMult - 1f) * dt;

        if (rb.velocity.y < maxFallSpeed)
            rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);
    }

    private void CheckGrounded() {
        Bounds b = boxCollider.bounds;
        Vector2 center = new Vector2(b.center.x, b.min.y + boxSizeOffset.y);
        Vector2 size = new Vector2(b.size.x * 0.9f, 0.05f);
        isGrounded = Physics2D.BoxCast(center, size, 0f, Vector2.down, groundCheckDistance, groundLayer);
    }

    private void CheckCeiling() {
        Bounds b = boxCollider.bounds;
        Vector2 center = new Vector2(b.center.x, b.max.y + 0.01f);
        Vector2 size = new Vector2(b.size.x * 0.9f, 0.05f);
        bool hit = Physics2D.BoxCast(center, size, 0f, Vector2.up, ceilingCheckDistance, groundLayer);

        ceilingBlocked = hit;

        if (hit && rb.velocity.y > 0f) {
            // cancel the current jump
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            isJumping = false;
            jumpTimeCounter = 0f;
        }
    }

    private void ControlFlip() {
        if (Mathf.Abs(moveInput) > 0.1f) {
            spriteRenderer.flipX = moveInput < 0;
        }
    }
    
    // Ground/ceiling check debugging
    private void OnDrawGizmosSelected() {
        if (!TryGetComponent(out BoxCollider2D bc)) return;
        Bounds b = bc.bounds;

        // Ground cast box (green)
        Vector2 gCenter = new Vector2(b.center.x, b.min.y + boxSizeOffset.y);
        Vector2 gSize = new Vector2(b.size.x * 0.9f, 0.05f);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(gCenter + Vector2.down * groundCheckDistance * 0.5f,
            new Vector3(gSize.x, gSize.y + groundCheckDistance, 0f));

        // Ceiling cast box (red)
        Vector2 cCenter = new Vector2(b.center.x, b.max.y + 0.01f);
        Vector2 cSize = new Vector2(b.size.x * 0.9f, 0.05f);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(cCenter + Vector2.up * ceilingCheckDistance * 0.5f,
            new Vector3(cSize.x, cSize.y + ceilingCheckDistance, 0f));
    }
}