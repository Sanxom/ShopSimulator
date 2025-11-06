using System;
using System.Collections.Generic;
using UnityEngine;

public class PriceLabel : MonoBehaviour, IInteractable
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private ShelfSpaceController myShelf;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    public GameObject MyObject { get; set; }
    #endregion

    #region Unity Callbacks
    #endregion

    #region Public Methods
    public void OnInteract(Transform holdPoint = null)
    {
        myShelf.StartPriceUpdate();
    }

    public string GetInteractionPrompt()
    {
        return $"Set Price of {myShelf.StockInfo.Name}";
    }
    #endregion

    #region Private Methods
    #endregion
}