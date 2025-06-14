using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAithor : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Rigidbody2D myRigidbody;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float acceleration;
    [SerializeField] private float jumpForce;
    [SerializeField] private float maxJumpTime;

    private bool isGrounded;
    private bool isJumping;
    private float jumpTimeCounter;

    [Header("Grounding")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance;
    [SerializeField] private Vector2 boxSizeOffset;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        myRigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        // Check if player is grounded
        CheckGrounded();
        
        // Control the player's movement
        ControlMovement();

        // Control the player's animation
        ControlAnimation();
    }

    private void ControlMovement()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");

        // Horizontal movement
        float targetVelocityX = moveInput * moveSpeed;
        float velocityX = Mathf.Lerp(myRigidbody.velocity.x, targetVelocityX, acceleration * Time.fixedDeltaTime);
        myRigidbody.velocity = new Vector2(velocityX, myRigidbody.velocity.y);

        // Start jump
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpForce);
        }

        // Hold jump for variable height
        if (Input.GetButton("Jump") && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                myRigidbody.velocity = new Vector2(myRigidbody.velocity.x, jumpForce);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        // Stop jump when released
        if (Input.GetButtonUp("Jump"))
        {
            isJumping = false;
        }
    }

    private void CheckGrounded()
    {
        Bounds bounds = boxCollider.bounds;

        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y + boxSizeOffset.y);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, 0.05f); // slightly smaller than actual collider width

        isGrounded = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, groundCheckDistance, groundLayer);
    }

    private void ControlAnimation()
    {
        if (myRigidbody.velocity.x > 0 && spriteRenderer.flipX)
        {
            spriteRenderer.flipX = false;
        }
        else if (myRigidbody.velocity.x < 0 && !spriteRenderer.flipX)
        {
            spriteRenderer.flipX = true;
        }
    }
}
