using UnityEngine;

public interface IInteractable
{
    public GameObject MyObject { get; }
    public string DisplayName { get; }

    public string GetInteractionPrompt();
    public bool CanInteract();
    public void OnInteract(PlayerInteraction player);
    public void OnTake(PlayerInteraction player);
    public void OnFocusGained();
    public void OnFocusLost();
}