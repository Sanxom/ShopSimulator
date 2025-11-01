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
    [SerializeField] private StockBoxController boxPrefab;
    [SerializeField] private FurnitureController furniturePrefab;
    [SerializeField] private int initialPooledObjectSize = 50; // TODO: Update this accordingly
    #endregion

    #region Private Fields
    private const string POOLED_BOX_GAMEOBJECT_NAME = "PooledBoxes";
    private const string POOLED_FURNITURE_GAMEOBJECT_NAME = "PooledFurniture";

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

    private void Start()
    {
        // TODO: Maybe place this in a new Script for Initializing ObjectPools instead of doing it here
        ObjectPool<StockBoxController>.Initialize(boxPrefab, initialPooledObjectSize, 0, transform.Find(POOLED_BOX_GAMEOBJECT_NAME));
        ObjectPool<FurnitureController>.Initialize(furniturePrefab, initialPooledObjectSize, 0, transform.Find(POOLED_FURNITURE_GAMEOBJECT_NAME));
        for (int i = 0; i < allStock.Count; i++)
        {
            if (allStock[i].currentPrice == 0)
            {
                allStock[i].currentPrice = allStock[i].basePrice;
            }

            ObjectPool<StockObject>.Initialize(allStock[i].stockObject, initialPooledObjectSize, 0, transform.GetChild(i));
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