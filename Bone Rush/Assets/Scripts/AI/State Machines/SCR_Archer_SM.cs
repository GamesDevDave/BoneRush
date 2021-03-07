//Base state machine adapted from https://unity3d.college/2019/04/28/unity3d-ai-with-state-machine-drones-and-lasers/  
//Code to make arrows spawn in front of archer https://answers.unity.com/questions/772331/spawn-object-in-front-of-player-and-the-way-he-is.html
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using FMOD.Studio;
using FMODUnity;

public class SCR_Archer_SM : MonoBehaviour
{
    [Header("Archer Variables")]
    [SerializeField] private GameObject[] retreatLocation;
    public State currentState;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private float seeDistance = 25f;
    private float retreatDistance = 10f;
    private int setPath = 0;
    private GameObject player;
    private bool reloading;
    private EventInstance eventInst;
    private Vector3 archerPosition;
    private Vector3 archerDirection;
    private Vector3 enemyDestination;
    private Vector3 currentLocation;
    private Vector3 distanceToRetreatPoint;
    [SerializeField] private float reloadtime = 2;
    private float distance_to_player;
    private EnemyStats ES;
    [Header("Arrow Variables")]
    [SerializeField] private GameObject arrow;
    private Vector3 arrowSpawn;
    [Header("Attacking Variables")]
    private bool stunned = false;
    [EventRef] [SerializeField] private string eventDamaged;        //FMOD: Played when archer takes damage

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
                        distance_to_player = Vector3.Distance(transform.position, player.transform.position);     //check distance to player
                        if (distance_to_player <= seeDistance && transform.position.y + 3 >= player.transform.position.y)
                        {
                            currentState = State.Shoot;     //shoots at player if they become to close
                        }
                        break;
                    }
                //spawns an arrow and shoots it at the player
                case State.Shoot:
                    {
                        //transform.rotation = Quaternion.SlerpUnclamped(transform.rotation, player.transform.rotation, 1);
                        transform.LookAt(player.transform);
                        distance_to_player = Vector3.Distance(transform.position, player.transform.position);
                        //archers will not shoot players above them or if the player is out of their see distance
                        if (distance_to_player >= seeDistance || transform.position.y + 3 < player.transform.position.y)      
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
                            StartCoroutine(ReloadTime(reloadtime));
                        }
                        break;
                    }
                //runs from the player if they get too close
                case State.Retreat:
                    {
                        Retreating();
                        break;
                    }
                //happens when the enemy takes damage
                case State.Damaged:
                    {
                        eventInst = RuntimeManager.CreateInstance(eventDamaged);       // FMOD: Grabs our event instance and creates a new event instance based on the string we called
                        eventInst.start();                                             // Starts event instance
                        if (ES.EnemyHealth <= 0) currentState = State.Death;   //check if the enemy is still alive
                        else if (reloading) currentState = State.Reload;       //stops the enemy from retreating while reloading
                        else currentState = State.Retreat;      //being hit causes the enemy to try and retreat
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
        Retreat,
        Damaged,
        Death,
        Stunned
    }

    private void ShootArrow()
    {
        archerPosition = transform.position;       //gets the location of the archer
        archerPosition[1] = archerPosition[1] + 1;        //increases the y value to get the arrow in line with the bow
        archerDirection = transform.forward;       //face the arrow in the forward direction
        arrowSpawn = archerPosition + archerDirection * 1f;      //combines the arrows direction and location
        GameObject obj = Instantiate(arrow, arrowSpawn, Quaternion.identity);      //spwans the arrow
        obj.GetComponent<SCR_Arrow>().damage = GetComponent<EnemyStats>().TransferEnemyDamage();
        currentState = State.Reload;
    }

    private void Retreating()
    {
        if (setPath == retreatLocation.Length)        //stops the enemy from retreating when it is at the end of its paths
        {
            currentState = State.Shoot;
        }
        else
        {
            enemyDestination = retreatLocation[setPath].transform.position;      //updates the enemy to its new location
            navMeshAgent.SetDestination(enemyDestination);        //tells the enemy to go to its new location
            currentLocation = transform.position;
            distanceToRetreatPoint = enemyDestination - currentLocation;    //checks if the enemy has hit its new location
            if (distanceToRetreatPoint.magnitude < 2)
            {
                setPath += 1;      //sets the enemy to its new location
                currentState = State.SpotPlayer;       //enemy will now search for the player again
            }
        }
    }

    private IEnumerator ReloadTime(float time)
    {
        reloading = true;
        yield return new WaitForSeconds(time);        //time it takes to reload
        distance_to_player = Vector3.Distance(transform.position, player.transform.position);
        if (distance_to_player <= retreatDistance)
        {
            currentState = State.Retreat;      //runs away from the player is they are close after reloading
            reloading = false;
        }
        else
        {
            currentState = State.SpotPlayer;       //goes back to spotting the player once reloaded
            reloading = false;
        }

    }

    private IEnumerator Stunned(float time)
    {
        stunned = true;
        yield return new WaitForSeconds(time);
        stunned = false;
        currentState = State.Retreat;
    }
}