using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PickupBase : MonoBehaviour
{
    public int score;
    [SerializeField] protected AudioClip pickSfx;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PickEffect(other.gameObject);
            Destroy(gameObject);
        }
    }

    protected abstract void PickEffect(GameObject player);
}
