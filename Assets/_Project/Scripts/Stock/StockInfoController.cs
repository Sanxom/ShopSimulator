using masonbell;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
    private int _initialPooledObjectSize = 50; // TODO: Update this accordingly
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

    private void Start()
    {
        for (int i = 0; i < allStock.Count; i++)
        {
            if (allStock[i].currentPrice == 0)
            {
                allStock[i].currentPrice = allStock[i].basePrice;
            }

            ObjectPool<StockObject>.Initialize(allStock[i].stockObject, _initialPooledObjectSize, 0, transform.GetChild(i));
        }

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

    public void UpdatePrice(string stockName, float newPrice)
    {
        for (int i = 0; i < allStock.Count; i++)
        {
            if (allStock[i].name == stockName)
            {
                allStock[i].currentPrice = newPrice;
            }
        }

        List<ShelfSpaceController> shelves = new();
        shelves.AddRange(FindObjectsByType<ShelfSpaceController>(FindObjectsSortMode.None)); // TODO: Need to refactor this eventually

        foreach (ShelfSpaceController shelf in shelves)
        {
            if(shelf.Info.name == stockName)
            {
                shelf.SetShelfLabelText(newPrice);
            }
        }
    }
    #endregion

    #region Private Methods
    #endregion
}