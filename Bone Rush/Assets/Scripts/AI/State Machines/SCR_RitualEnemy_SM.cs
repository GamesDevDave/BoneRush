//Base state machine adapted from https://unity3d.college/2019/04/28/unity3d-ai-with-state-machine-drones-and-lasers/  
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class SCR_RitualEnemy_SM : MonoBehaviour
{
    [Header("References")]
    private GameObject playerReference;
    private GameObject bossReference;
    public State currentState;
    private NavMeshAgent navMeshAgent;
    private bool stunned;
    private bool hasAttacked;
    public bool attacking;
    private Animator enemyAnim;
    private float attackRate = 1f;
    private Vector3 rotationClamp;
    private float distanceToPlayer;
    private Vector3 playerLocation;
    private EnemyStats ES;
    [Header("Bools")]
    public bool currentlyChanelling = true;
    private bool canFollow = false;
    [Header("Values")]
    [SerializeField] private float distanceFromPlayer;
    [SerializeField] private float followDistance = 10f;
    [SerializeField] private float attackDistance = 2f;
    [SerializeField] private float followDelay = 2f;
    [SerializeField] private float spinSpeed = 2f;

    private void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        currentState = State.Chanelling;               // Sets the default state to channeling.
        enemyAnim = GetComponentInChildren<Animator>();
        playerReference = GameObject.FindGameObjectWithTag("Player");
        bossReference = GameObject.FindGameObjectWithTag("Boss");
        ES = GetComponent<EnemyStats>();
    }

    private void Update()
    {
        if (!stunned)
        {
            // Detecting the distance from the enemy to the player. Used to check for follow and attack distance.
            distanceFromPlayer = Vector3.Distance(this.transform.position, playerReference.transform.position);
            switch (currentState)
            {
                //the enemy powers up the boss
                case State.Chanelling:
                    {
                        navMeshAgent.isStopped = true;
                        // Ensures the enemy is looking towards the boss, and clamps it so that the enemy doesn't tilt.
                        this.gameObject.transform.LookAt(bossReference.transform);
                        rotationClamp = new Vector3(Mathf.Clamp(transform.eulerAngles.x, 0, 0), transform.eulerAngles.y, Mathf.Clamp(transform.eulerAngles.z, 0, 0));
                        transform.localRotation = Quaternion.Euler(rotationClamp);
                        if (distanceFromPlayer < followDistance)      // Checks for a change in distance resulting in a state change to follow.
                        {
                            currentlyChanelling = false;
                            currentState = State.Delayed;
                        }
                        break;
                    }
                //the enemy attack the player
                case State.Attack:
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, playerReference.transform.rotation, spinSpeed * Time.deltaTime);
                        distanceToPlayer = Vector3.Distance(transform.position, playerReference.transform.position);
                        if (distanceToPlayer >= 1.5) currentState = State.Follow;
                        if (hasAttacked == false)
                        {
                            attacking = true;
                            enemyAnim.SetBool("isAttacking", true);
                            hasAttacked = true;
                            StartCoroutine(AttackDelay());      //delay on the enemy attack to make sure it is not spammed 
                        }
                        break;
                    }
                //the enemy follows the player after a short delay 
                case State.Follow:
                    {
                        transform.rotation = Quaternion.RotateTowards(transform.rotation, playerReference.transform.rotation, spinSpeed * Time.deltaTime);
                        hasAttacked = false;
                        if (canFollow)        // Checks if the enemy has waited until it can follow. (Defined in the Coroutine at the bottom)
                        {
                            navMeshAgent.isStopped = false;
                            Following();     // Continues following the player.
                            if (distanceFromPlayer < attackDistance && !hasAttacked) currentState = State.Attack;  // Checks if the player is within attack distance.
                        }
                        break;
                    }
                //happens when the enemy takes damage
                case State.Damaged:
                    {
                        TakeDamage();
                        break;
                    }
                //deletes the enemy gameobject 
                case State.Death:
                    {
                        Destroy(gameObject);
                        break;
                    }
                //makes enemies stay still after being shield bashed
                case State.Stunned:
                    {
                        navMeshAgent.SetDestination(transform.position);
                        if (!stunned)
                        {
                            StartCoroutine(Stunned(3));
                        }
                        break;
                    }
                //delays the enemy to follow the player
                case State.Delayed:
                    {
                        StartCoroutine(WaitForFollow());
                        break;
                    }
            }
        }
    }

    public enum State     // Sets up the different states.
    {
        Chanelling,
        Attack,
        Follow,
        Damaged,
        Death,
        Stunned,
		Delayed
    }

    private void TakeDamage()
    {
        if (ES.EnemyHealth <= 0) currentState = State.Death; //check if the enemy is still alive
        else currentState = State.Follow;
    }

    private IEnumerator WaitForFollow()
    {
        // Rotates the enemy to face the player.
        transform.rotation = Quaternion.RotateTowards(transform.rotation, playerReference.transform.rotation, spinSpeed * Time.deltaTime);
        // Adds a delay so that the player does not instantly get targeted after entering follow range.
        yield return new WaitForSeconds(followDelay);
        currentState = State.Follow;
        canFollow = true;
    }

    private IEnumerator Stunned(float time)
    {
        stunned = true;
        yield return new WaitForSeconds(time);
        stunned = false;
        currentState = State.Follow;
    }

    private void Following()
	{
		enemyAnim.SetBool("isMoving", true);
		distanceToPlayer = Vector3.Distance(transform.position, playerReference.transform.position);
		playerLocation = playerReference.transform.position;
		hasAttacked = false;
		if (distanceToPlayer > 1.5) navMeshAgent.SetDestination(playerLocation);
		else navMeshAgent.SetDestination(transform.position);
	}

    private IEnumerator AttackDelay()
    {
        yield return new WaitForSeconds(attackRate);
        attacking = false;
        enemyAnim.SetBool("isAttacking", false);
        currentState = State.Follow;
    }
}
