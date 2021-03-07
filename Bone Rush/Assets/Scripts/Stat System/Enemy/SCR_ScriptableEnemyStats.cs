using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Enemy_Stats", menuName = "ScriptableObjects/Stats/Enemy_Stats", order = 2)]
public class SCR_ScriptableEnemyStats : ScriptableObject
{
	[Header("General Stats: ")]
    public int health;
    public int damage;
    public float shieldReduction; // e.g. 0.6f = 60% of damage gets through
    public int xpDrop;
    public float arrowSpeed;
    public float speed; // Move speed
    public float seeDistance;
    public float attackRate; // Attack speed for sword and includes arrow reload time
    public bool Can_Kick;
    public bool Can_ShieldBash;
}
