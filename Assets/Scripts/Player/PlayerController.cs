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
    [SerializeField] private float bounceSpeed;
    private short moveDirection;
    private float moveInput;
    [Header("Ground check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float coyoteTime;
    private float coyoteTimer;
    private bool isGrounded;
    private bool jumpQueued;
    private bool jumped;
    [Header("Sats")]
    [SerializeField] private short hp;
    [SerializeField] private short maxHp;
    public short ammo;
    [SerializeField] private float shotCooldown;
    private float shotCdTimer;
    [Header("Interactions")]
    [SerializeField] private float damageFrames;
    [SerializeField] private float iFrames;
    [SerializeField] private bool invincible;
    [SerializeField] private float damageRecoilHorizontal;
    [SerializeField] private float damageRecoilVertical;
    [SerializeField] private BoxCollider2D stompHitbox;
    private float iFramesTimer;
    private float damageFramesTimer;
    // Components
    private Rigidbody2D rb;
    private InputSystem_Actions inputActions;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    // Player State
    public bool isAlive { get; private set; }
    public enum States : ushort
    {
        Default = 0,
        Damage = 1,
        Firing = 2,
    }
    public States playerState {  get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        moveDirection = 1;
        coyoteTimer = 0;
        jumpQueued = false;
        jumped =  false;
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
        inputActions.Player.Attack.performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJumpPerformed;
        inputActions.Player.Attack.performed -= OnAttackPerformed;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isGrounded)
            RunCoyoteTimer();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (jumpQueued)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
            rb.AddForce(new Vector2(0, jumpSpeed), ForceMode2D.Impulse);
            jumpQueued = false;
            jumped = true;
        }
        Move();
        CheckGrounded();
    }

    private void Move()
    {
        if (playerState == States.Damage)
            return;
        rb.velocity = new Vector2(moveSpeed * moveDirection, rb.velocity.y);
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        if (playerState == States.Damage)
            return;
        moveInput = ctx.ReadValue<float>();
        if (moveInput > 0)
        {
            moveDirection = 1;
            FlipSprite();
        }
        else if (moveInput < 0)
        {
            moveDirection = -1;
            FlipSprite();
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        moveInput = 0;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        if (playerState == States.Damage)
            return;
        if (isGrounded || (coyoteTimer < coyoteTime && !jumped)) {
            jumpQueued = true;
        }
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (playerState == States.Damage)
            return;
        if (ammo <= 0 || shotCdTimer > 0)
            return;
        //TODO
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
                ChangeDirection();
            }
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            //TODO
            TakeDamage();
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        if (isGrounded)
        {
            coyoteTimer = 0;
            jumped = false;
            stompHitbox.enabled = false;
        }
        else
        {
            stompHitbox.enabled = true;
        }
    }

    private void RunCoyoteTimer()
    {
        if (coyoteTimer < coyoteTime)
        {
            coyoteTimer += Time.deltaTime;
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

    private void ChangeDirection()
    {
        moveDirection *= -1;
        FlipSprite();
    }

    private void FlipSprite()
    {
        if (moveDirection >= 0)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("HorizontalSpeed", moveSpeed);
        animator.SetFloat("VerticalSpeed", rb.velocity.y);
    }

    public void TakeDamage()
    {
        if (invincible)
            return;
        hp -= 1;
        playerState = States.Damage;
        //AudioManager.Instance.PlaySFX(hurtSfx);
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        if (hp <= 0)
        {
            isAlive = false;
            animator.SetBool("IsDead", true);
            //DisableControl();
            moveInput = 0;
            //StartCoroutine(DeadSequence());
            DamageRecoil(1.5f);
        }
        else
        {
            // TODO jugador sigue vivo
            animator.ResetTrigger("IsHurt");
            animator.SetTrigger("IsHurt");
            invincible = true;
            StartCoroutine(InvulnerableTimer());
            StartCoroutine(DamageTimer());
            DamageRecoil(1);
        }
    }

    private IEnumerator InvulnerableTimer()
    {
        float elapsed = 0f;
        int flickerInterval = 4;  // flicker every 2 frames
        int frameCounter = 0;

        while (elapsed < iFrames)
        {
            if (frameCounter % flickerInterval == 0)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            frameCounter++;
            elapsed += Time.deltaTime;
            yield return null; // wait for next rendered frame
        }

        spriteRenderer.enabled = true;
        invincible = false;
        gameObject.layer = LayerMask.NameToLayer("PlayerDefault");
    }

    private IEnumerator DamageTimer()
    {
        damageFramesTimer = 0;
        while (damageFramesTimer < damageFrames)
        {
            damageFramesTimer += Time.deltaTime;
            yield return null;
        }
        playerState = States.Default;
        animator.ResetTrigger("IsRecovered");
        animator.SetTrigger("IsRecovered");
        ChangeDirection();
    }

    private void DamageRecoil(float mult)
    {
        float horizontalRecoil = damageRecoilHorizontal * moveDirection * -1;
        rb.AddForce(new Vector2(horizontalRecoil * mult, damageRecoilVertical * mult), ForceMode2D.Impulse);
    }

    private void AttackCooldown()
    {
        if (shotCdTimer == 0)
            return;
        shotCdTimer -= Time.deltaTime;
    }

    private void StompBounce()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0, bounceSpeed), ForceMode2D.Impulse);
    }
}
