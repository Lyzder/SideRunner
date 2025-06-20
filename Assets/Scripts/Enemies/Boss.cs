using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;

public class Boss : EnemyBase
{
    public float DeathTimer;
    private SpriteRenderer _renderer;
    public event Action OnDeath;

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RegisterBoss(this);
        _renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void TriggerDeath()
    {
        StartCoroutine(DyingSequence());
    }

    private IEnumerator DyingSequence()
    {
        float elapsed = 0f;
        int flickerInterval = 4;  // flicker every 2 frames
        int frameCounter = 0;

        while (elapsed < DeathTimer)
        {
            if (frameCounter % flickerInterval == 0)
            {
                _renderer.enabled = !_renderer.enabled;
            }

            frameCounter++;
            elapsed += Time.deltaTime;
            yield return null; // wait for next rendered frame
        }

        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        AudioManager.Instance.PlaySFX(explosionSfx);
        OnDeath?.Invoke();
        Destroy(gameObject);
    }
}
