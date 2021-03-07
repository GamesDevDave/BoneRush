using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using UnityEngine.SceneManagement;

public class SCR_Boss_Transition : MonoBehaviour
{
    // [ParamRef] [SerializeField] string paramEnraged;

    [EventRef] [SerializeField] string eventBossMSC;                   // Played when boss comes out of ritual state after ritual enemies are killed
    EventInstance eventInst;                                           // Event Instance

    public void StartMusic()
    {
        // Sets the Enraged parameter to 0, preventing the transition from happening instantly if param was already set to 1.
        eventInst.setParameterByName("Enraged", 0);

        eventInst = RuntimeManager.CreateInstance(eventBossMSC);       // Creates instance for BossMSC Event
        eventInst.start();                                             // Starts instance
    }

    public void TriggerTransition()
    {
        // Debug.Log("Transitioning to Phase 2");

        // This will set the Enraged parameter to 1, triggering the transition.
        eventInst.setParameterByName("Enraged", 1);
    }

    public void StopInstance()
    {
        eventInst.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }
}
