using UnityEngine;
using FMODUnity;

public class SCR_SFX_GroundPound : MonoBehaviour
{
    [EventRef] [SerializeField] string eventGroundPound;    // Played when boss starts BossGroundPound Animation

    // Called on start of BossGroundPound animation
    public void GroundPoundSFX()
    {
        // FMOD: Plays GroundPound SFX
        RuntimeManager.PlayOneShot(eventGroundPound, transform.position);
    }
}
