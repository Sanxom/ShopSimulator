using System;
using System.Collections.Generic;
using UnityEngine;

public class BuyMenuController : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    public GameObject stockPanel;
    public GameObject furniturePanel;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    #endregion

    #region Public Methods
    public void OpenStockPanel()
    {
        stockPanel.SetActive(true);
        furniturePanel.SetActive(false);
    }

    public void OpenFurniturePanel()
    {
        furniturePanel.SetActive(true);
        stockPanel.SetActive(false);
    }
    #endregion

    #region Private Methods
    #endregion
}