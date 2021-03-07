using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Characters.FirstPerson;
using System.Collections;

public class PlayerStaminaBar : MonoBehaviour
{

    [Header("References")]
    public Slider staminaBar;
    public RigidbodyFirstPersonController movementScript;
    public SwordThings swordThings;
    public Dashing dash;

    [Header("Values")]
    public float maxStamina = 100f;
    public float staminaDecrease = 10f;
    public float staminaRegen = 2f;
    public float minStamina = 0.5f;
    public float timeBeforeRegen = 1f;
    public float jumpStamina = 10f;
    public float dashStamina = 15f;
    float blockStamina = 10f;
    public bool canRegen;
    public bool canSprint = true;
    public bool canBlock;
    public bool canJump;
    public bool regenerating;

    private void Start()
    {
        GetComponent<SwordThings>();
        GetComponent<Dashing>();
		staminaBar = GameObject.Find("PlayerStaminaSlider").GetComponent<Slider>();

		staminaBar.value = GameManager.stamina;
        maxStamina = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerStats>().TransferPlayerMaxStamina();
    }

    private void Update()
    {
        CheckForStamina();
        UpdateSlider();
        StaminaRegen();
        CheckForJump();
        CheckForBlocking();
        CheckForHeavyAttack();
        DashStamina();
    }

    void CheckForStamina()
    {
        if (staminaBar.value <= minStamina && movementScript.Running)
        {
            canSprint = false;
        }
        else
        {
            canSprint = true;
        }
    }

    void UpdateSlider()
    {
        if (canSprint)
        {
            if (movementScript.Running)
            {
                staminaBar.value -= Time.deltaTime * staminaDecrease;
            }
        }
        else
        {
            StaminaRegen();
        }
    }

    void StaminaRegen()
    {
        if (staminaBar.value < maxStamina)
        {
            if (movementScript.Running == false)
            {
                StartCoroutine(WaitForRegen());
            }

            if (canRegen)
            {
                staminaBar.value += Time.deltaTime * staminaRegen;
                regenerating = true;
            }
        }
    }

    void CheckForJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && movementScript.Grounded && staminaBar.value > jumpStamina)
        {
            staminaBar.value -= Mathf.Clamp(jumpStamina, minStamina, maxStamina * 0.5f);
        }
    }

    void CheckForBlocking()
    {
        if (staminaBar.value <= blockStamina)
        {
            canBlock = false;
        }
        else
        {
            canBlock = true;
        }
    }

    void CheckForHeavyAttack()
    {
        if (staminaBar.value >= staminaBar.value*0.2)
        {
            //swordThings.canHeavyAttack = true;
        }
    }

    void DashStamina()
    {
        if (dash.dashing)
        {
            staminaBar.value -= Mathf.Clamp(dashStamina, minStamina, maxStamina * 0.5f);
            dash.dashing = false;
        }
    }

    IEnumerator WaitForRegen()
    {
        yield return new WaitForSeconds(timeBeforeRegen);
        canRegen = true;
    }

}
