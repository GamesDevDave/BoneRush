using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Object_Data", menuName = "ScriptableObjects/Objects/Object_Data", order = 0)]
public class SCR_ScriptableObjectData : ScriptableObject
{
    [Header("Can the object be interacted with by default?")]
    public bool amInteractableByDefault;

    [Header("Each new 'page' of description goes as a seperate array item")]
    [TextArea(3, 10)]
    public string[] descriptions;
}
