using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using FMODUnity;
using FMOD.Studio;

public class Dashing : MonoBehaviour
{
    [Header("Values")]
    public float dashSpeed;
    public bool dashing;

    [Header("References")]
    [SerializeField]
    Rigidbody rigidBody;
    RigidbodyFirstPersonController rbfp;
    PlayerStaminaBar stamScript;
    Vector3 savedVelocity;
    bool canDash = true;

    // FMOD:
    [EventRef] [SerializeField] string eventDash;       // Played when player dashes

    private void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        rbfp = GetComponent<RigidbodyFirstPersonController>();
        stamScript = GetComponent<PlayerStaminaBar>();
    }

    void Update()
    {
        Dash();
    }

    void Dash()
    {
        if (Input.GetKeyDown(KeyCode.F) && rbfp.Grounded && stamScript.staminaBar.value > stamScript.dashStamina && canDash)
        {
            // FMOD: Plays PlayerDash sound
            RuntimeManager.PlayOneShot(eventDash);

            savedVelocity = rigidBody.velocity;

            if (Input.GetKey(KeyCode.S))
            {
                rigidBody.velocity = -(transform.forward) * dashSpeed;
            }

            else if (Input.GetKey(KeyCode.D))
            {
                rigidBody.velocity = (transform.right) * dashSpeed;
            }

            else if (Input.GetKey(KeyCode.A))
            {
                rigidBody.velocity = -(transform.right) * dashSpeed;
            }

            else
            {
                rigidBody.velocity = transform.forward * dashSpeed;
            }

            StartCoroutine(DashingDuration());

            dashing = true;

            canDash = false;

        }
    }

    IEnumerator DashingDuration()
    {
        PlayerStats.invulnerable = true;
        yield return new WaitForSeconds(.25f);
        PlayerStats.invulnerable = false;
        yield return new WaitForSeconds(.35f);
        if(rigidBody.velocity.magnitude > savedVelocity.magnitude)
        {
            rigidBody.velocity = savedVelocity;
        }
        canDash = true;
    }
}
