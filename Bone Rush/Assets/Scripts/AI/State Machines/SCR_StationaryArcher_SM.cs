//Base state machine adapted from https://unity3d.college/2019/04/28/unity3d-ai-with-state-machine-drones-and-lasers/  
//Code to make arrows spawn in front of archer https://answers.unity.com/questions/772331/spawn-object-in-front-of-player-and-the-way-he-is.html
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using FMODUnity;
using FMOD.Studio;

public class SCR_StationaryArcher_SM : MonoBehaviour
{
    [Header("Archer Variables")]
    public State currentState;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float seeDistance = 50f;
    private GameObject player;
    private bool reloading;
    private float distanceToPlayer;
    private Vector3 archerPosition;
    private Vector3 archerDirection;
    private bool stunned = false;
    [SerializeField] private float reloadTime = 2;
    private EnemyStats ES;
    [Header("Arrow Reference")]
    [SerializeField] private GameObject arrow;
    private Vector3 arrowSpawn;
    [Header("FMOD Variables")]
    [EventRef] [SerializeField] private string eventDamaged;
    EventInstance eventInst;

    private void Start()
    {
        player = GameObject.FindWithTag("Player");
        navMeshAgent = GetComponent<NavMeshAgent>();
        ES = GetComponent<EnemyStats>();
    }

    private void Update()
    {
        if (!stunned)
        {
            switch (currentState)
            {
                //in this state the enemy stays still and tooks out for the player
                //will shoot the player if they get too close
                case State.SpotPlayer:
                    {
                        //raycast?
                        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);   //check distance to player
                        if (distanceToPlayer <= seeDistance && transform.position.y + 3 > player.transform.position.y)
                        {
                            currentState = State.Shoot; //shoots at player if they become to close
                        }
                        break;
                    }
                //spawns an arrow
                case State.Shoot:
                    {
                        transform.LookAt(player.transform);
                        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
                        //archers will not shoot players above them or if the player is out of their see distance
                        if (distanceToPlayer >= seeDistance || transform.position.y + 3 < player.transform.position.y)
                        {
                            currentState = State.SpotPlayer;
                        }
                        else
                        {
                            ShootArrow();       //spawns an arrow
                        }
                        break;
                    }
                //reloads bow   
                case State.Reload:
                    {
                        if (!reloading)
                        {
                            StartCoroutine(ReloadTime(reloadTime));
                        }
                        break;
                    }
                //happens when the ememy takes damage
                case State.Damaged:
                    {
                        eventInst = RuntimeManager.CreateInstance(eventDamaged);       // FMOD: Grabs our event instance and creates a new event instance based on the string we called
                        eventInst.start();                                           // Starts event instance
                        if (ES.EnemyHealth <= 0) currentState = State.Death;     //check if the enemy is still alive
                        else if (reloading) currentState = State.Reload;       //stops the enemy from retreating while reloading
                        else currentState = State.Shoot;
                        break;
                    }
                //destorys the enemy gameobject
                case State.Death:
                    {
                        Destroy(gameObject);
                        break;
                    }
                //makes enemies stunned after being shield bashed
                case State.Stunned:
                    {
                        navMeshAgent.SetDestination(transform.position);
                        if (!stunned)
                        {
                            StartCoroutine(Stunned(3));
                        }
                        break;
                    }
            }
        }
    }

    public enum State
    {
        SpotPlayer,
        Shoot,
        Reload,
        Damaged,
        Death,
        Stunned
    }

    private void ShootArrow()
    {
        archerPosition = transform.position;       //gets the location of the archer
        archerPosition[1] = archerPosition[1] + 1;        //increases the y value to get the arrow in line with the bow
        archerDirection = transform.forward;       //face the arrow in the forward direction
        arrowSpawn = archerPosition + archerDirection * 1.1f;      //combines the arrows direction and location
        GameObject obj = Instantiate(arrow, arrowSpawn, Quaternion.identity);      //spwans the arrow
        obj.GetComponent<SCR_Arrow>().damage = GetComponent<EnemyStats>().TransferEnemyDamage();
        currentState = State.Reload;
    }

    private IEnumerator ReloadTime(float time)
    {
        reloading = true;
        yield return new WaitForSeconds(time);      //time it takes to reload
        reloading = false;
        currentState = State.SpotPlayer;       //goes back to spotting the player once reloaded
    }

    private IEnumerator Stunned(float time)
    {
        stunned = true;
        yield return new WaitForSeconds(time);
        stunned = false;
        currentState = State.Shoot;
    }
}