using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using FMODUnity;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    //Main health values
    int PlayerMaxHealth = 100;
	int initialMaxHealth;
	int PlayerMaxStamina = 100;
    float PlayerCurrentHealth = 100;

    //Modifiers to health
    float Shield = 0.6f;

    //Damage values
    int PDamage = 3;
    int PHeavyDamage = 6;

    //Level up values
    int XpPool = 0;
    int NextLevelXp = 1;
	float XpLevelIncreaseMod = 2;
    int CurrentLevel = 1;
    [SerializeField] private int maxLevel = 50;

    //Stamina stats
    float StaminaToShield = 30;
    int StaminaHeal = 50;
    float StaminaRegeneration = 5;
    float DashDistance = 25;
    float StaminaToDash = 30;
    int ShieldStunDuration = 3;
    public float ShieldDamage = .5f;
    float JumpHeight = 30;
    float StaminaForJump = 20;
    float Speed = 10f;
	float projSpeed = 10;

    //pick up values
    private float HealthIncreace;
    private float StaminaIncreace;

    int lastLevelXP = 0;

    BossStats bs;

    public bool collided;
    public bool canCollide;

    public static bool invulnerable { get; set; }

	[SerializeField]
	SCR_ScriptablePlayerStats stats;
    [SerializeField]
    SCR_PlayerLevelUp level;
    [SerializeField]
    private MeleeHitDetection MHD;

    [SerializeField]
    GameObject player;

    [Header("FMOD Events")]
    [EventRef] [SerializeField] private string eventBlock;          // Played when player blocks an enemy attack   
    [EventRef] [SerializeField] private string eventDamaged;        // Played when player takes damage
    [EventRef] [SerializeField] private string eventStamHeal;       // Played when player picks up Stamina Pickup
    [EventRef] [SerializeField] private string eventHealthHeal;     // Played when player picks up Health Pickup

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AssignCurrentStats();
        invulnerable = false;
    }

    //Start is called before the first frame update
    void Start()
    {
        if (GetComponent<GameManager>().mainGM)
        {
            PlayerCurrentHealth = PlayerMaxHealth;
            GameManager.health = PlayerMaxHealth;
            SceneManager.sceneLoaded += OnSceneLoaded;
            if (SceneManager.GetActiveScene().buildIndex == 7)
            {
                bs = GameObject.FindGameObjectWithTag("Boss").GetComponent<BossStats>();
            }
            if (stats != null)
            {
                InitStats();
            }
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

	void InitStats()
	{
        level = GetComponent<SCR_PlayerLevelUp>();
		PlayerMaxHealth = stats.maxHealth;
		PlayerMaxStamina = stats.maxStamina;
		PDamage = stats.normalDamage;
		PHeavyDamage = stats.heavyDamage;
		XpLevelIncreaseMod = stats.xpLevelModifier;
		StaminaToShield = stats.staminaCostToShield;
		StaminaToDash = stats.staminaCostToDash;
		StaminaRegeneration = stats.staminaRegenRate;
		StaminaHeal = stats.staminaPickUpAmount;
		StaminaForJump = stats.staminaCostToJump;
		DashDistance = stats.dashDistance;
		ShieldStunDuration = stats.shieldStunDuration;
		ShieldDamage = stats.shieldDamage;
		JumpHeight = stats.jumpHeight;
		Speed = stats.speed;
		projSpeed = stats.projectileSpeed;
        AssignCurrentStats();
	}

    void AssignCurrentStats()
    {
        MHD = GameObject.FindGameObjectWithTag("Sword").GetComponent<MeleeHitDetection>();
        player = GameObject.FindGameObjectWithTag("Player");
        // Debug.Log(player);
        GameManager.UpdatePlayerMaxHealth(PlayerMaxHealth);
        GameManager.UpdatePlayerMaxStamina(PlayerMaxStamina);
        GameManager.UpdatePlayerEXPNeeded(NextLevelXp);
        GameManager.UpdatePlayerEXP(XpPool - lastLevelXP);
        PlayerStaminaBar psb = player.GetComponent<PlayerStaminaBar>();
        psb.staminaRegen = StaminaRegeneration;
        psb.maxStamina = PlayerMaxStamina;
        psb.dashStamina = StaminaToDash;
        psb.jumpStamina = StaminaForJump;
        player.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().movementSettings.JumpForce = JumpHeight;
        player.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().movementSettings.ForwardSpeed = Speed;
        player.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().movementSettings.BackwardSpeed = Speed / 2;
        player.GetComponent<UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController>().movementSettings.StrafeSpeed = Speed / 2;
        player.GetComponentInChildren<ShieldBashCheck>().stunDuration = ShieldStunDuration;
        player.GetComponent<Dashing>().dashSpeed = DashDistance;
        MHD.GetPlayerValues();
    }

	//Transfer Damage to Melee hit detection
	public int TransferPlayerDamage()
    {
        return PDamage;
    }
    public int TransferPlayerHeavyDamage()
    {
        return PHeavyDamage;
    }

	public int TransferPlayerMaxHP()
	{
		return PlayerMaxHealth;
	}

    public int TransferPlayerMaxStamina()
    {
        return PlayerMaxStamina;
    }
    public int TransferXPPool()
    {
        return XpPool;
    }

    public int TransferXPNeeded()
    {
        return NextLevelXp;
    }

    //basic damage calculation
    public void PlayerDamage(float Damage)
    {
        if (!invulnerable)
        {
            RuntimeManager.PlayOneShot(eventDamaged);
            GameManager.health -= Mathf.RoundToInt(Damage);
            //GameManager.health = (int)PlayerCurrentHealth;
            GameManager.UpdatePlayerHealth(Mathf.RoundToInt(GameManager.health));
            CheckPlayerDeath();
        }
    }

    //damage while guarding
    public void PlayerShieldDamage(float Damage)
    {
        if (!invulnerable)
        {
            // FMOD: Plays ShieldBlock OneShot Event so that the sound for blocking w/ shield is heard.
            RuntimeManager.PlayOneShot(eventBlock);

            // Debug.Log(eventBlock);
            // Debug.Log("BLOCKED");

            //PlayerCurrentHealth -= Damage * Shield;
            GameManager.health -= Mathf.RoundToInt(Damage * Shield);
            //GameManager.health = (int)PlayerCurrentHealth;
            GameManager.UpdatePlayerHealth(Mathf.RoundToInt(GameManager.health));
            CheckPlayerDeath();
        }
    }

    //check if the player is dead
    void CheckPlayerDeath()
    {
        if (GameManager.health <= 0)
        {
            SceneManager.LoadScene(9);
        }
    }

    //Script to heal the player
    public void PlayerHealing()
    {
        // FMOD: Plays HealthHeal SFX
        RuntimeManager.PlayOneShot(eventHealthHeal);

        // Debug.Log("Player healed by " + Heal + " HP");
        // Debug.Log("Player Health before heal: " + PlayerCurrentHealth + " HP");
        //Debug.Log(PlayerMaxHealth);
        //Debug.Log("before " + GameManager.health);
        GameManager.health += Mathf.RoundToInt(PlayerMaxHealth * 0.10f);
        //Debug.Log("Player Max Health" + TransferPlayerMaxHP());
        //Debug.Log("after " + GameManager.health);
        //Debug.LogWarning("Player Max Health Is" + PlayerMaxHealth);
        //Debug.LogWarning(GameManager.health);
        GameManager.health = Mathf.RoundToInt(GameManager.health + (PlayerMaxHealth * 0.05f));
       // Debug.LogWarning(GameManager.health);

        if (GameManager.health > PlayerMaxHealth)
        {
            GameManager.health = PlayerMaxHealth;
        }
        GameManager.UpdatePlayerHealth(Mathf.RoundToInt(GameManager.health));
    }

    //Script to give stamina to the player
    public void PlayerStaminaIncrease()
    {
        // FMOD: Plays StaminaHeal SFX
        RuntimeManager.PlayOneShot(eventStamHeal);

        //Debug.Log(GameManager.stamina);
        GameManager.stamina = Mathf.RoundToInt(GameManager.stamina + (PlayerMaxStamina * 0.25f));
        //Debug.Log(GameManager.stamina);

        GameManager.UpdatePlayerStamina(GameManager.stamina);

        if (GameManager.stamina > PlayerMaxStamina)
        {
            GameManager.stamina = PlayerMaxStamina;
        }
    }

	private void Update()
	{
        /*
		Debug.Log("Player Current Health: " + PlayerCurrentHealth);
		Debug.Log("GameManager Health: " + GameManager.health);
		*/

        // Debug.Log("current xp: " + XpPool);
        // Debug.Log("xp needed for next level: " + NextLevelXp);
        // Debug.Log("current level: " + CurrentLevel);
        if (Input.GetKeyDown(KeyCode.M))
        {
            GetXp(1000);
        }
    }

	void OnParticleCollision(GameObject other)
    {
        if (!collided && canCollide && other.name == "GroundPound")
        {
            collided = true;
            PlayerDamage(BossStats.GroundPoundCheck());
        }
        else if(other.name == "PS")
        {
            PlayerDamage(GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerStats>().TransferPlayerMaxHP() * .1f);
        }
    }

	//get experiance and add it to the xp pool. If reached next level up then run level up scripts.
	public void GetXp (int RecivedXp)
    {
        Debug.Log("Received XP: " + RecivedXp);
        XpPool += RecivedXp;

        GameManager.UpdatePlayerEXP(XpPool);
        while (XpPool >= NextLevelXp && CurrentLevel <= maxLevel - 1)
        {
            Debug.Log("running Level up");
            CurrentLevel = CurrentLevel + 1;
            PlayerMaxHealth = level.NewStatsHealth(PlayerMaxHealth);
            GameManager.UpdatePlayerMaxHealth(PlayerMaxHealth);
			PlayerMaxStamina = level.NewStatsStamina(PlayerMaxStamina);
            GameManager.UpdatePlayerMaxStamina(PlayerMaxStamina);
            Shield = level.NewStatsShield(Shield);
            PDamage = level.NewStatsDamage(PDamage);
            PHeavyDamage = level.NewStatsHeavyDamage(PHeavyDamage);
            DashDistance = level.NewStatsDash(DashDistance);
            JumpHeight = level.NewStatsJump(JumpHeight);
            StaminaRegeneration = level.NewStatsStaminaRegen(StaminaRegeneration);
            Speed = level.NewStatsSpeed(Speed);
            lastLevelXP = NextLevelXp;
            NextLevelXp = level.NewLevelThreshold(CurrentLevel);
            XpPool = 0;
            GameManager.UpdatePlayerEXP(XpPool);
			GameObject.Find("PlayerLVL").GetComponent<TextMeshProUGUI>().text = "LVL " + CurrentLevel;
            AssignCurrentStats();

            //Debug.Log("level for next level: " + NextLevelXp);
        }
    }
}
