using System;
using System.Collections.Generic;
using UnityEngine;

public class TrashCan : MonoBehaviour, IInteractable
{
    public GameObject MyObject { get; set; }

    private void Awake()
    {
        MyObject = gameObject;
    }

    public string GetInteractionPrompt()
    {
        return "Trash Can";
    }

    public void OnInteract(Transform holdPoint = null)
    {
        //noop, just here as marker, really
    }
}