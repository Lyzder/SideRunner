using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : EnemyBase
{
    public event Action OnDeath;
    public float DeathTimer;
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.RegisterBoss(this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void TriggerDeath()
    {

    }
}
