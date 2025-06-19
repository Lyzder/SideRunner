using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroler : EnemyBase
{


    protected override void TriggerDeath()
    {
        GameObject effect;
        effect = Instantiate(base.explosionEffect, transform.position, Quaternion.identity);
        effect.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
        AudioManager.Instance.PlaySFX(explosionSfx);
        Destroy(gameObject);
    }
}
