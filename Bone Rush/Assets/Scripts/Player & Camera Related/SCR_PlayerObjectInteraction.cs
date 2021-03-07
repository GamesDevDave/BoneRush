using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SCR_PlayerObjectInteraction : MonoBehaviour
{

    SCR_ObjectData[] objects;
    SCR_ObjectData closestObj;

    float closestDistance = 9999;

    public static float pauseTimer { get; set; }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Update is called once per frame
    void Update()
    {
        foreach(SCR_ObjectData obj in objects)
        {
            //Debug.Log("Running and found " + obj.name);
            if (obj.amInteractable)
            {
                float distance = Vector3.Distance(transform.position, obj.transform.position);
                if (distance < closestDistance && closestObj != obj)
                {
                    Debug.Log("NEW CLOSEST OBJECT");
                    closestDistance = distance;
                    if (closestObj != null)
                    {
                        closestObj.GetComponentInChildren<ParticleSystem>().Stop();
                    }
                    closestObj = obj;
                }
            }
            //Debug.Log("Closest Obj: " + closestObj.name + ", distance = "+closestDistance);
        }
        if(closestDistance < 6)
        {
            //Debug.Log("READY");
            if(closestObj.GetComponentInChildren<ParticleSystem>().isStopped) closestObj.GetComponentInChildren<ParticleSystem>().Play();
            if (pauseTimer <= 0)
            {
                if (Input.GetKeyDown(KeyCode.E) && !SCR_InteractableObjects.amDisplaying)
                {
                    SCR_InteractableObjects.InteractionStart(closestObj.descriptions);
                    if (closestObj.canGiveXP)
                    {
                        closestObj.canGiveXP = false;
                        GameObject.FindGameObjectWithTag("GameManager").GetComponent<PlayerStats>().GetXp(1);
                    }
                }
            }
        }
        else
        {
            if (closestObj != null)
            {
                closestObj.GetComponentInChildren<ParticleSystem>().Stop();
            }
            SCR_InteractableObjects.amDisplaying = false;
        }

        if (closestObj != null)
        {
            // Update distance to closest interactable object
            closestDistance = Vector3.Distance(transform.position, closestObj.transform.position);
        }


        if(pauseTimer > 0)
        {
            pauseTimer -= Time.deltaTime;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        objects = FindObjectsOfType<SCR_ObjectData>();
    }
}
