using masonbell;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuyStockFrameController : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    public StockInfo stockInfo;
    public TMP_Text nameText;
    public TMP_Text priceText;
    public TMP_Text amountInBoxText;
    public TMP_Text boxPriceText;
    public TMP_Text buttonText;

    public StockBoxController boxToSpawn;

    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    private float _boxCost;
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        UpdateFrameInfo();
    }
    #endregion

    #region Public Methods
    public void UpdateFrameInfo()
    {
        stockInfo = StockInfoController.Instance.GetStockInfo(stockInfo.name);
        nameText.text = stockInfo.name;
        priceText.text = $"${stockInfo.currentPrice:0.00}";

        int boxAmount = boxToSpawn.GetStockAmount(stockInfo.typeOfStock);
        amountInBoxText.text = $"{boxAmount} per box";

        _boxCost = boxAmount * stockInfo.currentPrice;
        boxPriceText.text = $"Box: ${_boxCost:0.00}";

        buttonText.text = $"PAY: ${_boxCost:0.00}";
    }

    public void BuyBox()
    {
        if (StoreController.Instance.CheckMoneyAvailable(_boxCost))
        {
            StoreController.Instance.SpendMoney(_boxCost);

            ObjectPool<StockBoxController>.GetFromPool(boxToSpawn, StoreController.Instance.stockSpawnPoint.position, Quaternion.identity).SetupBox(stockInfo);
            // Instantiate(boxToSpawn, StoreController.Instance.stockSpawnPoint.position, Quaternion.identity).SetupBox(stockInfo);
        }
    }
    #endregion

    #region Private Methods
    #endregion
}