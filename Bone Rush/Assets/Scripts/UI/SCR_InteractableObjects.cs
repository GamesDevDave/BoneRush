using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SCR_InteractableObjects : MonoBehaviour
{
    //[Header("Put any pop-up text inside here")]
    static string[] descriptions;

    public static bool amDisplaying { get; set; }

    static string fullText;
    string currentText;

    static int descIndex;

    static bool finished;
    public float delay = .1f;

    public Canvas objectCanvas;
    public TextMeshProUGUI canvasText;

    // Update is called once per frame
    void Update()
    {
        if(amDisplaying && Input.GetKeyDown(KeyCode.E))
        {
            if(finished)
            {
                descIndex++;
                if (descIndex <= descriptions.Length - 1)
                {
                    fullText = descriptions[descIndex];
                    StartCoroutine(DisplayText());
                }
                else
                {
                    StopAllCoroutines();
                    SCR_PlayerObjectInteraction.pauseTimer = .01f;
                    amDisplaying = false;
                }
            }
            else
            {
                StopAllCoroutines();
                currentText = fullText;
                canvasText.text = currentText;
                finished = true;
            }
        }

        // Switch canvas depending on interacting or not
        if (!amDisplaying && objectCanvas.gameObject.activeSelf)
        {
            objectCanvas.gameObject.SetActive(false);
        }
        else if(amDisplaying && !objectCanvas.gameObject.activeSelf)
        {
            objectCanvas.gameObject.SetActive(true);
            StartCoroutine(DisplayText());
        }
    }

    public static void InteractionStart(string[] text)
    {
        amDisplaying = true;
        descriptions = text;
        descIndex = 0;
        finished = false;
        fullText = descriptions[descIndex];
    }

    IEnumerator DisplayText()
    {
        finished = false;
        for(int i=0; i < fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i);
            canvasText.text = currentText;
            yield return new WaitForSeconds(delay);
        }
        finished = true;
    }
}
