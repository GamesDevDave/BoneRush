using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class old_PlayerHealth : MonoBehaviour
{
    /*public int playerHealth = 100;
    public int damageTaken = 25;
    public Slider healthSlider;

    public SCR_Boss_SM Boss;
    public bool collided;
    public bool canCollide;

    //checks to see if the player has been attacked the boss

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Boss" && Boss.swing.GetBool("Attacking"))//and boss is doing swing animation
        {
            Debug.Log("player damaged by boss"); //still triggers if player sword hit the boss
            playerHealth -= damageTaken;
        }
        if (playerHealth <= 0)
        {
            SceneManager.LoadScene("SCN_Menu_Defeat");
        }
    }

    void Start()
    {
        playerHealth = GameManager.health;
        collided = false;
    }

    void OnParticleCollision()
    {
        Debug.Log("Here");
        if (!collided && canCollide)
        {
            collided = true;
            //GameObject.FindGameObjectWithTag("Boss").GetComponent<Boss>().GroundPoundCheck();
        }
    }

    private void Update()
    {
        healthSlider.value = playerHealth;
        if (playerHealth <= 0)
        {
            SceneManager.LoadScene("SCN_Menu_Defeat");
        }
    }*/
}
    