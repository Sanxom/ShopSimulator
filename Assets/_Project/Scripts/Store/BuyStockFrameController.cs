using masonbell;
using TMPro;
using UnityEngine;

public class BuyStockFrameController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stock Information")]
    [SerializeField] private StockInfo _stockInfo;

    [Header("UI References")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private TMP_Text _amountInBoxText;
    [SerializeField] private TMP_Text _boxPriceText;
    [SerializeField] private TMP_Text _buttonText;

    [Header("Box Prefab")]
    [SerializeField] private StockBoxController _boxToSpawn;
    #endregion

    #region Private Fields
    private float _boxCost;
    #endregion

    #region Properties
    public StockInfo StockInfo => _stockInfo;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        UpdateFrameInfo();
    }
    #endregion

    #region Public Methods
    public void UpdateFrameInfo()
    {
        RefreshStockInfo();
        UpdateDisplayTexts();
        CalculateBoxCost();
    }

    public void BuyBox()
    {
        if (!CanAffordBox())
        {
            return;
        }

        PurchaseBox();
        SpawnBox();
    }
    #endregion

    #region Private Methods
    private void RefreshStockInfo()
    {
        if (StockInfoController.Instance != null)
        {
            _stockInfo = StockInfoController.Instance.GetStockInfo(_stockInfo.name);
        }
    }

    private void UpdateDisplayTexts()
    {
        if (_stockInfo == null)
        {
            return;
        }

        if (_nameText != null) _nameText.text = _stockInfo.name;
        if (_priceText != null) _priceText.text = $"${_stockInfo.currentPrice:0.00}";
    }

    private void CalculateBoxCost()
    {
        if (_boxToSpawn == null || _stockInfo == null)
        {
            return;
        }

        int boxAmount = _boxToSpawn.GetMaxPossibleStockAmount(_stockInfo.typeOfStock);
        _boxCost = boxAmount * _stockInfo.currentPrice;

        if (_amountInBoxText != null) _amountInBoxText.text = $"{boxAmount} per box";
        if (_boxPriceText != null) _boxPriceText.text = $"Box: ${_boxCost:0.00}";
        if (_buttonText != null) _buttonText.text = $"PAY: ${_boxCost:0.00}";
    }

    private bool CanAffordBox()
    {
        return StoreController.Instance != null && StoreController.Instance.CheckMoneyAvailable(_boxCost);
    }

    private void PurchaseBox()
    {
        if (StoreController.Instance != null)
        {
            StoreController.Instance.SpendMoney(_boxCost);
        }
    }

    private void SpawnBox()
    {
        if (_boxToSpawn == null || StoreController.Instance == null || _stockInfo == null)
        {
            return;
        }

        Vector3 spawnPosition = StoreController.Instance.StockSpawnPoint.position;
        StockBoxController spawnedBox = ObjectPool<StockBoxController>.GetFromPool(
            _boxToSpawn,
            spawnPosition,
            Quaternion.identity
        );

        spawnedBox.SetupBox(_stockInfo);
    }
    #endregion
}

//using masonbell;
//using System;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class BuyStockFrameController : MonoBehaviour
//{
//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public StockInfo stockInfo;
//    public TMP_Text nameText;
//    public TMP_Text priceText;
//    public TMP_Text amountInBoxText;
//    public TMP_Text boxPriceText;
//    public TMP_Text buttonText;

//    public StockBoxController boxToSpawn;

//    #endregion

//    #region Serialized Private Fields
//    #endregion

//    #region Private Fields
//    private float _boxCost;
//    #endregion

//    #region Public Properties
//    #endregion

//    #region Unity Callbacks
//    private void Start()
//    {
//        UpdateFrameInfo();
//    }
//    #endregion

//    #region Public Methods
//    public void UpdateFrameInfo()
//    {
//        stockInfo = StockInfoController.Instance.GetStockInfo(stockInfo.name);
//        nameText.text = stockInfo.name;
//        priceText.text = $"${stockInfo.currentPrice:0.00}";

//        int boxAmount = boxToSpawn.GetStockAmount(stockInfo.typeOfStock);
//        amountInBoxText.text = $"{boxAmount} per box";

//        _boxCost = boxAmount * stockInfo.currentPrice;
//        boxPriceText.text = $"Box: ${_boxCost:0.00}";

//        buttonText.text = $"PAY: ${_boxCost:0.00}";
//    }

//    public void BuyBox()
//    {
//        if (StoreController.Instance.CheckMoneyAvailable(_boxCost))
//        {
//            StoreController.Instance.SpendMoney(_boxCost);

//            ObjectPool<StockBoxController>.GetFromPool(boxToSpawn, StoreController.Instance.StockSpawnPoint.position, Quaternion.identity).SetupBox(stockInfo);
//            // Instantiate(boxToSpawn, StoreController.Instance.stockSpawnPoint.position, Quaternion.identity).SetupBox(stockInfo);
//        }
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}