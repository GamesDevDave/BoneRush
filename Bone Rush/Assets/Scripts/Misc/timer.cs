using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class timer : MonoBehaviour
{
    [SerializeField] private float maxTime = 300;  //5 mins in seconds
    private float delayTime;
    private bool active = false;
    private int percentage; 
    private Scene scene;
    private bool enteredBossRoom = false;
    private bool displayRecharge = false;
    private float currentTime;
    [SerializeField] private Slider slider;
    [SerializeField] private GameObject sliderGO;           // Needed to disable GO when entering Boss SCN
    [SerializeField] private GameObject timeDisplay;        // Needed to update the text display of time left

    private void Start()
    {
        currentTime = 0;        //resets current time back to 0, when the player restart the game
        delayTime = (0.01f * maxTime) - 0.5f;   //finds time it takes to get to 1%, plus a small delay, used for delay
    }

    private void Update()
    {
        scene = SceneManager.GetActiveScene();      //gets the current scene
        if (scene.buildIndex == 7 && !enteredBossRoom)       //check to see if the player is in the boss room
        {
            sliderGO.SetActive(false);      //deactives the timer
            enteredBossRoom = true;     //makes it so the slider is only disabled once
        }
        else if (scene.buildIndex > 0 && scene.buildIndex < 7)      //checks if the player is in the combat rooms
        {
            currentTime += Time.deltaTime;      //lowers the time
            if (!displayRecharge)       //checks if display time rechange has ended
            {
                StartCoroutine(DisplayTime(delayTime));     //starts the display time cooldown
            }
            if (currentTime > maxTime)      //checks if the currnet time has gone over max time
            {
                GameOver();     //kills the player
            }
        }
    }

    private void GameOver()
    {
        if (!active) 
        {
            SceneManager.LoadScene(9);      //loads the game over screen
        }
        active = true;      //makes sure the screen is not loaded again
    }

    private IEnumerator DisplayTime(float time)
    {
        displayRecharge = true;
        percentage = Mathf.RoundToInt((currentTime / maxTime) * 100);       //calculates the current percentage
        slider.value = percentage;      //Updates slider value
        timeDisplay.GetComponent<TextMeshProUGUI>().text = "Ritual Completion: " + percentage + "%";        // Updates text display
        yield return new WaitForSeconds(time);      //get the delay that should last about 1%
        displayRecharge = false;
    }
}
