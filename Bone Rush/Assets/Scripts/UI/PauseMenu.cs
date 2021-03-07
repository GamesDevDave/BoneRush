using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    // This allows it to be accessed by any other class.
    #region Singleton
    public static PauseMenu Instance;
    private void Awake()
    {
        if (Instance != null) Destroy(this);
        else Instance = this;
    }
    #endregion

    // SerializeField allows you to see a private variable in the inspector.
    [SerializeField]
    private GameObject pauseMenu;           // Used to enable when pausing.
    [SerializeField]
    private GameObject healthBar;           // Used to disable this when pausing.
    [SerializeField]
    private GameObject controlsMenu;        // Used to disable this when unpausing.
    [SerializeField]
    private GameObject helpMenu;            // Used to disable this when unpausing.

    private bool isPaused = false;

    // Detects when player inputs Escape key.
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !SCR_InteractableObjects.amDisplaying) PauseGame();
        else if (Input.GetKeyDown(KeyCode.Escape) && SCR_InteractableObjects.amDisplaying) SCR_InteractableObjects.amDisplaying = false;
    }

    // Sets timescale to 1 when the game starts.
    private void Start() 
    {
        Time.timeScale = 1f;
    }

    public void PauseGame()
    {
        // Inverts function.
        isPaused = !isPaused;

        // Debug.Log("isPaused value: " + isPaused);

        // Cursor is visible whenever game is paused.
        Cursor.visible = isPaused;

        // If the game is paused, the timeScale is set to 0 so it actually pauses the game.
        if (isPaused)
        {
            Time.timeScale = 0f;

            // Unlocks cursor
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
        }

        // Activates pauseMenu GO.
        pauseMenu.SetActive(isPaused);

        // Disables controlsMenu and helpMenu when pressing escape so that the menu doesn't stay enabled if escape is pressed whilst on these menus.
        controlsMenu.SetActive(false);
        helpMenu.SetActive(false);

        // Disabled healthBar when pausing.
        healthBar.SetActive(!isPaused);
    }
}
