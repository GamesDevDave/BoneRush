using UnityEngine;
using FMODUnity;

public class SCR_PlayerLevelUp : MonoBehaviour
{
    //get player script
    PlayerStats playerstats;

    //create modifier variables
    [SerializeField]
    private float PlayerHealthModifer;
    [SerializeField]
    private float PlayerStaminaModifer;
    [SerializeField]
    private float PlayerShieldModifer;
    [SerializeField]
    private float PlayerDamageModifer;
    [SerializeField]
    private float PlayerHeavyDamageModifer;
    [SerializeField]
    private float DashDistanceModifier;
    [SerializeField]
    private float StaminaRegenModifier;
    [SerializeField]
    private float JumpDistanceModifier;
    [SerializeField]
    private float SpeedModifier;

    //SpeedBuild
    [SerializeField]
    private float SBStaminaRegenModifier;
    [SerializeField]
    private float SBStaminaModifier;
    [SerializeField]
    private float SBDashDistanceModifier;
    [SerializeField]
    private float SBJumpDistanceModifier;
    [SerializeField]
    private float SBSpeedModifier;

    //Damage Build
    [SerializeField]
    private float DBPlayerDamageModifier;
    [SerializeField]
    private float DBPlayerHeavyDamageModifier;

    //Heavy Build
    [SerializeField]
    private float HBPlayerHealthModifier;
    [SerializeField]
    private float HBShieldModifier;


    //seperate value to hold part of the new value
    private float TempValueFloat;
    private int TempValueInt;

    // FMOD:
    [EventRef] [SerializeField] string eventLevelUp;     // Played when player levels up   

    //update players values on level up
    public int NewStatsHealth(int Health)
    {
        RuntimeManager.PlayOneShot(eventLevelUp);                       // FMOD: Plays LevelUp sound
        TempValueFloat = Health * PlayerHealthModifer;
        TempValueFloat += Health;
        TempValueInt = (int)TempValueFloat;
        return (TempValueInt);
    }

    public float NewStatsStaminaRegen(float StaminaRegen)
    {
        TempValueFloat = StaminaRegen * StaminaRegenModifier;
        TempValueFloat += StaminaRegen;
        return (TempValueFloat);
    }
    public int NewStatsStamina(int Stamina)
    {
        TempValueFloat = Stamina * PlayerStaminaModifer;
        TempValueFloat += Stamina;
        TempValueInt = (int)TempValueFloat;
        return (TempValueInt);
    }
    public float NewStatsShield(float Shield)
    {
        TempValueFloat = Shield * PlayerShieldModifer;
        TempValueFloat += Shield;
        return (TempValueFloat);
    }
    public int NewStatsDamage(int Damage)
    {
        TempValueFloat = Damage * PlayerDamageModifer;
        TempValueFloat += Damage;
        TempValueInt = (int)TempValueFloat;
        return (TempValueInt);
    }
    public int NewStatsHeavyDamage(int HeavyDamage)
    {
        TempValueFloat = HeavyDamage * PlayerHeavyDamageModifer;
        TempValueFloat += HeavyDamage;
        TempValueInt = (int)TempValueFloat;
        return (TempValueInt);
    }
    public float NewStatsDash(float Dash)
    {
        TempValueFloat = Dash * DashDistanceModifier;
        TempValueFloat += Dash;
        return (TempValueFloat);
    }
    public float NewStatsJump(float Jump)
    {
        TempValueFloat = Jump * JumpDistanceModifier;
        TempValueFloat += Jump;
        return (TempValueFloat);
    }
    public float NewStatsSpeed(float Speed)
    {
        TempValueFloat = Speed * SpeedModifier;
        TempValueFloat += Speed;
        return (TempValueFloat);
    }

    //New level up Threshold
    public int NewLevelThreshold(int OldThreshold)
    {
        TempValueFloat = Mathf.RoundToInt((2* OldThreshold) + 8);       // Using XP Requirement function f: y = 0.08x^(2)+10
        TempValueInt = (int)TempValueFloat;
        return (TempValueInt);
    }

    public void CharacterBuild(int Build)
    {
        if (Build == 1)
        {
            // Speed Build
            StaminaRegenModifier = SBStaminaRegenModifier;
            PlayerStaminaModifer = SBStaminaModifier;
            DashDistanceModifier = SBDashDistanceModifier;
            JumpDistanceModifier = SBJumpDistanceModifier;
            SpeedModifier = SBSpeedModifier;
        }

        if (Build == 2)
        {
            // Damage Build
            PlayerDamageModifer = DBPlayerDamageModifier;
            PlayerHeavyDamageModifer = DBPlayerHeavyDamageModifier;
        }

        if (Build == 3)
        {
            //Heavy Build
            PlayerHealthModifer = HBPlayerHealthModifier;
            PlayerShieldModifer = HBShieldModifier;
        }
     }
}
