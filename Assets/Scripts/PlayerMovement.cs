using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] SpriteRenderer sprite;

    [SerializeField] Animator animator;

    [SerializeField] LayerMask groundLayers;

    [Header("Audio")]
    [SerializeField] AudioClip jumpClip;

    [SerializeField] AudioSource audioSource;

    [Header("Settings")]
    [SerializeField] float moveSpeed = 7f;

    [SerializeField] float smoothInputSpeed = 0.2f;

    [SerializeField] float jumpForce = 14f;

    enum MovementState { idle, running, jumping, falling }
    MovementState state = MovementState.idle;

    Rigidbody2D rb;
    BoxCollider2D coll;

    Vector2 curMovementInput;
    Vector2 smoothInputVelocity;

    bool isMoving = false;

    bool canMove = true;

    private void OnEnable()
    {
        EventSystemNew.Subscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
    }

    private void OnDisable()
    {
        EventSystemNew.Unsubscribe(Event_Type.LEVEL_COMPLETED, LevelCompleted);
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
    }

    private void Update()
    {
        UpdateAnimationState();
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void LevelCompleted()
    {
        canMove = false;
    }

    private void UpdateAnimationState()
    {
        if (Mathf.Abs(curMovementInput.x) > 0.1f)
        {
            state = MovementState.running;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > .1f)
        {
            state = MovementState.jumping;
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }

        animator.SetInteger("State", (int)state);
    }

    private void Move()
    {
        if (!isMoving)
        {
            curMovementInput = Vector2.SmoothDamp(curMovementInput, Vector2.zero, ref smoothInputVelocity, smoothInputSpeed);
        }

        rb.velocity = new Vector2(curMovementInput.x * moveSpeed, rb.velocity.y);

        if (curMovementInput.x < 0f)
        {
            sprite.flipX = true;
        }
        else if (curMovementInput.x > 0f)
        {
            sprite.flipX = false;
        }
    }

    public void OnMove(InputAction.CallbackContext _context)
    {
        if (_context.phase == InputActionPhase.Started && canMove)
        {
            isMoving = true;

            curMovementInput = _context.ReadValue<Vector2>();
        }
        else if (_context.phase == InputActionPhase.Canceled)
        {
            isMoving = false;
        }
    }

    public void OnJump(InputAction.CallbackContext _context)
    {
        if (_context.phase == InputActionPhase.Started && IsGrounded() && canMove)
        {
            audioSource.PlayOneShot(jumpClip);

            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, groundLayers);
    }
}
