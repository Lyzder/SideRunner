using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed;
    private short moveDirection;
    [SerializeField] int damage;
    [Header("Effect")]
    [SerializeField] GameObject impactEffect;
    // Components
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rb.velocity = new Vector2(moveSpeed * moveDirection, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        // Check and delete bullet if out of camera
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);

        bool isOutside =
            viewportPos.x < 0 || viewportPos.x > 1 ||
            viewportPos.y < 0 || viewportPos.y > 1;

        if (isOutside)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 hitPoint = contact.point;
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyBase enemy = collision.gameObject.GetComponent<EnemyBase>();
            enemy.TakeDamage(damage);
        }
        PlayImpact(hitPoint);
        Destroy(gameObject);
    }

    private void PlayImpact(Vector2 position)
    {
        Instantiate(impactEffect, position, Quaternion.identity);
    }

    public void SetDirection(short direction)
    {
        moveDirection = direction;
        if (direction > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if(direction < 0)
        {
            spriteRenderer.flipX = true;
        }
    }
}
