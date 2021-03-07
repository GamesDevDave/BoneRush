using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_UI_BuildButtons : MonoBehaviour
{
    [SerializeField] private SCR_PlayerLevelUp Build;
    

    public void SpeedButton()
    {
        Build.CharacterBuild(1);
        SceneManager.LoadScene(1);
    }

    public void DamageButton()
    {
        Build.CharacterBuild(2);
        SceneManager.LoadScene(1);
    }

    public void DefenceButton()
    {
        Build.CharacterBuild(3);
        SceneManager.LoadScene(1);
    }
}
