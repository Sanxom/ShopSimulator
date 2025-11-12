using System.Collections.Generic;
using UnityEngine;

public class StockInfoController : MonoBehaviour
{
    public static StockInfoController Instance { get; private set; }

    #region Serialized Fields
    [Header("Stock Information")]
    [SerializeField] private List<StockInfo> _drinkInfo;
    [SerializeField] private List<StockInfo> _foodInfo;
    [SerializeField] private float _stockPickupAndPlaceWaitTimeDuration;

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
    private bool _isInitialized;

    private List<StockInfo> _allStock;
    #endregion

    #region Properties
    public List<StockInfo> DrinkInfo => _drinkInfo;
    public List<StockInfo> FoodInfo => _foodInfo;
    public WaitForSeconds StockPickupAndPlaceWaitTime { get; private set; }
    [field: SerializeField] public float StockPickupAndPlaceWaitTimeDuration { get; private set; }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
        StockPickupAndPlaceWaitTime = new(_stockPickupAndPlaceWaitTimeDuration);
    }

    private void Start()
    {
        if (_isInitialized) return;

        CombineStockLists();
        InitializeAllPools();

        _isInitialized = true;
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
        DontDestroyOnLoad(gameObject);
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
    #endregion

    #region Public Methods
    public StockInfo GetStockInfo(string stockName)
    {
        foreach (StockInfo stock in _allStock)
        {
            if (stock.Name == stockName)
                return stock;
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
            if (stock.Name == stockName)
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
            if (shelf.StockInfo != null && shelf.StockInfo.Name == stockName)
                shelf.SetShelfLabelText(newPrice);
        }
    }
    #endregion
}