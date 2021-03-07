using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InitValues : MonoBehaviour
{

    Slider stamina;
    Slider health;
    bool first;

    // Start is called before the first frame update
    void Start()
    {
        foreach(Slider slider in GetComponentsInChildren<Slider>())
        {
            if(slider.name == "PlayerStaminaSlider")
            {
                stamina = slider;
            }
            else if(slider.name == "PlayerHealthSlider")
            {
                health = slider;
            }
        }
        first = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (first)
        {
            health.value = GameManager.health;
            stamina.value = GameManager.stamina;
            first = false;
        }
        GameManager.stamina = Mathf.RoundToInt(stamina.value);
        GameManager.health = Mathf.RoundToInt(health.value);
    }
}
