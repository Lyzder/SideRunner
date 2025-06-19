using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Medkit : PickupBase
{
    public short hpRecover;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void PickEffect(GameObject player)
    {
        player.GetComponent<PlayerController>().Heal(hpRecover);
        player.GetComponent<PlayerController>().PlayPickup();
        GameManager.Instance.AddScore(base.score);
        AudioManager.Instance.PlaySFX(pickSfx);
    }
}
