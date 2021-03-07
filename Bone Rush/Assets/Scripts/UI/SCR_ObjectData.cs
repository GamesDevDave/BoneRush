using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SCR_ObjectData : MonoBehaviour
{
    // Determines if the object can be interacted with or not
    public bool amInteractable;

    public string[] descriptions;

    public bool canGiveXP;

    public SCR_ScriptableObjectData data;

    // Start is called before the first frame update
    void Start()
    {
        if (data != null)
        {
            amInteractable = data.amInteractableByDefault;
            descriptions = data.descriptions;
            GameObject obj = Instantiate(GameObject.FindGameObjectWithTag("GameManager").GetComponent<GameManager>().objectOutlinePRF, transform);
            ParticleSystem ps = obj.GetComponent<ParticleSystem>();
            var sh = ps.shape;
            sh.mesh = GetComponent<MeshFilter>().mesh;
            sh.scale = transform.localScale;
        }
    }
}
