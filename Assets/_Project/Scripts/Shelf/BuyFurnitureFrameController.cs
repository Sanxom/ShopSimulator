using masonbell;
using TMPro;
using UnityEngine;

public class BuyFurnitureFrameController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Furniture Settings")]
    [SerializeField] private FurnitureController _furniture;

    [Header("UI References")]
    [SerializeField] private TMP_Text _priceText;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        UpdatePriceDisplay();
    }
    #endregion

    #region Public Methods
    public void BuyFurniture()
    {
        if (!CanAffordFurniture())
        {
            return;
        }

        PurchaseFurniture();
        SpawnFurniture();
    }
    #endregion

    #region Private Methods
    private void UpdatePriceDisplay()
    {
        if (_priceText != null && _furniture != null)
        {
            _priceText.text = $"Price: ${_furniture.Price:0.00}";
        }
    }

    private bool CanAffordFurniture()
    {
        return _furniture != null
            && StoreController.Instance != null
            && StoreController.Instance.CheckMoneyAvailable(_furniture.Price);
    }

    private void PurchaseFurniture()
    {
        if (StoreController.Instance != null && _furniture != null)
        {
            StoreController.Instance.SpendMoney(_furniture.Price);
        }
    }

    private void SpawnFurniture()
    {
        if (_furniture == null || StoreController.Instance == null)
        {
            return;
        }

        ObjectPool<FurnitureController>.GetFromPool(
            _furniture,
            Vector3.zero,
            Quaternion.identity,
            StoreController.Instance.FurnitureSpawnPoint
        );
    }
    #endregion
}

//using masonbell;
//using System;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class BuyFurnitureFrameController : MonoBehaviour
//{
//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public FurnitureController furniture;
//    public TMP_Text priceText;
//    #endregion

//    #region Serialized Private Fields
//    #endregion

//    #region Private Fields
//    #endregion

//    #region Public Properties
//    #endregion

//    #region Unity Callbacks
//    private void Awake()
//    {
//        priceText.text = $"Price: ${furniture.Price:0.00}";
//    }
//    #endregion

//    #region Public Methods
//    public void BuyFurniture()
//    {
//        if (StoreController.Instance.CheckMoneyAvailable(furniture.Price))
//        {
//            StoreController.Instance.SpendMoney(furniture.Price);

//            ObjectPool<FurnitureController>.GetFromPool(
//                furniture, Vector3.zero, Quaternion.identity, StoreController.Instance.FurnitureSpawnPoint);
//        }
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}