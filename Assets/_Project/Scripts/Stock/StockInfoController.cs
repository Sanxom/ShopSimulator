using masonbell;
using System.Collections.Generic;
using UnityEngine;

public class StockInfoController : MonoBehaviour
{
    public static StockInfoController Instance { get; private set; }

    #region Serialized Fields
    [Header("Stock Information")]
    [SerializeField] private List<StockInfo> _drinkInfo;
    [SerializeField] private List<StockInfo> _foodInfo;

    [Header("Prefabs")]
    [SerializeField] private List<StockBoxController> _allBoxPrefabs;
    [SerializeField] private List<FurnitureController> _allFurniturePrefabs;

    [Header("Pooling Settings")]
    [SerializeField] private int _initialPooledObjectSize = 50;
    #endregion

    #region Private Fields
    private const string POOLED_STOCK_GAMEOBJECT_NAME = "PooledStock";
    private const string POOLED_BOX_GAMEOBJECT_NAME = "PooledBoxes";
    private const string POOLED_FURNITURE_GAMEOBJECT_NAME = "PooledFurniture";

    private List<StockInfo> _allStock;
    #endregion

    #region Properties
    public List<StockInfo> DrinkInfo => _drinkInfo;
    public List<StockInfo> FoodInfo => _foodInfo;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
        CombineStockLists();
    }

    private void Start()
    {
        InitializeAllPools();
    }

    private void OnDisable()
    {
        CleanupAllPools();
    }
    #endregion

    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void CombineStockLists()
    {
        _allStock = new List<StockInfo>();
        _allStock.AddRange(_drinkInfo);
        _allStock.AddRange(_foodInfo);
    }

    private void InitializeAllPools()
    {
        InitializeStockPools();
        InitializeFurniturePools();
        InitializeBoxPools();
    }

    private void InitializeStockPools()
    {
        Transform pooledStockParent = transform.Find(POOLED_STOCK_GAMEOBJECT_NAME);

        if (pooledStockParent == null)
        {
            Debug.LogWarning($"Could not find {POOLED_STOCK_GAMEOBJECT_NAME} GameObject");
            return;
        }

        for (int i = 0; i < _allStock.Count; i++)
        {
            StockInfo stock = _allStock[i];

            if (stock.currentPrice == 0f)
            {
                stock.currentPrice = stock.basePrice;
            }

            if (stock.stockObject != null && i < pooledStockParent.childCount)
            {
                ObjectPool<StockObject>.Initialize(
                    stock.stockObject,
                    _initialPooledObjectSize,
                    0,
                    pooledStockParent.GetChild(i)
                );
            }
        }
    }

    private void InitializeFurniturePools()
    {
        Transform pooledFurnitureParent = transform.Find(POOLED_FURNITURE_GAMEOBJECT_NAME);

        if (pooledFurnitureParent == null)
        {
            Debug.LogWarning($"Could not find {POOLED_FURNITURE_GAMEOBJECT_NAME} GameObject");
            return;
        }

        for (int i = 0; i < _allFurniturePrefabs.Count; i++)
        {
            if (_allFurniturePrefabs[i] != null && i < pooledFurnitureParent.childCount)
            {
                ObjectPool<FurnitureController>.Initialize(
                    _allFurniturePrefabs[i],
                    _initialPooledObjectSize,
                    0,
                    pooledFurnitureParent.GetChild(i)
                );
            }
        }
    }

    private void InitializeBoxPools()
    {
        Transform pooledBoxParent = transform.Find(POOLED_BOX_GAMEOBJECT_NAME);

        if (pooledBoxParent == null)
        {
            Debug.LogWarning($"Could not find {POOLED_BOX_GAMEOBJECT_NAME} GameObject");
            return;
        }

        for (int i = 0; i < _allBoxPrefabs.Count; i++)
        {
            if (_allBoxPrefabs[i] != null && i < pooledBoxParent.childCount)
            {
                ObjectPool<StockBoxController>.Initialize(
                    _allBoxPrefabs[i],
                    _initialPooledObjectSize,
                    0,
                    pooledBoxParent.GetChild(i)
                );
            }
        }
    }

    private void CleanupAllPools()
    {
        ObjectPool<StockObject>.ReturnAllToPool();
        ObjectPool<StockBoxController>.ReturnAllToPool();
        ObjectPool<FurnitureController>.ReturnAllToPool();

        ObjectPool<StockObject>.ClearAllPools();
        ObjectPool<StockBoxController>.ClearAllPools();
        ObjectPool<FurnitureController>.ClearAllPools();
    }
    #endregion

    #region Public Methods
    public StockInfo GetStockInfo(string stockName)
    {
        foreach (StockInfo stock in _allStock)
        {
            if (stock.name == stockName)
            {
                return stock;
            }
        }

        return null;
    }

    public void UpdatePrice(string stockName, float newPrice)
    {
        UpdateStockPrice(stockName, newPrice);
        UpdateShelvesPrice(stockName, newPrice);
    }

    private void UpdateStockPrice(string stockName, float newPrice)
    {
        foreach (StockInfo stock in _allStock)
        {
            if (stock.name == stockName)
            {
                stock.currentPrice = newPrice;
                break;
            }
        }
    }

    private void UpdateShelvesPrice(string stockName, float newPrice)
    {
        ShelfSpaceController[] shelves = FindObjectsByType<ShelfSpaceController>(FindObjectsSortMode.None);

        foreach (ShelfSpaceController shelf in shelves)
        {
            if (shelf.StockInfo != null && shelf.StockInfo.name == stockName)
            {
                shelf.SetShelfLabelText(newPrice);
            }
        }
    }
    #endregion
}

