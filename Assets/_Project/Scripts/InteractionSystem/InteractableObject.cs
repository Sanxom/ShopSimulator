using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    #region Event Fields

    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private string _displayName = "Interact";
    [SerializeField] private bool _isEnabled = true;
    #endregion

    #region Private Fields
    private Outline outline;
    #endregion

    #region Public Properties
    public GameObject MyObject { get; private set; }
    #endregion

    #region Unity Callbacks
    protected virtual void Awake()
    {
        MyObject = gameObject;

        //outline = MyObject.AddComponent<Outline>();
        //outline.OutlineMode = Outline.Mode.OutlineVisible;
        //outline.OutlineColor = Color.yellow;
        //outline.OutlineWidth = 1f;
        //outline.enabled = false;
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    #endregion

    public string DisplayName => _displayName;

    public bool CanInteract() => _isEnabled;

    public virtual string GetInteractionPrompt()
    {
        return DisplayName;
    }

    public virtual void OnInteract(PlayerInteraction player)
    {
    }

    public virtual void OnTake(PlayerInteraction player)
    {

    }

    public void OnFocusGained() => outline.enabled = true;

    public void OnFocusLost() => outline.enabled = false;
}