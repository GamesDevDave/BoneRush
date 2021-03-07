using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using static SCR_SwordEnemy_SM;
using static SCR_Archer_SM;
using static SCR_StationaryArcher_SM;
using static SCR_RitualEnemy_SM;

public class ShieldBashCheck : MonoBehaviour
{

    PlayerStats ps;
    public bool shieldBash;
    float gracePeriod; // Grace period for the boss to stop consecutive hits from shield bash
    [SerializeField]
    float gracePeriodReset = .3f;
    public float stunDuration;

    [Header("FMOD Events")]
    [EventRef] [SerializeField] string eventShieldBashCollision;         // Called when shield collides with enemy.

    void Start()
    {
        ps = GetComponentInParent<PlayerStats>();
    }

    void Update()
    {
        if(gracePeriod >= 0)
        {
            gracePeriod -= Time.deltaTime;
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (shieldBash)
        {
            if (other.tag == "Enemy")
            {
                RuntimeManager.PlayOneShot(eventShieldBashCollision, transform.position);

                if(other.GetComponent<SCR_enemyID>().ID == 0)   //sword enemy
                {
                    other.GetComponent<SCR_SwordEnemy_SM>().currentState = SCR_SwordEnemy_SM.State.Stunned;
                }
                else if(other.GetComponent<SCR_enemyID>().ID == 1)   //archer enemy
                {
                    other.GetComponent<SCR_Archer_SM>().currentState = SCR_Archer_SM.State.Stunned;
                }
                else if (other.GetComponent<SCR_enemyID>().ID == 2)   //stationary archer enemy
                {
                    other.GetComponent<SCR_StationaryArcher_SM>().currentState = SCR_StationaryArcher_SM.State.Stunned;
                }
                else if (other.GetComponent<SCR_enemyID>().ID == 3)   //ritual enemy
                {
                    other.GetComponent<SCR_RitualEnemy_SM>().currentState = SCR_RitualEnemy_SM.State.Stunned;
                }
            }
            else if (other.tag == "Boss")
            {
                if(gracePeriod <= 0)
                {
                    BossStats.BossDamage(ps.ShieldDamage, false);
                    gracePeriod = gracePeriodReset;
                }
            }
        }
    }
}
