using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Player_Stats", menuName = "ScriptableObjects/Stats/Player_Stats", order = 3)]
public class SCR_ScriptablePlayerStats : ScriptableObject
{
	[Header("General Stats: ")]
	public int maxHealth;
	public int maxStamina;
	public int normalDamage;
	public int heavyDamage;
	public int xpLevelModifier;
	public int staminaCostToShield;
	public int staminaPickUpAmount;
	public int staminaRegenRate;
	public int dashDistance;
	public int staminaCostToDash;
	public int shieldStunDuration;
	public float shieldDamage;
	public int jumpHeight;
	public int staminaCostToJump;
	public int speed; // Move speed
	public float shieldReduction; // e.g. 0.6f = 60% of damage gets through
	public float projectileSpeed;
}
