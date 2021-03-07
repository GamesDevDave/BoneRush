using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Boss_Stats", menuName = "ScriptableObjects/Stats/Boss_Stats", order = 1)]
public class SCR_ScriptableBossStats : ScriptableObject
{
	[Header("General Stats: ")]
	public string name;
	public float healthIncreaseRate; // How often health will increase
	public float statIncreaseRate; // How often stats are increased
	public float statIncreaseModifier; // How much the stats are increased by

	
	[Header("Before buffs, stats will at start be: ")]
	[Space(15)]
	public int health;
    public float damage;
    public float arrowSpeed;
    public float speed; // Move speed
    public float attackDelay; // Attack speed for sword and includes arrow reload time
    public float GroundPoundDamage;
    public float RecoverTime; // Rest time

	
	[Header("After buffs, stats will at most be: ")]
	[Space(15)]
	public int healthMax;
	public float damageMax;
	public float arrowSpeedMax;
	public float speedMax; // Move speed
	public float attackDelayMin; // Attack speed for sword and includes arrow reload time
	public float GroundPoundDamageMax;
	public float RecoverTimeMin; // Rest time
}
