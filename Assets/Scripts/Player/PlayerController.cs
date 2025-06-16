using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpSpeed;
    private short moveDirection;
    private float moveInput;
    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;
    private bool jumpQueued = false;
    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        moveDirection = 1;
        // Initialize Input Actions
        inputActions = new InputSystem_Actions();
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Jump.performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJumpPerformed;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        if (jumpQueued)
        {
            rb.AddForce(new Vector2(0, jumpSpeed), ForceMode2D.Impulse);
            jumpQueued = false;
        }
        Move();
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
    }

    private void Move()
    {
        rb.velocity = new Vector2(moveSpeed * moveDirection, rb.velocity.y);
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<float>();
        if (moveInput > 0)
            moveDirection = 1;
        else if (moveInput < 0)
            moveDirection = -1;
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = 0;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (isGrounded) {
            jumpQueued = true;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Solid"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 normal = contact.normal;

            if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
            {
                Debug.Log("Side collision detected with Wall");
                moveDirection *= -1;
            }
        }
    }

    // For debug visualization
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}
