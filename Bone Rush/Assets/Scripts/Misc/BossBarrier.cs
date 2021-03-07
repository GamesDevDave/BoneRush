using UnityEngine;
using System.Collections;

public class BossBarrier : MonoBehaviour
{
    private bool active = false;
    private Collider boxCollider;
    private float delayTime = 2f;

    private void Start()
    {
        boxCollider = GetComponent<Collider>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!active)
        {
            active = true;
            boxCollider.enabled = true;      //turns on collider so player can't exit boss room   
        }
    }
}