using UnityEngine;

public class BuyMenuController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Panel References")]
    [SerializeField] private GameObject _stockPanel;
    [SerializeField] private GameObject _furniturePanel;
    #endregion

    #region Public Methods
    public void OpenStockPanel()
    {
        SetPanelStates(true, false);
    }

    public void OpenFurniturePanel()
    {
        SetPanelStates(false, true);
    }
    #endregion

    #region Private Methods
    private void SetPanelStates(bool stockActive, bool furnitureActive)
    {
        if (_stockPanel != null)
        {
            _stockPanel.SetActive(stockActive);
        }

        if (_furniturePanel != null)
        {
            _furniturePanel.SetActive(furnitureActive);
        }
    }
    #endregion
}

//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class BuyMenuController : MonoBehaviour
//{
//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public GameObject stockPanel;
//    public GameObject furniturePanel;
//    #endregion

//    #region Serialized Private Fields
//    #endregion

//    #region Private Fields
//    #endregion

//    #region Public Properties
//    #endregion

//    #region Unity Callbacks
//    #endregion

//    #region Public Methods
//    public void OpenStockPanel()
//    {
//        stockPanel.SetActive(true);
//        furniturePanel.SetActive(false);
//    }

//    public void OpenFurniturePanel()
//    {
//        furniturePanel.SetActive(true);
//        stockPanel.SetActive(false);
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}