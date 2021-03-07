using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_PickupInteraction : MonoBehaviour
{
    [Header("References.")]
    [SerializeField] GameObject GameManagerObj;
    PlayerStats playerStats;
    bool canHeal = true;

    [Header("Conditioning.")]
    public SCR_ScriptablePickUps currentPickup;

    void Start()
    {
        GameManagerObj = GameObject.FindGameObjectWithTag("GameManager");
        playerStats = GameManagerObj.GetComponent<PlayerStats>();
    }


    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentPickup.effectsHealth && canHeal)
        {
            
			canHeal = false;
			playerStats.PlayerHealing();            
            Destroy(gameObject);
            // Debug.Log("Healing player by " + currentPickup.valueGiven + " HP");
        }

        else if (collision.gameObject.CompareTag("Player") && currentPickup.effectsStamina && canHeal)
        {
            canHeal = false;
            playerStats.PlayerStaminaIncrease();
            Destroy(gameObject);
            // Debug.Log("Healing player Stamina by " + currentPickup.valueGiven + " Stamina Points");
        }
    }
}
