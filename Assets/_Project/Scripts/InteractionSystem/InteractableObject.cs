using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
    private Outline _outline;
    #endregion

    #region Public Properties
    public GameObject MyObject { get; private set; }
    #endregion

    #region Unity Callbacks
    protected virtual void Awake()
    {
        MyObject = gameObject;
        if (MyObject.TryGetComponent(out Outline outline))
        {
            _outline = outline;
            _outline.OutlineColor = Color.green;
            _outline.OutlineWidth = 20f;
            _outline.OutlineMode = Outline.Mode.OutlineVisible;
            _outline.enabled = false;
        }
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

    public virtual void OnFocusGained()
    {
        if (_outline == null) return;
        _outline.enabled = true;
    }

    public virtual void OnFocusLost()
    {
        if (_outline == null) return;
        _outline.enabled = false;
    }
}