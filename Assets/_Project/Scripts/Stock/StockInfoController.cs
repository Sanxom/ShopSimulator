using System;
using System.Collections.Generic;
using UnityEngine;

public class StockInfoController : MonoBehaviour
{
    public static StockInfoController Instance { get; private set; }

    #region Event Fields
    #endregion

    #region Public Fields
    public List<StockInfo> drinkInfo;
    public List<StockInfo> foodInfo;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    private List<StockInfo> allStock = new();
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
            Instance = this;

        allStock.AddRange(drinkInfo);
        allStock.AddRange(foodInfo);
    }
    #endregion

    #region Public Methods
    public StockInfo GetStockInfo(string stockName)
    {
        StockInfo infoToReturn = null;

        for (int i = 0; i < allStock.Count; i++)
        {
            if (allStock[i].name == stockName)
            {
                infoToReturn = allStock[i];
            }
        }

        return infoToReturn;
    }
    #endregion

    #region Private Methods
    #endregion
}