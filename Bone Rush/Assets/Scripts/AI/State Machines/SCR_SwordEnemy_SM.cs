//Base state machine adapted from https://unity3d.college/2019/04/28/unity3d-ai-with-state-machine-drones-and-lasers/  
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using FMODUnity;
using UnityStandardAssets.Characters.FirstPerson;

public class SCR_SwordEnemy_SM : MonoBehaviour
{
    [Header("Pathfinding Variables")]
    [SerializeField] private GameObject[] destinations;
    [SerializeField] private NavMeshAgent navMeshAgent;
    private float rotationAmount = 270;
    public State currentState;
    private float followDistance = 10f;
    private int setPath = 0;
    private float lookRange = 25f;
    [SerializeField] private float rotationSpeed = 50;
    private GameObject player;
    private bool locationSet = false;
    private float stoppingRotation;
    private float enemyCurrentRotation;
    private bool shielding = false;
    private Animator enemyAnim;
    private bool canKick;
    private bool canShieldBash;
    private bool mustAttack = false;
    private int kickRNG = 0;
    private int shieldBashRNG = 0;
    private bool kickRecharge = false;
    private bool shieldBashRecharge = false;
    private bool playerStunnded = false;
    private bool isStunned = false;
    private bool playerStunned = false;
    private int lungeCharge = 0;
    private float distanceToPlayer;
    private Vector3 enemyLocation;
    private Vector3 distanceToDestination;
    private Vector3 playerLocation;
    private EnemyStats ES;
    [Header("Attacking Variables")]
    private float attackRate = 1f;
    private bool hasAttacked;
    public bool attacking;
    [Header("FMOD Events")]
    [EventRef] [SerializeField] private string eventDamaged;        // Played when sword enemy takes damage
    [EventRef] [SerializeField] private string eventKick;           // Played when sword enemy kicks
    [EventRef] [SerializeField] private string eventShieldBash;     // Played when sword enemy shield bashes

    private void Start()
    {
        //DrawPatrolPoints();
        player = GameObject.FindWithTag("Player");
        enemyAnim = GetComponentInChildren<Animator>();
        canKick = GetComponent<EnemyStats>().Can_Kick;
        canShieldBash = GetComponent<EnemyStats>().Can_ShieldBash;
        navMeshAgent = GetComponent<NavMeshAgent>();
        ES = GetComponent<EnemyStats>();
    }

    private void OnDisable()        //enables the player movement when the enemy is killed
    {
        player.GetComponent<RigidbodyFirstPersonController>().enabled = true;
        player.GetComponent<Dashing>().enabled = true;
        player.GetComponent<PlayerAnimationController>().enabled = true;
        playerStunnded = false;
    }

    private void Update()
    {
        switch (currentState)
        {
            //enemy will walk between set patrol points
            case State.Patrol:
                {
                    Patrolling();       //check if the enemy has reached a patrol point
                    break;
                }
            //the enemy rotates around the patrol point keeping and eye out for the player
            case State.Search:
                {
                    Searching();
                    break;
                }
            //follows the player unless they go out of range of the enemy,
            //if the player gets far enough away the enemy goes back to patrolling
            case State.Follow:
                {
                    Following();        //makes the enemy follow the player
                    break;
                }
            //enemy attacks the player and then goes back to following
            case State.Attack:
                {
                    Attacking();
                    break;
                }
            //happens when enemy is damaged, set the enemy to use their shield
            case State.Damaged:
                {
                    TakeDamage();
                    break;
                }
            //destroys the enemy gameobject
            case State.Death:
                {
                    Destroy(gameObject);
                    break;
                }
            //makes enemies stay stunned after being shield bashed by the player
            case State.Stunned:
                {
                    if (!isStunned) StartCoroutine(Stunned(3));
                    break;
                }
            //enemy kick backs the player
            case State.Kick:
                {
                    PushPlayer();       //pushes the player back
                    break;
                }
            //Bashes backs the player with their shield, makes the player get stunned 
            case State.ShieldBash:
                {
                    ShieldBashing();
                    break;
                }
            //Lunges after the player after a short charge up
            case State.Lunge:
                {
                    StartCoroutine(ChargeLunge());
                    break;
                }
        }
    }

    public enum State
    {
        Patrol,
        Search,
        Follow,
        Attack,
        Damaged,
        Death,
        Stunned,
        Kick,
        ShieldBash,
        Lunge
    }

