using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    //serialies enemy stats
    public int EnemyHealth;
    private int EDamage;
    private float EnemyShield;
    private int XpDrop;
    private float MoveSpeed;
    private float arrowSpeed;
    private float see_distance;
    private float attackRate;
	bool dying;
    public bool Can_Kick;
    public bool Can_ShieldBash;

    [SerializeField]
    SCR_ScriptableEnemyStats stats;

    float gracePeriod;

    [SerializeField]
    PlayerStats player;

    void Start()
    {
        if(stats != null)
        {
            InitStats();
        }
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerStats>();
        }
    }

    void InitStats()
    {
        EnemyHealth = stats.health;
        EDamage = stats.damage;
        EnemyShield = stats.shieldReduction;
        XpDrop = stats.xpDrop;
        MoveSpeed = stats.speed;
        arrowSpeed = stats.arrowSpeed;
        see_distance = stats.seeDistance;
        attackRate = stats.attackRate;
        Can_Kick = stats.Can_Kick;
        Can_ShieldBash = stats.Can_ShieldBash;
    }

    void Update()
    {
        if (gracePeriod >= 0)
        {
            gracePeriod -= Time.deltaTime;
        }
    }

    //transfer damage to melee hit detection
    public int TransferEnemyDamage()
    {
        return EDamage;
    }

    public float TransferArrowSpeed()
    {
        return arrowSpeed;
    }

    //basic damage against the enemy
    public void EnemyDamage(int Damage)
    {
        if(gracePeriod <= 0)
        {
            EnemyHealth -= Damage;
            gracePeriod = .5f;
            EnemyDeath();
        }
    }

    //enemy damage calculations if enemy is blocking
    public void EnemyShieldDamage(int Damage)
    {
        EnemyHealth -= Mathf.RoundToInt(Damage * EnemyShield);
        EnemyDeath();
    }

    // check if enemy is dead
    void EnemyDeath()
    {
        if (EnemyHealth <= 0 && !dying)
        {
			dying = true;
            // player.PlayerHealing(3); // Playtesting heal player
            //send experiance points to the player
            player.GetXp(XpDrop);
        }
    }
}
