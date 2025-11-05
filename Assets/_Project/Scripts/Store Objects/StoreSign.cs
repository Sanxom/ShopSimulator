using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StoreSign : MonoBehaviour, IInteractable
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private TMP_Text storeSignText;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    public GameObject MyObject { get; set; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        MyObject = gameObject;
    }
    #endregion

    #region Public Methods
    public string GetInteractionPrompt()
    {
        return $"Store Sign";
    }

    public void OnInteract(Transform holdPoint = null)
    {
        storeSignText.text = ""; // TODO: Bring up a window to change the Text of the Store Sign with Player Input
    }
    #endregion

    #region Private Methods
    #endregion
}