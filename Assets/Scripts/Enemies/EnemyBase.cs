using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Playables;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int hp;
    [SerializeField] private int score;
    public float moveSpeed;
    public float chaseSpeed;
    [Header("Patroling")]
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] BoxCollider2D patrolArea;
    public float checkRadius;
    public float idleTime;
    public float attackCooldown;
    private Vector3 targetPosition;
    private int currentPoint;
    private float waitTimer;
    [Header("States")]
    [SerializeField] private float iFrames;
    [SerializeField] private bool invincible;
    private bool isChasing;
    private bool isAlive;
    private short moveDirection;
    private enum EnemyStates { Patrolling, Chasing, Idle, Damage, Cooldown}
    private EnemyStates enemyState;
    [Header("Effects")]
    [SerializeField] protected GameObject explosionEffect;
    [Header("Sound Effects")]
    [SerializeField] protected AudioClip explosionSfx;
    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveDirection = -1;
        enemyState = EnemyStates.Idle;
        waitTimer = 0;
        isChasing = false;
        isAlive = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (!isAlive)
            return;
        switch (enemyState)
        {
            case EnemyStates.Idle:
                WaitIdle();
                StartChasing();
                break;
            case EnemyStates.Patrolling:
                Move();
                CheckDestination();
                StartChasing();
                break;
            case EnemyStates.Chasing:
                UpdatChaseTarget();
                Move();
                CheckChaseDestination();
                break;
            case EnemyStates.Cooldown:
                WaitCooldown();
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        if (invincible)
            return;
        hp -= damage;
        enemyState = EnemyStates.Damage;
        //AudioManager.Instance.PlaySFX(hurtSfx);
        gameObject.layer = LayerMask.NameToLayer("EnemiesInvincible");
        if (hp <= 0)
        {
            isAlive = false;
            animator.SetBool("IsDead", true);
            GameManager.Instance.AddScore(score);
            TriggerDeath();
        }
        else
        {
            animator.SetBool("IsHurt", true);
            invincible = true;
            enemyState = EnemyStates.Damage;
            rb.velocity = Vector3.zero;
            StartCoroutine(InvulnerableTimer());
        }
    }

    protected abstract void TriggerDeath();

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
        gameObject.layer = LayerMask.NameToLayer("Enemies");
        enemyState = EnemyStates.Patrolling;
        animator.SetBool("IsHurt", false);
        StartChasing();
    }

    private void FlipSprite()
    {
        if (moveDirection >= 0)
            spriteRenderer.flipX = false;
        else
            spriteRenderer.flipX = true;
    }

    public void SetChasing(bool chasing)
    {
        isChasing = chasing;
    }

    private void StartChasing()
    {
        if (isChasing)
            enemyState = EnemyStates.Chasing;
    }

    private void Move()
    {
        float speed = enemyState == EnemyStates.Chasing ? chaseSpeed : moveSpeed;
        rb.velocity = new Vector2(speed * moveDirection, rb.velocity.y);
    }

    private void SetTargetDestination(Vector3 destination)
    {
        targetPosition = destination;
        if ((transform.position.x  - targetPosition.x) < 0)
        {
            moveDirection = 1;
        }
        else
        {
            moveDirection = -1;
        }
        FlipSprite();
    }

    private void GetPatrolPoint()
    {
        currentPoint = (currentPoint + 1) % patrolPoints.Length;
        SetTargetDestination(patrolPoints[currentPoint].position);
    }

    private void WaitIdle()
    {
        if (waitTimer <= 0)
        {
            GetPatrolPoint();
            enemyState = EnemyStates.Patrolling;
        }
        else
            waitTimer -= Time.deltaTime;
    }

    private void WaitCooldown()
    {
        if (waitTimer <= 0)
        {
            if (enemyState != EnemyStates.Chasing)
            {
                GetPatrolPoint();
                enemyState = EnemyStates.Patrolling;
            }
        }
        else
        {
            waitTimer -= Time.deltaTime;
        }
    }

    private void CheckDestination()
    {
        if (math.abs(transform.position.x - targetPosition.x) > checkRadius)
            return;
        enemyState = EnemyStates.Idle;
        rb.velocity = Vector3.zero;
        waitTimer = idleTime;
    }

    private void UpdatChaseTarget()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;
        SetTargetDestination(player.transform.position);
    }

    private void CheckChaseDestination()
    {
        if (!isChasing)
        {
            enemyState = EnemyStates.Idle;
            waitTimer = idleTime;
            rb.velocity = Vector2.zero;
            return;
        }
        if (math.abs(transform.position.x - targetPosition.x) > 0.3)
            return;
        enemyState = EnemyStates.Cooldown;
        rb.velocity = Vector3.zero;
        waitTimer = attackCooldown;
    }

    private void UpdateAnimator()
    {
        animator.SetFloat("Speed", math.abs(rb.velocity.x));
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            enemyState = EnemyStates.Cooldown;
            rb.velocity = Vector3.zero;
            waitTimer = attackCooldown;
        }
    }
}
