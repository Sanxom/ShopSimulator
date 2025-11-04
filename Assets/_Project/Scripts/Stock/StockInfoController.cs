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
    [SerializeField] private List<StockBoxController> allBoxPrefabs;
    [SerializeField] private List<FurnitureController> allFurniturePrefabs;
    [SerializeField] private int initialPooledObjectSize = 50; // TODO: Update this accordingly
    #endregion

    #region Private Fields
    private const string POOLED_STOCK_GAMEOBJECT_NAME = "PooledStock";
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
        // TODO: Maybe place this in a new Script for Initializing ObjectPools instead of doing it here and find a better way to do this. We're
        //       using transform.Find to look for a specific GameObject which is pretty unsafe if we delete that object or move them around
        for (int i = 0; i < allStock.Count; i++)
        {
            if (allStock[i].currentPrice == 0)
            {
                allStock[i].currentPrice = allStock[i].basePrice;
            }

            ObjectPool<StockObject>.Initialize(allStock[i].stockObject, initialPooledObjectSize, 0, transform.Find(POOLED_STOCK_GAMEOBJECT_NAME).GetChild(i));
        }

        // Initialize ALL furniture objects, and set their parents to specific objects in the hierarchy
        for (int i = 0; i < allFurniturePrefabs.Count; i++)
        {
            ObjectPool<FurnitureController>.Initialize(allFurniturePrefabs[i], initialPooledObjectSize, 0, transform.Find(POOLED_FURNITURE_GAMEOBJECT_NAME).GetChild(i));
        }

        for(int i = 0; i < allBoxPrefabs.Count; i++)
        {
            ObjectPool<StockBoxController>.Initialize(allBoxPrefabs[i], initialPooledObjectSize, 0, transform.Find(POOLED_BOX_GAMEOBJECT_NAME).GetChild(i));
        }
    }

    // TODO: Not sure if this is necessary. Should look at profiler to see if this prevents some Garbage from hanging around.
    private void OnDisable()
    {
        ObjectPool<StockObject>.ReturnAllToPool();
        ObjectPool<StockBoxController>.ReturnAllToPool();
        ObjectPool<FurnitureController>.ReturnAllToPool();
        ObjectPool<FurnitureController>.ClearAllPools();
        ObjectPool<StockBoxController>.ClearAllPools();
        ObjectPool<StockObject>.ClearAllPools();
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