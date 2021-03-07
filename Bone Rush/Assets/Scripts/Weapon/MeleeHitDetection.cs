using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using static SCR_Boss_SM;

public class MeleeHitDetection : MonoBehaviour
{
    SCR_Boss_SM boss;
    SCR_SwordEnemy_SM enemy;
    PlayerStaminaBar stam;
    PlayerAnimationController swordAttackScript;

    // call entity functions
    PlayerStats playerHealth;
    EnemyStats enemyHealth;

    GameObject GameManagerObj;

    public bool isCombo;
    
    [SerializeField]
    public int damageToDo = 10; // The damage that will be done by the object the script is attatched to
    float playerGracePeriod; // Stops consecutive hits against the player
    protected float bossGracePeriod; // Stops consecutive hits against the boss
    public float enemyGracePeriod;
    float gracePeriodReset = .85f;
    bool doHeavy; // This is strictly used by the player
    public bool groundPoundAnimFinished;

    //Player damage
    public int PlayerDamage;
    public int PlayerHeavyDamage;

    //Enemy damage
    int EnemyDamage;

    //Boss damage
    float BossDamage;

    // FMOD:
    [EventRef] [SerializeField] private string eventDamaged;            // Played when player takes damage
    [EventRef] [SerializeField] private string eventSwordCollision;     // Played when player's sword hits enemy

