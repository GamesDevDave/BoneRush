using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackEventTrigger : MonoBehaviour
{
    public void AnimationAttackTrigger(int i)
    {
        Debug.Log("Animation event received with id: " + i);
        if (i == 1)
        {
            GetComponentInParent<SCR_SwordEnemy_SM>().attacking = true;
        }
    }
}