//using masonbell;
//using System;
//using System.Collections.Generic;
//using System.Security.Cryptography.X509Certificates;
//using UnityEngine;

//public class StockInfoController : MonoBehaviour
//{
//    public static StockInfoController Instance { get; private set; }

//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public List<StockInfo> drinkInfo;
//    public List<StockInfo> foodInfo;
//    #endregion

//    #region Serialized Private Fields
//    [SerializeField] private StockBoxController boxPrefab;
//    [SerializeField] private List<StockBoxController> allBoxPrefabs;
//    [SerializeField] private List<FurnitureController> allFurniturePrefabs;
//    [SerializeField] private int initialPooledObjectSize = 50; // TODO: Update this accordingly
//    #endregion

//    #region Private Fields
//    private const string POOLED_STOCK_GAMEOBJECT_NAME = "PooledStock";
//    private const string POOLED_BOX_GAMEOBJECT_NAME = "PooledBoxes";
//    private const string POOLED_FURNITURE_GAMEOBJECT_NAME = "PooledFurniture";

//    private List<StockInfo> allStock = new();
//    #endregion

//    #region Public Properties
//    #endregion

//    #region Unity Callbacks
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//        }
//        else
//            Instance = this;

//        allStock.AddRange(drinkInfo);
//        allStock.AddRange(foodInfo);
//    }

//    private void Start()
//    {
//        // TODO: Maybe place this in a new Script for Initializing ObjectPools instead of doing it here and find a better way to do this. We're
//        //       using transform.Find to look for a specific GameObject which is pretty unsafe if we delete that object or move them around
//        for (int i = 0; i < allStock.Count; i++)
//        {
//            if (allStock[i].currentPrice == 0)
//            {
//                allStock[i].currentPrice = allStock[i].basePrice;
//            }

//            ObjectPool<StockObject>.Initialize(allStock[i].stockObject, initialPooledObjectSize, 0, transform.Find(POOLED_STOCK_GAMEOBJECT_NAME).GetChild(i));
//        }

//        // Initialize ALL furniture objects, and set their parents to specific objects in the hierarchy
//        for (int i = 0; i < allFurniturePrefabs.Count; i++)
//        {
//            ObjectPool<FurnitureController>.Initialize(allFurniturePrefabs[i], initialPooledObjectSize, 0, transform.Find(POOLED_FURNITURE_GAMEOBJECT_NAME).GetChild(i));
//        }

//        for(int i = 0; i < allBoxPrefabs.Count; i++)
//        {
//            ObjectPool<StockBoxController>.Initialize(allBoxPrefabs[i], initialPooledObjectSize, 0, transform.Find(POOLED_BOX_GAMEOBJECT_NAME).GetChild(i));
//        }
//    }

//    // TODO: Not sure if this is necessary. Should look at profiler to see if this prevents some Garbage from hanging around.
//    private void OnDisable()
//    {
//        ObjectPool<StockObject>.ReturnAllToPool();
//        ObjectPool<StockBoxController>.ReturnAllToPool();
//        ObjectPool<FurnitureController>.ReturnAllToPool();
//        ObjectPool<FurnitureController>.ClearAllPools();
//        ObjectPool<StockBoxController>.ClearAllPools();
//        ObjectPool<StockObject>.ClearAllPools();
//    }
//    #endregion

//    #region Public Methods
//    public StockInfo GetStockInfo(string stockName)
//    {
//        StockInfo infoToReturn = null;

//        for (int i = 0; i < allStock.Count; i++)
//        {
//            if (allStock[i].name == stockName)
//            {
//                infoToReturn = allStock[i];
//            }
//        }

//        return infoToReturn;
//    }

//    public void UpdatePrice(string stockName, float newPrice)
//    {
//        for (int i = 0; i < allStock.Count; i++)
//        {
//            if (allStock[i].name == stockName)
//            {
//                allStock[i].currentPrice = newPrice;
//            }
//        }

//        List<ShelfSpaceController> shelves = new();
//        shelves.AddRange(FindObjectsByType<ShelfSpaceController>(FindObjectsSortMode.None)); // TODO: Need to refactor this eventually

//        foreach (ShelfSpaceController shelf in shelves)
//        {
//            if(shelf.Info.name == stockName)
//            {
//                shelf.SetShelfLabelText(newPrice);
//            }
//        }
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}