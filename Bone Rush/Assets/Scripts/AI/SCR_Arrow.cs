using UnityEngine;
using System.Collections;
using FMODUnity;

public class SCR_Arrow : MonoBehaviour
{
    [Header("Arrow Variables")]
    private GameObject player;
    [SerializeField] private float thrust = 65f;
    private Vector3 playerLocation;
    public int damage;
    [HideInInspector] private Rigidbody rb;
    [Header("Attacking Variables")]
    private PlayerStats ph;
    private bool hit;
    [Header("FMOD Events")]
    [EventRef] [SerializeField] private string eventFlyBy;          // Played when the arrow is within distance of the player
    [EventRef] [SerializeField] private string eventDamaged;        // Played when arrow hits player
    [EventRef] [SerializeField] private string eventBlock;          // Played when arrow hits player shield
    private bool soundPlayed = false;           // Determines whether the sound will play or not: If false, will play sound when needed, otherwise will not play sound a second time.
    [SerializeField] private GameObject Arrow;          // Needed to attach the sound to the Arrow.
    private float distanceToPlayer;       // Needed to determine when the arrow flyby sound plays.

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        ph = player.GetComponent<PlayerStats>();
        playerLocation = player.transform.position;
        rb = GetComponent<Rigidbody>();
        transform.LookAt(player.transform);
        rb.velocity = transform.forward * thrust;
        StartCoroutine(Destroy_Arrow());
        GetComponent<Collider>().isTrigger = true;
    }

    private void FixedUpdate()
    {
        if(Vector3.Distance(rb.velocity, (transform.forward * thrust) * 0.1f) >= 1)     //makes sure the arrow does not get to slow
        {
            rb.AddForce(-transform.forward * thrust * 0.1f);        //slows the arrow down to give the arrow an arc
        }
        distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);           // Sets the distance of the arrow from the player
        /// FMOD: Plays Arrow FlyBy sound when the distance of the arrow from the player is less than or equal to 10.
        /// Will not play sound if it has already been played - This avoids the sound being played multiple times per arrow.
        if (distanceToPlayer <= 10 && !soundPlayed)
        {
            RuntimeManager.PlayOneShotAttached(eventFlyBy, Arrow);
            soundPlayed = true;
        }
    }

    private void OnTriggerStay(Collider collision)      //hit detection for the arrow   
    {
        // Explosive arrows?
        /*GameObject PS = GetComponentInChildren<ParticleSystem>().gameObject;
        PS.transform.parent = null;
        PS.GetComponent<ParticleSystem>().Play();*/
        if ((collision.CompareTag("Player") || collision.CompareTag("Shield")) && !collision.CompareTag("Sword") && !hit)
        {
            RuntimeManager.PlayOneShot(eventDamaged);               // Plays PlayerHurt SFX
            hit = true;
            if (collision.name == "PlayerShield" && collision.GetComponentInParent<SwordThings>().isBlocking)       //checks if the player is blocking
            {
                RuntimeManager.PlayOneShot(eventBlock);              // Plays ShieldBlock SFX
                Destroy(gameObject);    //destorys arrow on contact with shield
            }
            else       //checks if the player is not blocking
            {
                ph.PlayerDamage(damage);    //damages player
                Destroy(gameObject);    //destorys arrow on contact with the player
            }
        }
        else if (collision.gameObject.tag == "Enemy")
        {
            Destroy(gameObject);     //destorys arrow on contact with another enemy
        }
        else
        {
            Destroy(gameObject);    //destorys arrow on contact with anything else 
        }
    }

    private IEnumerator Destroy_Arrow()
    {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);        //deletes the arrow after 5 seconds, to stop a build up of gameobjects in the scene
    }
}