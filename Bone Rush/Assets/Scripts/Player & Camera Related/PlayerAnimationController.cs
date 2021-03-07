//Double click script from: https://www.youtube.com/watch?v=GR0XJX1phiw
using System.Collections;
using UnityEngine;
using FMODUnity;

public class PlayerAnimationController : MonoBehaviour
{
    public Animator playerAnim;
    private bool shielding;
    private bool heavySwing;
    private bool shieldBashing;
    private bool swinging;

    private float timeOfFirstClick;
    private float timeBetweenClicks;
    private bool active;
    private int amountOfClicks;

    private MeleeHitDetection MHD;
    private PlayerStaminaBar PSB;
    private ShieldBashCheck SBC;


    [Header("FMOD Events")]
    [EventRef] [SerializeField] private string eventShieldBashStart;            // Called when ShieldBash() is called to play SFX
    [EventRef] [SerializeField] private string eventSwingStart;                 // Called when SwordSwing() is called to play SFX
    [EventRef] [SerializeField] private string eventHeavySwingStart;            // Called when HeavySwing() is called to play SFX

    // Start is called before the first frame update
    private void Start()
    {
        playerAnim = GetComponentInChildren<Animator>();
        timeOfFirstClick = 0;
        timeBetweenClicks = 0.5f;
        active = true;
        amountOfClicks = 0;
        PSB = GetComponent<PlayerStaminaBar>();
        MHD = GetComponentInChildren<MeleeHitDetection>();
        SBC = GetComponentInChildren<ShieldBashCheck>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            amountOfClicks += 1;
            if (amountOfClicks == 1 && active)
            {
                timeOfFirstClick = Time.time;
                StartCoroutine(DoubleClickCheck());     //this is used to check to see if the player has double clicked
            }
        }

        if (Input.GetMouseButtonDown(0) && !shielding && !swinging)
        {
            StartCoroutine(SwordSwing());
        }

        if (Input.GetKeyDown(KeyCode.R) && !shielding && PSB.staminaBar.value > PSB.maxStamina * 0.2)
        {
            if (!heavySwing)
            {
                StartCoroutine(HeavySwing());       //allows the player to heavy swing if they have enough stamina and the cooldown has ended
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            shielding = !shielding;
            if (shielding && PSB.staminaBar.value > PSB.maxStamina * 0.1 && PSB.canBlock)
            {
                ShieldUp();     //raises the player shield if they have enough stamina
            }
            else
            {
                ShieldDown();
            }
        }

        if (Input.GetMouseButtonDown(0) && shielding && PSB.staminaBar.value > PSB.maxStamina * 0.15)
        {
            StartCoroutine(ShieldBash());       //allows the player to shield bash if they have enough stamina and is currently shielding  
        }

        if (shielding)
        {
            //slows the player while shielding
            if (PSB.staminaBar.value < PSB.maxStamina * 0.1)
            {
                ShieldDown();       //makes the player lower their shield if they don't have enough stamina to hold it
                shielding = false;
            }
        }
    }

    private IEnumerator SwordSwing()
    {
        swinging = true;
        RuntimeManager.PlayOneShot(eventSwingStart);     // FMOD: Plays SwordSwing SFX
        playerAnim.SetBool("IsAttack", true);
        yield return new WaitForSeconds(0.8f);
        playerAnim.SetBool("IsAttack", false);
        swinging = false;
    }

    private IEnumerator ComboSwing()
    {
        StopCoroutine(SwordSwing());
        playerAnim.SetBool("IsAttack", true);
        yield return new WaitForSeconds(0);
        playerAnim.SetBool("IsCombo", true);
        yield return new WaitForSeconds(0);
        playerAnim.SetBool("IsAttack", false);
        yield return new WaitForSeconds(0.3f);
        RuntimeManager.PlayOneShot(eventSwingStart);     // FMOD: Plays SwordSwing SFX
        yield return new WaitForSeconds(0.6f);
        playerAnim.SetBool("IsCombo", false);
    }

    private IEnumerator HeavySwing()
    {
        heavySwing = true;
        RuntimeManager.PlayOneShot(eventHeavySwingStart);     // FMOD: Plays SwordSwing SFX
        playerAnim.SetBool("IsHeavySwing", true);
        yield return new WaitForSeconds(1);
        playerAnim.SetBool("IsHeavySwing", false);
        PSB.staminaBar.value -= PSB.maxStamina * 0.2f;
        yield return new WaitForSeconds(5);
        heavySwing = false;
    }

    private void ShieldUp()
    {
        playerAnim.SetBool("IsShielding", true);
        PSB.staminaBar.value = PSB.staminaBar.value - PSB.staminaBar.value * 0.1f;
    }

    private void ShieldDown()
    {
        playerAnim.SetBool("IsShielding", false);
    }

    private IEnumerator ShieldBash()
    {
        RuntimeManager.PlayOneShot(eventShieldBashStart);       // Plays ShieldBashStart SFX
        SBC.shieldBash = true;
        playerAnim.SetBool("IsShieldBashP", true);
        yield return new WaitForSeconds(1.042f);
        playerAnim.SetBool("IsShieldBashP", false);
        PSB.staminaBar.value -= PSB.maxStamina * 0.15f;
        SBC.shieldBash = false;
    }

    private IEnumerator DoubleClickCheck()
    {
        active = false;
        while (Time.time < timeOfFirstClick + timeBetweenClicks)
        {
            if (amountOfClicks >= 2 && !shielding)
            {
                StartCoroutine(ComboSwing());       //starts the combo if the player has double clicked
                break;
            }
            yield return new WaitForEndOfFrame();
        }
        amountOfClicks = 0;
        timeOfFirstClick = 0f;
        active = true;
    }
}

