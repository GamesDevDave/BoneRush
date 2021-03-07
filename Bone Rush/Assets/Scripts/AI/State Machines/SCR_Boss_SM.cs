//Base state machine adapted from https://unity3d.college/2019/04/28/unity3d-ai-with-state-machine-drones-and-lasers/  
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using FMODUnity;

public class SCR_Boss_SM : MonoBehaviour
{
    [Header("Pathfinding Variables")]
    public State currentState;
    [SerializeField] private NavMeshAgent navMeshAgent;
    private GameObject player;
    private PlayerStats ps;
    private Animator swing;
    private Animator swingPivot; // For spin attack, sledgehammer uses a pivot point on the boss for animation
    private bool groundpounding;
    private float attackDelayReset = 2f;
    private float attackDelay;
    public bool attacking;
    private float restTime;
    private bool spinAttack = false;
    private bool groundPoundAttack = false;
    [SerializeField] private float groundPoundAttackCooldownReset = 10f;
    private float groundPoundAttackCooldown;
    private float spinAttackCooldownReset = 15f;
    private float spinAttackCooldown;
    private bool inRest;
    private bool inSpinTime;
    [SerializeField] private float[] enragedHealthPercentage;
    [SerializeField] private float lowerGPCooldown;
    [SerializeField] private float lowerSwingCooldown;
    [SerializeField] private int musicTrigger = 2;
    private int bossEnrageState = 0;
    private Vector3 currentLocation;
    private int numberOfTaggedObjects;
    private float distanceToPlayer;
    [Header("Attacking Variables")]
    private SCR_RitualEnemy_SM[] rsm;
    public static int ritualEnemiesChanneling { get; set; }
    [Header("FMOD Events")]
    [EventRef] [SerializeField] private string eventSpinAttack;     // Played when boss starts spin attack
    [EventRef] [SerializeField] private string eventAttack;         // Played when boss attacks normally
    [EventRef] [SerializeField] private string eventRest;           // Played when boss is recovering from an attack
    private SCR_Boss_Transition transitionTrigger;          // Needed to call TransitionTrigger() function to make the music transition into the next phase
    private bool transitionTriggered = false;               // Needed to stop TransitionTrigger() calling every frame - set to true when transition is triggered

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        swingPivot = GameObject.Find("SledgehammerBossPivot").GetComponent<Animator>();
        swing = GameObject.Find("MDL_Hammer").GetComponent<Animator>();
        swingPivot.enabled = false;
        groundPoundAttackCooldown = 0; //groundPoundAttackCooldownReset
        ps = player.GetComponent<PlayerStats>();
        spinAttackCooldown = spinAttackCooldownReset;
        restTime = BossStats.TransferRecoveryTime();
        currentState = State.Ritual;
        rsm = FindObjectsOfType<SCR_RitualEnemy_SM>();
        navMeshAgent = GetComponent<NavMeshAgent>();
        // Assigns transitionTrigger to the SCR_Boss_Transition script component so the StopInstance() function can be called.
        transitionTrigger = GameObject.FindGameObjectWithTag("GameManager").GetComponent<SCR_Boss_Transition>();
    }

    private void Update()
    {
        // Triggers music transition when boss reaches enraged phase. 
        if (bossEnrageState == musicTrigger && transitionTriggered == false)
        {
            // Triggers transition function in SCR_Boss_Transition script
            transitionTrigger.TriggerTransition();
            transitionTriggered = true;
        }

        EnragedCheck();
        AbilitiesCooldown();

        switch (currentState)
        {
            //follows the player untill the boss can attack
            //checks if it can use special attack before using a normal attack
            case State.Follow:
                {
                    Following();
                    break;
                }
            //basic boss swing with hammer
            case State.Attack:
                {
                    NormalAttack();
                    break;
                }
            //special attack, boss slams down its hammer creating a shock wave, cooldown is activated once attack is used
            case State.GroundPound:
                {
                    GroundPoundAttack();
                    break;
                }
            //special attack, boss swings its hammer around, cooldown is activated once attack is used
            case State.Swing:
                {
                    SwingAttack();
                    break;
                }
            //resting after special attack, boss stays still and recovers, allows the player to attack or run
            case State.Rest:
                {
                    navMeshAgent.SetDestination(transform.position);
                    if (!inRest) StartCoroutine(RestTime(restTime));
                    break;
                }
            //happens when the boss takes damage
            case State.Damaged:
                {
                    if (BossStats.BossCurrentHealth <= 0) currentState = State.Death;  //check if the boss is still alive
                    else if (inRest) currentState = State.Rest;       //makes the boss stay in rest if it gets attacked while in rest
                    else currentState = State.Follow;
                    break;
                }
            //death state destroys the gameobject
            case State.Death:
                {
                    Destroy(gameObject);
                    break;
                }
            //boss stays still and does the ritual
            case State.Ritual:
                {
                    GainingPower();
                    break;
                }
        }
    }

    public enum State
    {
        Follow,
        Attack,
        GroundPound,
        Swing,
        Rest,
        Damaged,
        Death,
        Ritual
    }

    private void EnragedCheck()
    {
        // Shortens the boss's cooldown when it reaches a certain percentage of health
        if (enragedHealthPercentage.Length > 0 && bossEnrageState <= enragedHealthPercentage.Length - 1)
        {
            if (BossStats.BossCurrentHealth < (BossStats.BossMaxHealth * (enragedHealthPercentage[bossEnrageState] / 100)) && bossEnrageState <= enragedHealthPercentage.Length)
            {
                groundPoundAttackCooldownReset = groundPoundAttackCooldownReset - lowerGPCooldown;
                spinAttackCooldownReset = spinAttackCooldownReset - lowerSwingCooldown;
                if (spinAttackCooldownReset < 12.5f)        //check always fails for some reason
                {                                               //hard limit so the cooldown does not get too low
                    spinAttackCooldownReset = 8f;
                }
                if (groundPoundAttackCooldownReset < 10)        //check always fails for some reason
                {                                               //hard limit so the cooldown does not get too low
                    groundPoundAttackCooldownReset = 5;
                }
                bossEnrageState = bossEnrageState + 1;
            }
        }
    }

    private void AbilitiesCooldown()
    {
        if (currentState == State.Ritual)        //continues the ritual
        {
            ritualEnemiesChanneling = 0;
            foreach (SCR_RitualEnemy_SM ritual in rsm)
            {
                if (ritual.currentlyChanelling)
                {
                    ritualEnemiesChanneling++;
                }
            }
        }
        else
        {
            if (groundPoundAttackCooldown <= 0)      //cooldowns for boss abilities 
            {
                groundPoundAttack = true;
            }
            else
            {
                groundPoundAttackCooldown -= Time.deltaTime;
            }

            if (spinAttackCooldown <= 0)
            {
                spinAttack = true;
            }
            else
            {
                spinAttackCooldown -= Time.deltaTime;
            }

            if (attackDelay > 0)
            {
                swing.SetBool("Attacking", false);
                attackDelay -= Time.deltaTime;
            }
            else
            {
                attacking = false;
            }
        }
    }

    private void Following()
    {
        transform.LookAt(new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z), Vector3.up);
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > 3.5f)
        {
            Vector3 player_location = player.transform.position;
            navMeshAgent.SetDestination(player_location);
        }
        if (spinAttack == true && distanceToPlayer <= 5.5f && !groundpounding)      //boss always try to swing first
        {
            swingPivot.enabled = true;
            currentState = State.Swing;
        }
        else if (groundPoundAttack == true && distanceToPlayer <= 24 && distanceToPlayer >= 4 && !inSpinTime)      //then trys to groundpound next
        {
            currentState = State.GroundPound;

            navMeshAgent.SetDestination(transform.position);
        }
        else if (distanceToPlayer <= 4)      //then trys to do a normal attack last
        {
            currentState = State.Attack;
        }
    }

    private void NormalAttack()
    {
        currentLocation = transform.position;
        if (attackDelay <= 0)      //added delay so boss does not spam attack the player
        {
            // FMOD: Plays NormalAttack SFX
            RuntimeManager.PlayOneShot(eventAttack, transform.position);

            attacking = true;
            swing.SetBool("Attacking", true);
            navMeshAgent.SetDestination(currentLocation);
            attackDelay = attackDelayReset;
        }
        currentState = State.Follow;       //boss goes back to following the player after attacking
    }

    private void GroundPoundAttack()
    {
        restTime = 2.5f;    //initial rest time 
        for (int a = 1; a < enragedHealthPercentage.Length + 1; a++)
        {
            if(a <= bossEnrageState)
            {
                restTime = restTime + 0.2f;     //increases boss rest time
            }
        }
        if (restTime > 3.5f)
        {
            restTime = 3.5f;        //sets a hard limit for the boss rest time
        }

        groundPoundAttack = false;
        attacking = true;
        groundpounding = true;
        swing.SetBool("GroundPound", true);
        attackDelay = attackDelayReset;

        if (GetComponentInChildren<MeleeHitDetection>().groundPoundAnimFinished)
        {
            GetComponentInChildren<SCR_GroundPoundCheck>().GroundPound();
            groundPoundAttackCooldown = groundPoundAttackCooldownReset;
            currentState = State.Rest;
        }
    }

    private void SwingAttack()
    {
        restTime = 3f;    //initial rest time 
        for (int a = 1; a < enragedHealthPercentage.Length + 1; a++)
        {
            if (a <= bossEnrageState)
            {
                restTime = restTime + 0.2f;     //increases boss rest time
            }
        }
        if (restTime > 4)
        {
            restTime = 4;       //sets a hard limit for the boss rest time
        }

        spinAttack = false;
        attacking = true;
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > 3.5f)
        {
            Vector3 player_location = player.transform.position;
            navMeshAgent.SetDestination(player_location);
        }
        else navMeshAgent.SetDestination(transform.position);
        if (!inSpinTime)
        {
            swingPivot.SetTrigger("StartBossSpin");
            StartCoroutine(SpinDuration(3.5f));
        }
    }

    private void GainingPower()      //makes the boss stand still
    {
        navMeshAgent.SetDestination(transform.position);
        numberOfTaggedObjects = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (numberOfTaggedObjects == 0)
        {
            transitionTrigger.StartMusic();             // Starts Boss MSC
            currentState = State.Follow;
        }
    }

    private IEnumerator RestTime(float time)
    {
        // FMOD: Plays Rest SFX
        RuntimeManager.PlayOneShot(eventRest, transform.position);
        inRest = true;
        attacking = false;
        attackDelay = attackDelayReset;
        yield return new WaitForSeconds(time);
        swingPivot.enabled = false;
        swing.SetBool("GroundPound", false);
        groundpounding = false;
        ps.collided = false;
        currentState = State.Follow;       //makes the boss follow the player after it is finished resting
        inRest = false;
    }

    private IEnumerator SpinDuration(float time)
    {
        inSpinTime = true;
        // FMOD: Plays SpinAttack SFX
        RuntimeManager.PlayOneShot(eventSpinAttack, transform.position);
        attackDelay = attackDelayReset + time;
        yield return new WaitForSeconds(time);
        swingPivot.SetTrigger("EndBossSpin");
        spinAttackCooldown = spinAttackCooldownReset;
        currentState = State.Rest;
        inSpinTime = false;
    }
}