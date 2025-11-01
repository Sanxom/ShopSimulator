using masonbell;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuyFurnitureFrameController : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    public FurnitureController furniture;
    public TMP_Text priceText;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        priceText.text = $"Price: ${furniture.price:0.00}";
    }
    #endregion

    #region Public Methods
    public void BuyFurniture()
    {
        if (StoreController.Instance.CheckMoneyAvailable(furniture.price))
        {
            StoreController.Instance.SpendMoney(furniture.price);

            ObjectPool<FurnitureController>.GetFromPool(
                furniture, Vector3.zero, Quaternion.identity, StoreController.Instance.furnitureSpawnPoint);
        }
    }
    #endregion

    #region Private Methods
    #endregion
}