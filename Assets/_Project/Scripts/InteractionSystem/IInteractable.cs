using UnityEngine;

public interface IInteractable
{
    public GameObject MyObject { get; set; }
    public void OnInteract(Transform holdPoint = null);

    public string GetInteractionPrompt();
}