using System;
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
    [Header("Shooting")]
    [SerializeField] GameObject bulletPrefab;
    public Vector3 bulletSpawnOffset;
    [SerializeField] private float shotCooldown;
    private float shotCdTimer;
    [Header("Interactions")]
    [SerializeField] private float damageFrames;
    [SerializeField] private float iFrames;
    [SerializeField] private bool invincible;
    [SerializeField] private float damageRecoilHorizontal;
    [SerializeField] private float damageRecoilVertical;
    [SerializeField] private BoxCollider2D stompHitbox;
    [SerializeField] private float deadTime;
    private float deadTimer;
    private bool ignoreCollision;
    private float damageFramesTimer;
    [Header("Effects")]
    [SerializeField] GameObject jumpEffect;
    [SerializeField] GameObject shootEffect;
    [SerializeField] GameObject pickupEffect;
    public Vector3 jumpEffectOffset;
    public Vector3 shootEffectOffset;
    [Header("Sound Effects")]
    [SerializeField] private AudioClip jumpSfx;
    [SerializeField] private AudioClip stompSfx;
    [SerializeField] private AudioClip shootSfx;
    [SerializeField] private AudioClip hurtSfx;
    [SerializeField] private AudioClip noAmmoSfx;
    [SerializeField] private AudioClip reloadSfx;
    [SerializeField] private AudioClip deadSfx;
    // Components
    private Rigidbody2D rb;
    private BoxCollider2D collider2d;
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
    // Events
    public event Action<short> OnHealthChanged;
    public event Action<short> OnAmmoChanged;
    public event Action OnDeath;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        collider2d = GetComponent<BoxCollider2D>();
        moveDirection = 1;
        coyoteTimer = 0;
        jumpQueued = false;
        jumped =  false;
        isAlive = true;
        // Initialize Input Actions
        inputActions = new InputSystem_Actions();
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RegisterPlayer(this);
        OnHealthChanged?.Invoke(hp);
        OnAmmoChanged?.Invoke(ammo);
    }

    private void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Move.performed += OnMovePerformed;
        inputActions.Player.Move.canceled += OnMoveCanceled;
        inputActions.Player.Jump.performed += OnJumpPerformed;
        inputActions.Player.Attack.performed += OnAttackPerformed;
        GameManager.Instance.OnWinning += DisableControls;
    }

    private void OnDisable()
    {
        inputActions.Disable();
        inputActions.Player.Move.performed -= OnMovePerformed;
        inputActions.Player.Move.canceled -= OnMoveCanceled;
        inputActions.Player.Jump.performed -= OnJumpPerformed;
        inputActions.Player.Attack.performed -= OnAttackPerformed;
        GameManager.Instance.OnWinning -= DisableControls;
    }

    // Update is called once per frame
    void Update()
    {
        if (!isAlive)
            return;
        if (!isGrounded)
            RunCoyoteTimer();
        if (shotCdTimer > 0)
            AttackCooldown();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (!isAlive)
            return;
        if (jumpQueued)
        {
            Jump();
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

    private void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0, jumpSpeed), ForceMode2D.Impulse);
        jumpQueued = false;
        jumped = true;
        Instantiate(jumpEffect, groundCheck.position + jumpEffectOffset, Quaternion.identity);
        AudioManager.Instance.PlaySFX(jumpSfx);
    }

    private void Shoot()
    {
        Bullet bullet;
        GameObject effect;
        // Spawn bullet
        Vector3 bulletOffset = new Vector3(bulletSpawnOffset.x * moveDirection, bulletSpawnOffset.y, bulletSpawnOffset.z);
        bullet = Instantiate(bulletPrefab, transform.position + bulletOffset, Quaternion.identity).GetComponent<Bullet>();
        bullet.SetDirection(moveDirection);
        shotCdTimer = shotCooldown;
        ammo -= 1;
        OnAmmoChanged?.Invoke(ammo);
        // Spawn Effect
        Vector3 effectOffset = new Vector3(shootEffectOffset.x * moveDirection, shootEffectOffset.y, shootEffectOffset.z);
        effect = Instantiate(shootEffect);
        effect.transform.SetParent(transform);
        effect.transform.localPosition = effectOffset;
        effect.transform.localRotation = Quaternion.identity;
        if (moveDirection > 0)
        {
            effect.GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>().flip = new Vector3(1f, 0);
        }
        else if (moveDirection < 0)
        {
            effect.GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>().flip = new Vector3(0f, 0);
        }
        AudioManager.Instance.PlaySFX(shootSfx);
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
        if (shotCdTimer > 0)
            return;
        if (ammo <= 0)
        {
            AudioManager.Instance.PlaySFX(noAmmoSfx);
            return;
        }
        Shoot();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Solid"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            Vector2 normal = contact.normal;

            if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
            {
                ChangeDirection();
                TemporarilyIgnoreCollision(0.1f);
            }
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            //TODO
            TakeDamage();
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (ignoreCollision)
            return;
        if (collision.gameObject.CompareTag("Solid"))
        {
            ContactPoint2D[] contacts = collision.contacts;
            Vector2 normal;
            foreach (ContactPoint2D contactPoint in contacts)
            {
                normal = contactPoint.normal;

                if (Mathf.Abs(normal.x) > Mathf.Abs(normal.y))
                {
                    ChangeDirection();
                    TemporarilyIgnoreCollision(0.1f);
                    return;
                }
            }
        }
    }

    public void TemporarilyIgnoreCollision(float duration)
    {
        StartCoroutine(IgnoreCollisionRoutine(duration));
    }

    private IEnumerator IgnoreCollisionRoutine(float duration)
    {
        ignoreCollision = true;
        yield return new WaitForSeconds(duration);
        ignoreCollision = false;
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);
        if (isGrounded)
        {
            coyoteTimer = 0;
            jumped = false;
            stompHitbox.enabled = false;
            collider2d.size = new Vector2(0.8f, 0.89f);
        }
        else if (playerState != States.Damage)
        {
            stompHitbox.enabled = true;
            collider2d.size = new Vector2(0.8f, 0.65f);
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

    public void ChangeDirection()
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
        OnHealthChanged?.Invoke(hp);
        gameObject.layer = LayerMask.NameToLayer("PlayerInvincible");
        AudioManager.Instance.PlaySFX(hurtSfx);
        if (hp <= 0)
        {
            isAlive = false;
            animator.SetBool("IsDead", true);
            moveInput = 0;
            StartCoroutine(DeadSequence());
            DamageRecoil(2f);
            AudioManager.Instance.PlaySFX(deadSfx);
            DisableControls();
        }
        else
        {
            // TODO jugador sigue vivo
            animator.ResetTrigger("IsHurt");
            animator.SetTrigger("IsHurt");
            invincible = true;
            StartCoroutine(InvulnerableTimer());
            StartCoroutine(DamageTimer());
            DamageRecoil(1.5f);
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
        rb.velocity = Vector2.zero;
        rb.AddForce(new Vector2(horizontalRecoil * mult, damageRecoilVertical * mult), ForceMode2D.Impulse);
    }

    private void AttackCooldown()
    {
        shotCdTimer -= Time.deltaTime;
        if (shotCdTimer <= 0)
        {
            if (ammo > 0)
                AudioManager.Instance.PlaySFX(reloadSfx);
            else
                AudioManager.Instance.PlaySFX(noAmmoSfx);
        }
    }

    private void StompBounce()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.AddForce(new Vector2(0, bounceSpeed), ForceMode2D.Impulse);
        AudioManager.Instance.PlaySFX(stompSfx);
    }

    public void OnStomp()
    {
        StompBounce();
    }

    public void Heal(short amount)
    {
        hp += amount;
        if (hp > maxHp)
            hp = maxHp;
        OnHealthChanged?.Invoke(hp);
    }

    public void Reload(short amount)
    {
        ammo += amount;
        OnAmmoChanged?.Invoke(ammo);
    }

    public void PlayPickup()
    {
        Instantiate(pickupEffect, transform);
    }

    private IEnumerator DeadSequence()
    {
        deadTimer = 0;
        while (deadTimer < deadTime)
        {
            deadTimer += Time.deltaTime;
            yield return null;
        }
        OnDeath?.Invoke();
    }

    public void SetHp(short hp)
    {
        this.hp = hp;
        OnHealthChanged?.Invoke(hp);
    }

    public void SetAmmo(short ammo)
    {
        this.ammo = ammo;
        OnAmmoChanged?.Invoke(ammo);
    }

    public short GetHp()
    {
        return hp;
    }

    public short GetAmmo()
    {
        return ammo;
    }

    private void DisableControls()
    {
        inputActions.Disable();
    }
}