    // Start is called before the first frame update
    void Start()
    {
        GameManagerObj = GameObject.FindGameObjectWithTag("GameManager");
        // Initializing script components
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;

        //ph = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        if (transform.root.GetChild(0).CompareTag("Player"))
        {
            swordAttackScript = GetComponentInParent<PlayerAnimationController>();
            playerHealth = GameManagerObj.GetComponent<PlayerStats>();
            GetPlayerValues();
        }
        else if (!transform.parent.parent.CompareTag("Boss"))
        {
            playerHealth = GameManagerObj.GetComponent<PlayerStats>();
            enemyHealth = GetComponentInParent<EnemyStats>();
            GetEnemyValues();
        }

        if (!transform.root.GetChild(0).CompareTag("Player"))
        {
            swordAttackScript = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerAnimationController>();
        }

        if (sceneName == "BOSS_BLOCKOUT")
        {
            if (transform.parent.parent.CompareTag("Boss"))
            {
                playerHealth = GameManagerObj.GetComponent<PlayerStats>();
                boss = GetComponentInParent<SCR_Boss_SM>();
                GetBossValues();
            }
            else
            {
                boss = GameObject.FindGameObjectWithTag("Boss").GetComponent<SCR_Boss_SM>();       //add check to see if the boss bar is needed
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the player/boss is in grace period.
        if (playerGracePeriod >= 0)
        {
            playerGracePeriod -= Time.deltaTime;
        }
        if (bossGracePeriod >= 0)
        {
            bossGracePeriod -= Time.deltaTime;
        }
        if (enemyGracePeriod >= 0)
        {
            enemyGracePeriod -= Time.deltaTime;
        }
    }

    // Public HeavyAttack() is accessed from SwordThings.cs if the player's attack is counted as a heavy attack
    public void HeavyAttack()
    {
        // doHeavy is used in OnTriggerStay
        doHeavy = true;
    }

    private void OnTriggerStay(Collider other)
    {
        // Stops the weapon being used from hitting the allies or itself and running the additional statements 
        if (other.transform.root != transform.root)
        {

            ///<summary>
            /// This section of code will be checking which object is currently using MeleeHitDetection.cs by checking for specific tags that are on each object.
            /// For example, if the script is attatched to the player object, expect that the tag "Player" is in the parent object, therefore check the parent tag and compare to "Player".
            /// If true, then the object is on the player's weapon, therefore code specific to player will run.
            /// 
            /// In this example, the player's animator is used to determine if the player is currently in swing animation, if so, we know the player is attacking.
            /// If the player is attacking, the script checks if the enemy hit is the boss or an enemy. IMPORTANT - For now, enemies are just destroyed but when the stat system is
            /// implemented, the enemy will take damage instead and most likely have it's own grace period.
            ///</summary>


            // If attatched object is the Player
			if (transform.root.GetChild(0).CompareTag("Player"))
            {

                // Checks animator, if player is attacking
                if (swordAttackScript.playerAnim.GetBool("IsAttack") || swordAttackScript.playerAnim.GetBool("IsCombo") || swordAttackScript.playerAnim.GetBool("IsHeavySwing"))
                {
                    if (isCombo)
                    {
                        gracePeriodReset = .1f;
                        isCombo = false;
                    }
                    // Check tag to see what type of enemy was hit
                    switch (other.gameObject.tag)
                    {
                        //if boss is hit
                        case ("Boss"):
                            //checks if boss' grace period is over
                            if(bossGracePeriod <= 0 && boss.currentState != State.Ritual)
                            {
                                //cheack if boss is doing a heavy attack
                                if (doHeavy)
                                {
                                    BossStats.BossDamage(PlayerHeavyDamage, doHeavy);
                                    doHeavy = false;
                                }
                                else
                                {
                                    BossStats.BossDamage(PlayerDamage, doHeavy);
                                }
                                boss.currentState = SCR_Boss_SM.State.Damaged;
                                //update bosses health bars
                                //GameObject.Find("PRF_UI_BossHealthBar").GetComponentInChildren<UpdateBossHealth>().UpdateHealth();
                                bossGracePeriod = gracePeriodReset;
                            }
                            break;
                                
                        //if enemy is hit
                        case ("Enemy"):

                            enemyHealth = other.GetComponent<EnemyStats>();
                            if(other.GetComponentInChildren<MeleeHitDetection>().enemyGracePeriod <= 0)
                            {
                                RuntimeManager.PlayOneShot(eventSwordCollision);        // Plays SwordCollision SFX
                                Debug.Log(enemyHealth.EnemyHealth);
                                // Placeholder - Can be removed when Damaged state is called for enemies)
                                //if (other.GetComponent<SCR_enemyID>().ID == 0 && other.GetComponent<SCR_SwordEnemy_SM>().currentState == SCR_SwordEnemy_SM.State.Damaged)   //sword enemy
                                //{
                                    //makes it so the sword enemy takes no damage while shielding 
                                    //Debug.Log("shielding");
                                //}
                                if (doHeavy)
                                {
                                    enemyHealth.EnemyDamage(PlayerHeavyDamage);
                                }
                                else
                                {
                                    enemyHealth.EnemyDamage(PlayerDamage);

                                }

                                if (other.GetComponent<SCR_enemyID>().ID == 0)   //sword enemy
                                {
                                    other.GetComponent<SCR_SwordEnemy_SM>().currentState = SCR_SwordEnemy_SM.State.Damaged;
                                }
                                else if (other.GetComponent<SCR_enemyID>().ID == 1)   //archer enemy
                                {
                                    other.GetComponent<SCR_Archer_SM>().currentState = SCR_Archer_SM.State.Damaged;
                                }
                                else if (other.GetComponent<SCR_enemyID>().ID == 2)   //stationary archer enemy
                                {
                                    other.GetComponent<SCR_StationaryArcher_SM>().currentState = SCR_StationaryArcher_SM.State.Damaged;
                                }
                                else if (other.GetComponent<SCR_enemyID>().ID == 3)   //ritual enemy
                                {
                                    other.GetComponent<SCR_RitualEnemy_SM>().currentState = SCR_RitualEnemy_SM.State.Damaged;
                                }
                                other.GetComponentInChildren<MeleeHitDetection>().enemyGracePeriod = gracePeriodReset;
                            }
                            break;

                        case ("Arrow"):

                            Destroy(other.gameObject);

                            Debug.Log("Deflect");

                            break;
                    }
                    if (!isCombo)
                    {
                        gracePeriodReset = .85f;
                    }
                }
            }

            // If attatched object is the Boss
            else if (transform.parent.parent.CompareTag("Boss"))
            {

                // Check if attacking in their AI states and they are hitting the player
                if (GetComponentInParent<SCR_Boss_SM>().attacking && playerGracePeriod <= 0 && other.gameObject.CompareTag("Player")){

                    // If the player is blocking, triggers block in SwordThings.cs, otherwise it will hurt the player directly
                    if (!swordAttackScript.playerAnim.GetBool("IsBlocking"))
                    {
                        RuntimeManager.PlayOneShot(eventDamaged);       // Plays PlayerHurt SFX
                        playerHealth.PlayerDamage(BossDamage);
                    }
                    else
                    {
                        playerHealth.PlayerShieldDamage(BossDamage);
                    }
                    playerGracePeriod = 0.5f;
                }
            }

            // If attatched object is an Enemy
            else
            {

                if(GetComponentInParent<SCR_enemyID>().ID == 0)
                {
                    // Check if attacking in their AI states and they are hitting the player
                    if (GetComponentInParent<SCR_SwordEnemy_SM>().attacking && playerGracePeriod <= 0 && other.gameObject.CompareTag("Player"))
                    {

                        // If the player is blocking, triggers block in SwordThings.cs, otherwise it will hurt the player directly
                        if (!swordAttackScript.playerAnim.GetBool("IsShielding"))
                        {
                            RuntimeManager.PlayOneShot(eventDamaged);       // Plays PlayerHurt SFX
                            playerHealth.PlayerDamage(EnemyDamage);
                        }
                        else
                        {
                            playerHealth.PlayerShieldDamage(EnemyDamage);
                        }
                        playerGracePeriod = 1f;
                    }
                }
				else if(GetComponentInParent<SCR_enemyID>().ID == 3)
				{
					// Check if attacking in their AI states and they are hitting the player
					if (GetComponentInParent<SCR_RitualEnemy_SM>().attacking && playerGracePeriod <= 0 && other.gameObject.CompareTag("Player"))
					{
                        Debug.Log(EnemyDamage);
						// If the player is blocking, triggers block in SwordThings.cs, otherwise it will hurt the player directly
						if (!swordAttackScript.playerAnim.GetBool("IsShielding"))
						{
                            Debug.Log(EnemyDamage);
                            playerHealth.PlayerDamage(EnemyDamage);
						}
						else
						{
                            Debug.Log(EnemyDamage);
                            playerHealth.PlayerShieldDamage(EnemyDamage);
						}
						playerGracePeriod = 1f;
					}
				}
            }
        }
    }
    public void GetPlayerValues()
    {
        PlayerDamage = playerHealth.TransferPlayerDamage();
        PlayerHeavyDamage = playerHealth.TransferPlayerHeavyDamage();
    }

    void GetBossValues()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        string sceneName = currentScene.name;
        if (sceneName == "BOSS_BLOCKOUT")
        {
            BossDamage = BossStats.TransferBossDamage();
        }
    }

    void GetEnemyValues()
    {
        EnemyDamage = enemyHealth.TransferEnemyDamage();
    }
}