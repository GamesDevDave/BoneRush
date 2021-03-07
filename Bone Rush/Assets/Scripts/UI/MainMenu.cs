using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }


    public void PlayGame()
    {
        // SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        // Loads the next scene in the build order.
        // Ensure that the game scene is immediately after the MainMenu scene in the build order.

        // Sets timescale to 1 to avoid 'Pause -> Main Menu -> Play' bug
        // Time.timeScale = 1f;
        // GameManager.stamina = 100;
        // GameManager.health = 100;
        // Destroy(GameObject.FindGameObjectWithTag("GameManager"));

        ResetGame();
        SceneManager.LoadScene(10);
    }

    // This sets timescale to 1 and resets player stats
    public void ResetGame()
    {
        // Sets timescale to 1 to avoid 'Pause -> Main Menu -> Play' bug
        Time.timeScale = 1f;

        // Resets player stats
        GameManager.stamina = 100;
        GameManager.health = 100;
        Destroy(GameObject.FindGameObjectWithTag("GameManager"));
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene("SCN_Menu_Title");
    }

    public void QuitGame()
    {
        Debug.Log("Game was quit.");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
