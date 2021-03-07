/// <summary>
/// This script is attached to the LevelSelect Menu and is used to load each individual scene.
/// There is a separate function for loading each scene, these of which can be called in the inspector from any button's OnClick() function.
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_UI_LevelSelect : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // The following functions are used to load specific levels:

    // Loads CBR1:
    public void LoadCBR1()
    {
        SceneManager.LoadScene("SCN_CBR_1");
    }

    // Loads CBR2:
    public void LoadCBR2()
    {
        SceneManager.LoadScene("SCN_CBR_2");
    }

    // Loads CBR3:
    public void LoadCBR3()
    {
        SceneManager.LoadScene("SCN_CBR_3");
    }

    // Loads CBR4:
    public void LoadCBR4()
    {
        SceneManager.LoadScene("SCN_CBR_4");
    }

    // Loads CBR5:
    public void LoadCBR5()
    {
        SceneManager.LoadScene("SCN_CBR_5");
    }

    // Loads CBR6:
    public void LoadCBR6()
    {
        SceneManager.LoadScene("SCN_CBR_6");
    }

    // Loads Boss Scene:
    public void LoadBossSCN()
    {
        SceneManager.LoadScene("BOSS_BLOCKOUT");
    }
}
