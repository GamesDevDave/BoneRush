using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static int stamina { get; set; }
    public static int health { get; set; }

    private static Slider ph; //Player Health
    private static Slider ps; //Player Stamina
    private static Slider xp;
    public Animator anim;

    private static Slider bh; //Boss Health

    public GameObject objectCanvasPRF;

    public GameObject objectOutlinePRF;

    public static float enemyStatModifier { get; set; }

	public bool mainGM;
    [SerializeField]
    GameObject bosshealthbar;

    // Start is called before the first frame update

    void OnEnable()
    { 
        if (GameObject.FindGameObjectsWithTag("GameManager").Length <= 1)
        {
			enemyStatModifier = 1;
            SceneManager.sceneLoaded += OnSceneLoaded;
            health = 100;
            stamina = 100;  
            DontDestroyOnLoad(gameObject);
            mainGM = true;
        }
        else
        {
            Destroy(gameObject);
        }
    }


    void Start()
    {
        if (mainGM)
        {
            stamina = GetComponent<PlayerStats>().TransferPlayerMaxStamina();
            health = GetComponent<PlayerStats>().TransferPlayerMaxHP();
        }
        anim = GetComponent<Animator>();
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) // This runs each time a new scene is loaded
    {
        if (mainGM)
        {
            if (SceneManager.GetActiveScene().buildIndex == 7)
            {
                bosshealthbar.SetActive(true);
                Debug.Log(bosshealthbar);
                bh = GameObject.Find("BossHealthSlider").GetComponent<Slider>();
                bh.maxValue = BossStats.TransferBossMaxHP();
                bh.value = BossStats.TransferBossHP();
            }
            // If not in Boss SCN, the Boss MSC will stop.
            else
            {
                // Debug.Log("Stopping instance.");
                GetComponent<SCR_Boss_Transition>().StopInstance();     // Gets script component of GameManager then calls StopInstance function within script
            }

            //Debug.Log(scene.name);
            //Debug.Log(scene == SceneManager.GetSceneByName("BOSS_BLOCKOUT"));
            Instantiate(objectCanvasPRF);
            ph = GameObject.Find("PlayerHealthSlider").GetComponent<Slider>(); // Assigns the player sliders from the UI in the scene to "ph" (this scripts instance of player health)
            ps = GameObject.Find("PlayerStaminaSlider").GetComponent<Slider>(); // Assigns the player sliders from the UI in the scene to "ps" (this scripts instance of player stamina)
            xp = GameObject.Find("PlayerEXPBar").GetComponent<Slider>();
            UpdatePlayerEXP(GetComponent<PlayerStats>().TransferXPPool());
            UpdatePlayerEXPNeeded(GetComponent<PlayerStats>().TransferXPNeeded());
            UpdatePlayerMaxHealth(GetComponent<PlayerStats>().TransferPlayerMaxHP());
            UpdatePlayerMaxStamina(GetComponent<PlayerStats>().TransferPlayerMaxStamina());
        }
    }

    public static void UpdatePlayerHealth(int newHealth)
    {
        ph.value = newHealth;
    }

    public static void UpdatePlayerStamina(int newStamina)
    {
        ps.value = newStamina;
    }

    public static void UpdatePlayerMaxHealth(int newHealth)
    {
        ph.maxValue = newHealth;
    }

    public static void UpdatePlayerMaxStamina(int newStamina)
    {
        ps.maxValue = newStamina;
    }

    public static void UpdatePlayerEXPNeeded(int newXPNeeded)
    {
        xp.maxValue = newXPNeeded;
        // Debug.Log("newXPNeeded: " + newXPNeeded);
    }

    public static void UpdatePlayerEXP(int newXP)
    {
        xp.value = newXP;
    }

    public static void UpdateBossHealth(int newHealth)      //sets value of the slider
    {
        if(bh != null)
        {
            bh.value = newHealth;
        }
    }

    public static void UpdateBossMaxHealth(int maxHealth)
    {
        if(bh != null)
        {
            bh.maxValue = maxHealth;
        }
    }

    public void Update()
    {
        //Debug.Log("boss health (slider):" + bh.value); //value of the slider

        //anim.SetFloat("AnimTrigger", stamina);
        //Debug.Log(stamina);
        if(stamina <= 35)
        {
            anim.Play("StamAnimFlash");
        }
    }
}
