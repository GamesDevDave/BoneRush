//Double click code from: https://www.youtube.com/watch?v=GR0XJX1phiw
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class SwordThings : MonoBehaviour
{

	float attackHoldTime;
	bool countAttackTime;
	bool startedCounting;
	float timePassedSinceAttacking;


	float attackDelay;
    [SerializeField]
    float attackDelayReset = .3f;
    
    bool canAttack;
    public bool canHeavyAttack;
    public float heavyAttackStamina = 20f;

    [SerializeField]
    int heavyDamage = 40;
    [SerializeField]
    int normalDamage = 10;

    public bool heavyAttack;
    public bool isBlocking;

    [SerializeField]
    float heavyAttackReqTime = 1.5f;

    [SerializeField]
    float shieldBlockModifier = .1f;

    [Header("Components")]
    [Space(30)]
    public Animator swordAnimation;
    [SerializeField]
    Animator shieldAnimation;
    [SerializeField]
    public PlayerStaminaBar stam;

    public int shieldBashDmg = 1;

	CameraShake cameraShake;

    // FMOD:
    [EventRef] [SerializeField] string eventSwing;      // Played when player attacks

	// Start is called before the first frame update
	void Start()
	{
        // Initializing script and components
		cameraShake = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>();
        shieldAnimation = GameObject.Find("PlayerShield").GetComponent<Animator>();
		swordAnimation = GameObject.Find("PlayerSword").GetComponent<Animator>();
		swordAnimation.SetBool("Left?", true);
        stam = GetComponent<PlayerStaminaBar>();
	}

	// Update is called once per frame
	void Update()
	{
        ///Debug Remove this when blocking arrows is fine
        ///Will slow down gametime to test arrows
        if (Input.GetKeyDown(KeyCode.K))
        {
            Time.timeScale = .025f;
            GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerStats>().PlayerHealing();
        }
        if (Input.GetKeyUp(KeyCode.K))
        {
            Time.timeScale = 1f;
        }
        if(attackDelay <= 0 && !isBlocking)
        {
            canAttack = true;
        }
        else
        {
            canAttack = false;
        }

        // If the player is able to attack:
		if (canAttack)
		{

            // On left click pressed, reset variables
			if (Input.GetMouseButtonDown(0))
			{
				countAttackTime = true;
				startedCounting = true;
				timePassedSinceAttacking = 0f;
			}

            // If holding left click, and attack initiated (countAttackTime): 
			else if (Input.GetMouseButton(0) && countAttackTime == true)
			{

                // If the camera is not currently shaking, start shaking coroutine again
				if (!cameraShake.shaking)
				{
					StartCoroutine(cameraShake.Shake(.2f));
				}

                // Because the player is holding the attack, they have not currently attacked and the timePassed is reset
				timePassedSinceAttacking = 0f;
			}

            // On release of left mouse button:
            if (Input.GetMouseButtonUp(0))
			{
                // No longer attacking
				countAttackTime = false;
			}
		}

        // If the player can't attack then reduce attackDelay
		else
		{
			attackDelay -= Time.deltaTime;
		}

        // If currently holding left mouse (attacking):
		if (countAttackTime)
		{

            // increment attackHoldTime (time that the attack has currently been held for)
			attackHoldTime += Time.deltaTime;

            // If been holding the attack for 3 or more seconds then automatically cancel the attack
			if(attackHoldTime >= 3f)
			{
				countAttackTime = false;
			}
		}

        // If no longer counting how long the attack has been held for, but the attack had previously startedCounting, then the attack is initiated
		else if (startedCounting == true)
		{
            // attack variables at this point are all set to negative (no longer attacking, counting and reset timers)
			startedCounting = false;
			Attack(attackHoldTime);
			attackHoldTime = 0;
		}


        // Blocking checks
        CheckBlocking();

        /// <summary>
        /// Following code is related to animation (in region "anim")
        /// Will need to be changed when animations are implemented
        /// </summary>
        #region anim
        timePassedSinceAttacking += Time.deltaTime;

		if ((timePassedSinceAttacking >= 1.5f && !swordAnimation.GetBool("Left?")) || (isBlocking && !swordAnimation.GetBool("Left?")))
		{
			swordAnimation.SetBool("Swing", false);
			swordAnimation.SetBool("ResetPos", true);
			swordAnimation.SetBool("Left?", !swordAnimation.GetBool("Left?"));
		}
		else if (swordAnimation.GetBool("ResetPos") && !swordAnimation.GetCurrentAnimatorStateInfo(0).IsName("SwordSwing"))
		{
			swordAnimation.SetBool("ResetPos", false);
		}

		if (swordAnimation.GetBool("Left?") && swordAnimation.GetCurrentAnimatorStateInfo(0).IsName("SwordSwing 0"))
		{
			swordAnimation.SetBool("Swing", false);
		}
		else if (!swordAnimation.GetBool("Left?") && swordAnimation.GetCurrentAnimatorStateInfo(0).IsName("SwordSwing"))
		{
			swordAnimation.SetBool("Swing", false);
		}
        #endregion
    }

    void CheckBlocking()
    {
        if (isBlocking)
        {
            CheckForShieldBash();
        }

        /// <summary>
        /// Checks if:
        /// - Player has pressed right click
        /// - Player is not attacking
        /// - Player has enough stamina to use the shield (more than 10%)
        /// </summary>
        if (Input.GetMouseButtonDown(1) && !startedCounting && stam.staminaBar.value > stam.maxStamina*0.1 && stam.canBlock)
        {
            isBlocking = true;
            Block();
        }
        

        /// <summary>
        /// Checks if:
        /// - Player no longer holding down right click
        /// - Player is currently blocking
        /// - Player no longer has enough stamina to use the shield
        /// </summary>
        else if ((!Input.GetMouseButton(1) && isBlocking) || stam.staminaBar.value < stam.maxStamina * 0.1 && stam.canBlock)
        {
            isBlocking = false;
            Block();
        }
    }

    void CheckForShieldBash()
    {
        if (Input.GetMouseButtonDown(0) && !GetComponentInChildren<ShieldBashCheck>().shieldBash)
        {
            shieldAnimation.SetTrigger("shieldBash");
            stam.staminaBar.value -= stam.maxStamina * .15f;
        }
    }

    void Attack(float time) // time is how long the attack was held for
	{

        // Once attacked, add a delay before the next attack
		attackDelay = attackDelayReset;

        // If player held the attack for long enough to count as heavy and they have enough stamina for the attack, then a heavy attack is initiated
        // Otherwise, a regular attack is initiated
        if (time > heavyAttackReqTime && stam.staminaBar.value > heavyAttackStamina)
        {

            // Gets the MeleeHitDetection script attatched to the player and activate HeavyAttack() function
            GetComponentInChildren<MeleeHitDetection>().HeavyAttack();

            // Stamina affected by the cost of using heavy attack
            stam.staminaBar.value -= Mathf.Clamp(heavyAttackStamina, stam.minStamina, stam.maxStamina);

            swordAnimation.SetBool("Left?", !swordAnimation.GetBool("Left?"));
            swordAnimation.SetBool("Swing", true);
        }
        if (time < heavyAttackReqTime)
        {
            swordAnimation.SetBool("Left?", !swordAnimation.GetBool("Left?"));
            swordAnimation.SetBool("Swing", true);
        }

        // Triggers SwordSwing event in FMOD
        FMODUnity.RuntimeManager.PlayOneShot(eventSwing);
    }

    public void Block(float damage = 0, bool isBossAttack = false)
    {
        if (isBlocking)
        {
            // Starts/continues blocking animation and state
            shieldAnimation.SetBool("isBlocking", true);

            // If the attack blocked is a boss attack, this area can be used to affect stamina/health differently
            // Otherwise, the attack will take away from stamina (affected by the shieldBlockModifier), limited to 50% of max stamina per hit
            if (isBossAttack)
            {
                stam.staminaBar.value -= Mathf.Clamp(damage, stam.minStamina, stam.maxStamina);
            }
            else
            {
                stam.staminaBar.value -= Mathf.Clamp(damage * shieldBlockModifier, stam.minStamina, stam.maxStamina * 0.5f);
            }
        }
        else
        {
            // Ends blocking animation and state
            shieldAnimation.SetBool("isBlocking", false);
        }
    }
}