    private void DrawPatrolPoints()       //used to draw lines between enemy patrol points
    {
        for (int i = 0; i < destinations.Length; i++)
        {
            if (i < destinations.Length - 1) Debug.DrawLine(destinations[i].transform.position, destinations[i + 1].transform.position, Color.red, 60);
            else if (i == destinations.Length - 1) Debug.DrawLine(destinations[i].transform.position, destinations[0].transform.position, Color.red, 60);
        }
    }

    private void Patrolling()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        enemyAnim.SetBool("isMoving", true);
        enemyLocation = destinations[setPath].transform.position;
        navMeshAgent.SetDestination(enemyLocation);
        distanceToDestination = enemyLocation - transform.position;
        if (distanceToDestination.magnitude < 2)      //checks if the enemy is at a patrol point
        {
            if (setPath == (destinations.Length - 1)) setPath = -1;
            setPath += 1;      //assigns the enemy to its next patrol point
            currentState = State.Search;
        }
        if (distanceToPlayer <= followDistance) currentState = State.Follow;    //sets the enemy to follow the player if they are within the enemies follow distance
    }

    private void Searching()
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (destinations.Length == 1) enemyAnim.SetBool("isMoving", false);     //sets the enemy to use the idle animation if it has no patrol points
        else
        {
            EnemySight();       //sends enemy into follow state if they see the player 
            EnemyRotation();        //makes enemy spin
        }
        if (distanceToPlayer <= followDistance / 2)      //enemy has lower awareness of immediate surroundings while searching
        {
            currentState = State.Follow;
        }
    }

    private void EnemySight()
    {
        enemyAnim.SetBool("isMoving", false);
        int layerMask = 1 << 10;    //raycast is used to simulate enemy sight 
        RaycastHit hit; //the first raycast checks for walls
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, lookRange, layerMask)) { }
        else  //the second raycast checks for the player
        {
            layerMask = 1 << 8;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, lookRange, layerMask))
            {
                currentState = State.Follow;       //enemy follows the player is they see them
            }
        }
    }

    private void EnemyRotation()
    {
        transform.Rotate(Vector3.up * -rotationSpeed * Time.deltaTime, Space.World);
        enemyCurrentRotation = transform.eulerAngles.y;
        if (locationSet == false)
        {
            if (rotationAmount >= 360)     //limits rotation amount
            {
                rotationAmount = 359;
            }
            stoppingRotation = Mathf.Round(enemyCurrentRotation) - rotationAmount;
            if (stoppingRotation < 0)
            {
                stoppingRotation = 360 + stoppingRotation;
            }
            locationSet = true;
        }
        if (Mathf.Abs(enemyCurrentRotation - stoppingRotation) < 1)
        {
            locationSet = false;
            currentState = State.Patrol;       //once the enemy finishes its rotation it goes to its next patrol point
        }
    }

    private void Following()
    {
        enemyAnim.SetBool("isMoving", true);
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        playerLocation = player.transform.position;
        hasAttacked = false;
        if (distanceToPlayer > 1.5f)
        {
            navMeshAgent.SetDestination(playerLocation);
        }
        else  //stops the enemy getting too close to the player
        {
            navMeshAgent.SetDestination(transform.position);
        }

        if (distanceToPlayer <= 1.5 && !hasAttacked)  //attacks the player
        {
            shieldBashRNG = Random.Range(1, 11);
            kickRNG = Random.Range(1, 11);
            if (lungeCharge >= 3)   //lunge attack it attempted first
            {
                currentState = State.Lunge;
            }
            else if (canShieldBash == true && shieldBashRNG <= 100 && mustAttack == false && shieldBashRecharge == false) //then the shield bash
            {
                currentState = State.ShieldBash;
            }
            else if (canKick == true && kickRNG <= 3 && mustAttack == false && kickRecharge == false) //then the kick
            {
                currentState = State.Kick;
            }
            else //then the normal attack
            {
                mustAttack = false;
                currentState = State.Attack;
            }
        }
        else
        {
            if (distanceToPlayer > (followDistance * 2)) //enemy is more alert while following the player
            {
                currentState = State.Patrol;
            }
            else
            {
                currentState = State.Follow;
            }
        }

    }

    private void Attacking()     //normal basic swing
    {
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer >= 1.5)  //enemy won't swing if the player is too far away
        {
            currentState = State.Follow;
        }

        if (hasAttacked == false) //enemy won't swing if it is still on the attack delay
        {
            AnimationClip clip;
            AnimationEvent evt;
            evt = new AnimationEvent();

            evt.intParameter = 1;
            evt.time = .7f;
            evt.functionName = "AnimationAttackTrigger";

            clip = enemyAnim.runtimeAnimatorController.animationClips[2];
            clip.AddEvent(evt);

            //attacking = true;
            enemyAnim.SetBool("isAttacking", true);
            hasAttacked = true;
            StartCoroutine(AttackDelay());
        }
    }

    private void TakeDamage()
    {
        navMeshAgent.SetDestination(transform.position);
        if (ES.EnemyHealth <= 0)//check if the enemy is still alive
        {
            currentState = State.Death;
        }
        // FMOD:
        RuntimeManager.PlayOneShot(eventDamaged, transform.position);
        if (!shielding) StartCoroutine(ShieldTime());   //makes the enemy use their shield
    }

    private void PushPlayer()
    {
        // FMOD Kick Sound:
        RuntimeManager.PlayOneShot(eventKick, transform.position);
        if (!kickRecharge)
        {
            StartCoroutine(KickRecharging());
            mustAttack = true; //enemy will always do a normal attack after they kick, unless they can do a lunge attack
            player.GetComponent<Rigidbody>().velocity = player.transform.position + (Vector3.back * 0.1f);    //moves the player
        }
        currentState = State.Follow;
    }

    private void ShieldBashing()
    {
        if (!shieldBashRecharge)
        {
            // FMOD Shield Bash sound:
            RuntimeManager.PlayOneShot(eventShieldBash, transform.position);
            kickRNG = Random.Range(1, 11);      //enemy can kick after doing a shield bash
            if (!playerStunned)
            {
                StartCoroutine(PlayerStunned(1f));             //Disable player's movement and attack scripts
                StartCoroutine(ShieldBashRecharging());
            }
            if (canKick == true && kickRNG <= 3 && mustAttack == false && kickRecharge == false)
            {
                currentState = State.Kick;
            }
            else
            {
                currentState = State.Follow;
            }
        }
        else
        {
            currentState = State.Follow;
        }
    }

    private IEnumerator AttackDelay()
    {
        lungeCharge = lungeCharge + 1;  //lunge attacks happen every 3 attacks
        yield return new WaitForSeconds(attackRate);
        attacking = false;
        enemyAnim.SetBool("isAttacking", false);
        currentState = State.Follow;
    }

    private IEnumerator ShieldTime()
    {
        shielding = true;
        yield return new WaitForSeconds(2);     //enemy is invincible while shielding
        shielding = false;
        currentState = State.Follow;
    }

    private IEnumerator Stunned(float time)
    {
        isStunned = true;
        navMeshAgent.isStopped = true;
        enemyAnim.SetBool("isMoving", false);
        enemyAnim.SetBool("isStunned", true);
        yield return new WaitForSeconds(time);
        enemyAnim.SetBool("isStunned", false);
        navMeshAgent.isStopped = false;
        isStunned = false;
        currentState = State.Follow;
    }

    private IEnumerator PlayerStunned(float time)      //stuns the player
    {
        playerStunned = true;
        mustAttack = true;     //enemy will always do a normal attack after they shield bash, unless they can do a lunge attack
        player.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().enabled = false;
        player.GetComponent<Dashing>().enabled = false;
        player.GetComponent<PlayerAnimationController>().enabled = false;
        enemyAnim.SetBool("isShieldBash", true);
        yield return new WaitForSeconds(time);
        enemyAnim.SetBool("isShieldBash", false);
        player.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().enabled = true;
        player.GetComponent<Dashing>().enabled = true;
        player.GetComponent<PlayerAnimationController>().enabled = true;
        playerStunned = false;
    }

    private IEnumerator KickRecharging()     //delay for kick to stop it being spammed by enemies 
    {
        kickRecharge = true;
        yield return new WaitForSeconds(3f);
        kickRecharge = false;
    }

    private IEnumerator ShieldBashRecharging()    //delay for shield bash to stop it being spammed by enemies
    {
        shieldBashRecharge = true;
        yield return new WaitForSeconds(3f);
        shieldBashRecharge = false;
    }

    private IEnumerator ChargeLunge()
    {
        lungeCharge = 0;
        yield return new WaitForSeconds(1.5f);      //wind up time for animation
        //add lunge stuff here
        //add force to enemy rigidbody
        GetComponent<Rigidbody>().velocity = transform.position + Vector3.forward;
        currentState = State.Follow;
    }
}