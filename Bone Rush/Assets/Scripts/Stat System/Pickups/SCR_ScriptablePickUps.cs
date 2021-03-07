using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PickUp", menuName = "Pickups/Pickup", order = 1)]
public class SCR_ScriptablePickUps : ScriptableObject
{
    public bool effectsStamina;
    public bool effectsHealth;
}
