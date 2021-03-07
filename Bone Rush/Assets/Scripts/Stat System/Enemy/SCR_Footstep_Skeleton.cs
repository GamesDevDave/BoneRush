using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class SCR_Footstep_Skeleton : MonoBehaviour
{
    // FMOD Events:
    [EventRef] [SerializeField] string eventFootstep;       // Played during skeleton walk animation

    // Called during skeleton's animation event
    public void Footstep()
    {
        RuntimeManager.PlayOneShot(eventFootstep, transform.position);          // Plays Footstep sound
    }
}
