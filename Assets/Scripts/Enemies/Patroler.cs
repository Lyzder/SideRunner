using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Patroler : EnemyBase
{


    protected override void TriggerDeath()
    {
        Destroy(gameObject);
    }
}
