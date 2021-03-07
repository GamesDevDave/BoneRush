using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using TMPro;

public class BossStats : MonoBehaviour
{
    //Boss health
    public static int BossMaxHealth;
	static int BossInitialMaxHealth;
	public static float BossCurrentHealth;

	//Boss Damage
	static float BDamage = 15;

	//More stats
	static float GroundPoundDamage = 25;
	static float healthIncreaseRate = 1;
	static float RecoverTime = 2.5f;
	static float speed = 3.5f;
	static float projectileSpeed;
	static float attackRate = 2f;
	static string name = "Tormir Von Ludwig";
	float statIncreaseRate = 1f;
	float statIncreaseModifier = 1f;
	public static bool initStatIncrease;

	//After buff max stats
	static float Max_Damage = 15;
	static float Max_GroundPoundDamage = 25;
	static float Min_RecoverTime = 2.5f;
	static float Max_speed = 3.5f;
	static float Max_projectileSpeed = 10;
	static float Min_attackRate = 2f;

    //Values containing 1% of the difference between initial stat and max value of a stat
    float attackRateDif;
    float groundPoundDamageDif;
    float projSpeedDif;
    float bDamageDif;
    float speedDif;
    float recoveryDif;

    //Debug value
    float percentPower;

	[SerializeField]
    SCR_ScriptableBossStats stats;

	static GameObject boss;

    void Start()
    {
        if (stats != null)
        {
            InitStats();
        }
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		if(scene.name == "BOSS_BLOCKOUT")
		{
			boss = GameObject.FindGameObjectWithTag("Boss");
			boss.GetComponent<NavMeshAgent>().speed = speed;
			GameObject.Find("BossText").GetComponent<TextMeshProUGUI>().text = name;
		}
	}

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

	void InitStats()
	{
		name = stats.name;
		statIncreaseRate = stats.statIncreaseRate;
        statIncreaseModifier = stats.statIncreaseModifier;
		healthIncreaseRate = stats.healthIncreaseRate;
		BossInitialMaxHealth = stats.health;
		BossMaxHealth = stats.health;
		BDamage = stats.damage;
		speed = stats.speed;
		projectileSpeed = stats.arrowSpeed;
		attackRate = stats.attackDelay;
		GroundPoundDamage = stats.GroundPoundDamage;
		RecoverTime = stats.RecoverTime;

		Max_GroundPoundDamage = stats.GroundPoundDamageMax;
		Max_projectileSpeed = stats.arrowSpeedMax;
		Max_speed = stats.speedMax;
		Min_attackRate = stats.attackDelayMin;
		Min_RecoverTime = stats.RecoverTimeMin;
		Max_Damage = stats.damageMax;

        attackRateDif = Get1PercentOfDifference(Min_attackRate, attackRate);
        groundPoundDamageDif = Get1PercentOfDifference(Max_GroundPoundDamage, GroundPoundDamage);
        projSpeedDif = Get1PercentOfDifference(Max_projectileSpeed, projectileSpeed);
        bDamageDif = Get1PercentOfDifference(Max_Damage, BDamage);
        speedDif = Get1PercentOfDifference(Max_speed, speed);
        recoveryDif = Get1PercentOfDifference(Min_RecoverTime, RecoverTime);
    }

    //Gets 1 percent of the difference between v1 and v2
    float Get1PercentOfDifference(float v1, float v2)
    {
        return (v1 - v2) / 100;
    }

	private void Update()
	{
		if (initStatIncrease)
		{
			InvokeRepeating("IncreaseStats", statIncreaseRate, statIncreaseRate);
			initStatIncrease = false;
		}
	}

	void IncreaseStats()
	{
        percentPower += 1 * statIncreaseModifier;
        Debug.Log("Boss is at " + Mathf.RoundToInt(percentPower) + "% of his maximum power!");

		if(attackRate > Min_attackRate)
		{
			attackRate += attackRateDif * statIncreaseModifier;
		}

		if(GroundPoundDamage < Max_GroundPoundDamage)
		{
			GroundPoundDamage += groundPoundDamageDif * statIncreaseModifier;
		}

		if(projectileSpeed < Max_projectileSpeed)
		{
			projectileSpeed += projSpeedDif * statIncreaseModifier;
		}

		if(BDamage < Max_Damage)
		{
			BDamage += bDamageDif * statIncreaseModifier;
		}

		if(speed < Max_speed)
		{
			speed += speedDif * statIncreaseModifier;
		}

		if(RecoverTime > Min_RecoverTime)
		{
			RecoverTime += recoveryDif * statIncreaseModifier;
		}
	}

	//transfer damage to melee hit detection
	public static float TransferBossDamage()
    {
        return BDamage;
    }

	public static float TransferHealthIncreaseRate()
	{
		return healthIncreaseRate;
	}

    public static float TransferAttackRate()
    {
        return attackRate;
    }

    public static float GroundPoundCheck()
    {
        return GroundPoundDamage;
    }
	
	public static float TransferProjectileSpeed()
	{
		return projectileSpeed;
	}

    public static float TransferRecoveryTime()
    {
        return RecoverTime;
    }

    public static void IncreaseMaxHealth(int MaxHealth)
    {
        BossMaxHealth = MaxHealth;
    }

	public static void UpdateBossHP(int hp)
	{
        //Debug.Log("Updating boss hp");
		BossCurrentHealth = hp;
		GameManager.UpdateBossHealth(Mathf.RoundToInt(BossCurrentHealth));
	}

    public static float TransferBossHP()
    {
        return BossCurrentHealth;
    }

	public static float TransferBossInitialMaxHP()
	{
		return BossInitialMaxHealth;
	}

    public static float TransferBossMaxHP()
    {
        return BossMaxHealth;
    }

    //Boss taking damage
    public static void BossDamage(float damage, bool heavyswing)
    {
        if (damage >= 0.05 * BossMaxHealth && heavyswing == false)     //limits player damage to 5% of it's health
        {
            damage = Mathf.RoundToInt(0.05f * BossMaxHealth);
        }
        else if(damage >= 0.10 * BossMaxHealth && heavyswing == true)     //limits player heavy damage to 10% of it's health
        {
            damage = Mathf.RoundToInt(0.10f * BossMaxHealth);
        }
        BossCurrentHealth -= damage;
        GameManager.UpdateBossHealth(Mathf.RoundToInt(BossCurrentHealth));
        BossDeath();
    }

	//check if boss is dead
	static void BossDeath()
    {
        if (BossCurrentHealth <= 0)
        {
            SceneManager.LoadScene("SCN_Menu_Win");
        }
    }

}
