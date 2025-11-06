#region Claude Code v2
using System.Collections.Generic;
using UnityEngine;

public class StockBoxController : MonoBehaviour, IInteractable, ITrashable
{
    #region Serialized Fields
    [Header("Stock Settings")]
    [SerializeField] private StockInfo _stockInfo;
    [SerializeField] private List<StockObject> _stockInBox;

    [Header("Point Lists")]
    [SerializeField] private List<Transform> _bigDrinkPoints;
    [SerializeField] private List<Transform> _cerealPoints;
    [SerializeField] private List<Transform> _tubeChipPoints;
    [SerializeField] private List<Transform> _fruitPoints;
    [SerializeField] private List<Transform> _largeFruitPoints;
    [SerializeField] private List<Transform> _vegetablePoints;

    [Header("Debug")]
    [SerializeField] private bool _testFill;
    #endregion

    #region Private Fields
    private const float MOVE_SPEED = 10f;
    private const string OPEN_BOX_ANIMATOR_PARAMETER = "openBox";

    private bool _isHeld;
    private Rigidbody _rigidbody;
    private Collider _collider;
    private Animator _animator;
    private bool _isOpen;
    private int _maxCapacity;
    #endregion

    #region Properties
    public GameObject MyObject { get; set; }
    public Rigidbody Rb => _rigidbody;
    public Collider Col => _collider;
    public bool OpenBox => _isOpen;
    public StockInfo StockInfo => _stockInfo;
    public List<StockObject> StockInBox => _stockInBox;
    public int MaxCapacity => _maxCapacity;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        CacheComponents();
    }

    private void Update()
    {
        if (_testFill)
        {
            _testFill = false;
            SetupBox(_stockInfo);
        }

        if (_isHeld)
        {
            MoveToHoldPosition(Time.deltaTime);
        }
    }

    private void OnDisable()
    {
        _maxCapacity = 0;
    }
    #endregion

    #region Initialization
    private void CacheComponents()
    {
        MyObject = gameObject;
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
        _animator = GetComponent<Animator>();
    }
    #endregion

    #region Setup
    public void SetupBox(StockInfo stockType)
    {
        _stockInfo = stockType;

        if (_stockInfo == null)
        {
            Debug.LogWarning("Cannot setup box: stockType is null");
            return;
        }

        List<Transform> activePoints = GetListOfPointsForStockType(_stockInfo.typeOfStock);

        if (_stockInBox.Count == 0)
        {
            FillBoxWithStock(activePoints);
        }
        _maxCapacity = activePoints.Count;
    }

    /// <summary>
    /// Returns the max number of Stock of the StockType that can be in this box.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private int GetCountForListOfStockType(StockInfo.StockType type) => type switch
    {
        StockInfo.StockType.BigDrink => _bigDrinkPoints?.Count ?? 0,
        StockInfo.StockType.Cereal => _cerealPoints?.Count ?? 0,
        StockInfo.StockType.TubeChips => _tubeChipPoints?.Count ?? 0,
        StockInfo.StockType.Fruit => _fruitPoints?.Count ?? 0,
        StockInfo.StockType.FruitLarge => _largeFruitPoints?.Count ?? 0,
        StockInfo.StockType.Vegetable => _vegetablePoints?.Count ?? 0,
        _ => 0
    };

    /// <summary>
    /// Returns the List of points based on the StockType
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private List<Transform> GetListOfPointsForStockType(StockInfo.StockType type) => type switch
    {
        StockInfo.StockType.BigDrink => _bigDrinkPoints,
        StockInfo.StockType.Cereal => _cerealPoints,
        StockInfo.StockType.TubeChips => _tubeChipPoints,
        StockInfo.StockType.Fruit => _fruitPoints,
        StockInfo.StockType.FruitLarge => _largeFruitPoints,
        StockInfo.StockType.Vegetable => _vegetablePoints,
        _ => new List<Transform>()
    };

    private void FillBoxWithStock(List<Transform> points)
    {
        if (_stockInfo == null || _stockInfo.stockObject == null)
        {
            Debug.LogWarning("Cannot fill box: stockInfo or stockObject is null");
            return;
        }

        foreach (Transform point in points)
        {
            StockObject stock = ObjectPool<StockObject>.GetFromPool(_stockInfo.stockObject, point);
            stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            stock.PlaceInBox();
            _stockInBox.Add(stock);
        }
    }

    public int GetMaxPossibleStockAmount(StockInfo.StockType type) => GetCountForListOfStockType(type);
    #endregion

    #region Movement
    private void MoveToHoldPosition(float deltaTime)
    {
        transform.SetLocalPositionAndRotation(Vector3.MoveTowards(
            transform.localPosition,
            Vector3.zero,
            MOVE_SPEED * deltaTime
        ), Quaternion.Slerp(
            transform.localRotation,
            Quaternion.identity,
            MOVE_SPEED * deltaTime
        ));
    }
    #endregion

    #region Pickup/Release
    public void Pickup(Transform holdPoint)
    {
        SetPhysicsState(true, false);
        transform.SetParent(holdPoint);
        _isHeld = true;
    }

    public void Release()
    {
        SetPhysicsState(false, true);
        _isHeld = false;
    }

    private void SetPhysicsState(bool isKinematic, bool colliderEnabled)
    {
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = isKinematic;
        }

        if (_collider != null)
        {
            _collider.enabled = colliderEnabled;
        }
    }
    #endregion

    #region Box Operations
    public void OpenClose()
    {
        _isOpen = !_isOpen;

        if (_animator != null)
        {
            _animator.SetBool(OPEN_BOX_ANIMATOR_PARAMETER, _isOpen);
        }

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(2);
    }

    public void PlaceStockOnShelf(ShelfSpaceController shelf)
    {
        if (_stockInBox.Count == 0 || shelf == null)
        {
            return;
        }

        int lastIndex = _stockInBox.Count - 1;
        StockObject stockToPlace = _stockInBox[lastIndex];

        if (stockToPlace == null || stockToPlace.StockInfo == null)
        {
            Debug.LogWarning("Cannot place stock on shelf: stock or its StockInfo is null");
            _stockInBox.RemoveAt(lastIndex);
            return;
        }

        shelf.PlaceStock(stockToPlace);

        if (stockToPlace.IsPlaced)
        {
            _stockInBox.RemoveAt(lastIndex);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(7);
        }

        if (_stockInBox.Count == 0)
            _stockInfo = null;

        if (!_isOpen)
        {
            OpenClose();
        }
    }

    public void TakeStockFromShelf(StockObject stock)
    {
        if (stock == null || stock.StockInfo == null)
        {
            Debug.LogWarning("Cannot take stock from shelf: stock or its StockInfo is null");
            return;
        }

        // If box is empty, set the stock info from the first item
        if (_stockInBox.Count == 0)
        {
            _stockInfo = stock.StockInfo;
        }

        // Double-check that stockInfo is valid
        if (_stockInfo == null)
        {
            Debug.LogWarning("Cannot take stock: box StockInfo is still null after assignment");
            return;
        }

        // Get the appropriate point list for this stock type
        List<Transform> points = GetListOfPointsForStockType(_stockInfo.typeOfStock);

        // Check if box is full
        if (_stockInBox.Count >= points.Count || points.Count == 0)
        {
            return;
        }

        // Get the next available point
        Transform targetPoint = points[_stockInBox.Count];

        if (targetPoint == null)
        {
            Debug.LogWarning("Cannot take stock: target point is null");
            return;
        }

        // Place stock in box - set parent and make kinematic immediately to prevent wiggling
        stock.transform.SetParent(targetPoint);
        stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        stock.PlaceInBox();
        _stockInBox.Add(stock);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(6);

        if (_stockInBox.Count == 1)
        {
            _maxCapacity = GetListOfPointsForStockType(stock.StockInfo.typeOfStock).Count;
        }

        // Open box if it's not already open
        if (!_isOpen)
        {
            OpenClose();
        }
    }

    public bool CanTakeStockFromHand(StockObject stock)
    {
        if (stock == null || stock.StockInfo == null)
        {
            Debug.LogWarning("Cannot take stock from hand: stock or its StockInfo is null");
            return false;
        }

        // If box is empty, set the stock info from the first item
        if (_stockInBox.Count == 0)
        {
            _stockInfo = stock.StockInfo;
        }

        // Double-check that stockInfo is valid
        if (_stockInfo == null)
        {
            Debug.LogWarning("Cannot take stock from hand: box StockInfo is still null after assignment");
            return false;
        }

        // Get the appropriate point list for this stock type
        List<Transform> points = GetListOfPointsForStockType(_stockInfo.typeOfStock);

        // Check if box is full
        if (_stockInBox.Count >= points.Count || points.Count == 0)
        {
            return false;
        }

        // Get the next available point
        Transform targetPoint = points[_stockInBox.Count];

        if (targetPoint == null)
        {
            Debug.LogWarning("Cannot take stock from hand: target point is null");
            return false;
        }

        // Place stock in box - set parent and make kinematic immediately to prevent wiggling
        stock.transform.SetParent(targetPoint);
        stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        stock.PlaceInBox();
        _stockInBox.Add(stock);
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(6);

        if (_stockInBox.Count == 1)
        {
            _maxCapacity = GetListOfPointsForStockType(stock.StockInfo.typeOfStock).Count;
        }

        // Open box if it's not already open
        if (!_isOpen)
        {
            OpenClose();
        }

        return true;
    }

    public void TrashObject()
    {
        ObjectPool<StockBoxController>.ReturnToPool(this);
    }

    public void OnInteract(Transform holdPoint)
    {
        Pickup(holdPoint);
        if (!OpenBox)
            OpenClose();
    }

    public string GetInteractionPrompt()
    {
        if (_stockInBox.Count > 0 && _stockInBox[0] != null && _stockInBox[0].StockInfo != null)
        {
            return $"Box of {_stockInBox[0].StockInfo.Name}";
        }
        return "Empty Box";
    }
    #endregion
}
#endregion

#region Claude Code v1
//using masonbell;
//using System.Collections.Generic;
//using UnityEngine;

//public class StockBoxController : MonoBehaviour, ITrashable
//{
//    #region Serialized Fields
//    [Header("Stock Settings")]
//    [SerializeField] private StockInfo _stockInfo;
//    [SerializeField] private List<StockObject> _stockInBox;

//    [Header("Point Lists")]
//    [SerializeField] private List<Transform> _bigDrinkPoints;
//    [SerializeField] private List<Transform> _cerealPoints;
//    [SerializeField] private List<Transform> _tubeChipPoints;
//    [SerializeField] private List<Transform> _fruitPoints;
//    [SerializeField] private List<Transform> _largeFruitPoints;
//    [SerializeField] private List<Transform> _vegetablePoints;

//    [Header("Debug")]
//    [SerializeField] private bool _testFill;
//    #endregion

//    #region Private Fields
//    private const float MOVE_SPEED = 10f;
//    private const string OPEN_BOX_ANIMATOR_PARAMETER = "openBox";

//    private bool _isHeld;
//    private Rigidbody _rigidbody;
//    private Collider _collider;
//    private Animator _animator;
//    private bool _isOpen;
//    #endregion

//    #region Properties
//    public Rigidbody Rb => _rigidbody;
//    public Collider Col => _collider;
//    public bool OpenBox => _isOpen;
//    public StockInfo StockInfo => _stockInfo;
//    public List<StockObject> StockInBox => _stockInBox;
//    #endregion

//    #region Unity Lifecycle
//    private void Awake()
//    {
//        CacheComponents();
//    }

//    private void Update()
//    {
//        if (_testFill)
//        {
//            _testFill = false;
//            SetupBox(_stockInfo);
//        }

//        if (_isHeld)
//        {
//            MoveToHoldPosition(Time.deltaTime);
//        }
//    }
//    #endregion

//    #region Initialization
//    private void CacheComponents()
//    {
//        _rigidbody = GetComponent<Rigidbody>();
//        _collider = GetComponent<Collider>();
//        _animator = GetComponent<Animator>();
//    }
//    #endregion

//    #region Setup
//    public void SetupBox(StockInfo stockType)
//    {
//        _stockInfo = stockType;
//        List<Transform> activePoints = GetPointsForStockType(_stockInfo.typeOfStock);

//        if (_stockInBox.Count == 0)
//        {
//            FillBoxWithStock(activePoints);
//        }
//    }

//    private List<Transform> GetPointsForStockType(StockInfo.StockType type)
//    {
//        return type switch
//        {
//            StockInfo.StockType.BigDrink => _bigDrinkPoints,
//            StockInfo.StockType.Cereal => _cerealPoints,
//            StockInfo.StockType.TubeChips => _tubeChipPoints,
//            StockInfo.StockType.Fruit => _fruitPoints,
//            StockInfo.StockType.FruitLarge => _largeFruitPoints,
//            StockInfo.StockType.Vegetable => _vegetablePoints,
//            _ => new List<Transform>()
//        };
//    }

//    private void FillBoxWithStock(List<Transform> points)
//    {
//        foreach (Transform point in points)
//        {
//            StockObject stock = ObjectPool<StockObject>.GetFromPool(_stockInfo.stockObject, point);
//            stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//            stock.PlaceInBox();
//            _stockInBox.Add(stock);
//        }
//    }

//    public int GetStockAmount(StockInfo.StockType type)
//    {
//        List<Transform> points = GetPointsForStockType(type);
//        return points.Count;
//    }
//    #endregion

//    #region Movement
//    private void MoveToHoldPosition(float deltaTime)
//    {
//        transform.SetLocalPositionAndRotation(Vector3.MoveTowards(
//            transform.localPosition,
//            Vector3.zero,
//            MOVE_SPEED * deltaTime
//        ), Quaternion.Slerp(
//            transform.localRotation,
//            Quaternion.identity,
//            MOVE_SPEED * deltaTime
//        ));
//    }
//    #endregion

//    #region Pickup/Release
//    public void Pickup(Transform holdPoint)
//    {
//        SetPhysicsState(true, false);
//        transform.SetParent(holdPoint);
//        _isHeld = true;
//    }

//    public void Release()
//    {
//        SetPhysicsState(false, true);
//        _isHeld = false;
//    }

//    private void SetPhysicsState(bool isKinematic, bool colliderEnabled)
//    {
//        if (_rigidbody != null)
//        {
//            _rigidbody.isKinematic = isKinematic;
//        }

//        if (_collider != null)
//        {
//            _collider.enabled = colliderEnabled;
//        }
//    }
//    #endregion

//    #region Box Operations
//    public void OpenClose()
//    {
//        _isOpen = !_isOpen;

//        if (_animator != null)
//        {
//            _animator.SetBool(OPEN_BOX_ANIMATOR_PARAMETER, _isOpen);
//        }
//    }

//    public void PlaceStockOnShelf(ShelfSpaceController shelf)
//    {
//        if (_stockInBox.Count == 0)
//        {
//            return;
//        }

//        int lastIndex = _stockInBox.Count - 1;
//        StockObject stockToPlace = _stockInBox[lastIndex];

//        shelf.PlaceStock(stockToPlace);

//        if (stockToPlace.IsPlaced)
//        {
//            _stockInBox.RemoveAt(lastIndex);
//        }

//        if (!_isOpen)
//        {
//            OpenClose();
//        }
//    }

//    public void TrashObject()
//    {
//        ObjectPool<StockBoxController>.ReturnToPool(this);
//    }
//    #endregion
//}
#endregion

#region James' Code
//using masonbell;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class StockBoxController : MonoBehaviour, ITrashable
//{
//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public StockInfo stockInfo;
//    public List<Transform> bigDrinkPoints;
//    public List<Transform> cerealPoints;
//    public List<Transform> tubeChipPoints;
//    public List<Transform> fruitPoints;
//    public List<Transform> largeFruitPoints;
//    public List<Transform> vegetablePoints;
//    public List<StockObject> stockInBox;

//    public bool testFill;
//    #endregion

//    #region Serialized Private Fields
//    #endregion

//    #region Private Fields
//    private const float moveSpeed = 10f;

//    private bool _isHeld;
//    #endregion

//    #region Public Properties
//    public Rigidbody Rb { get; private set; }
//    public Collider Col { get; private set; }
//    public Animator Anim { get; private set; }
//    public bool OpenBox { get; private set; }
//    #endregion

//    #region Unity Callbacks
//    private void Awake()
//    {
//        Rb = GetComponent<Rigidbody>();
//        Col = GetComponent<Collider>();
//        Anim = GetComponent<Animator>();
//    }

//    private void Update()
//    {
//        if (testFill)
//        {
//            testFill = false;
//            SetupBox(stockInfo);
//        }

//        if (_isHeld)
//        {
//            transform.SetLocalPositionAndRotation(Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime),
//                Quaternion.Slerp(transform.localRotation, Quaternion.identity, moveSpeed * Time.deltaTime));
//        }
//    }
//    #endregion

//    #region Public Methods
//    public int GetStockAmount(StockInfo.StockType type)
//    {
//        int toReturn = 0;

//        switch (type)
//        {
//            case StockInfo.StockType.Cereal:
//                toReturn = cerealPoints.Count;
//                break;
//            case StockInfo.StockType.BigDrink:
//                toReturn = bigDrinkPoints.Count;
//                break;
//            case StockInfo.StockType.TubeChips:
//                toReturn = tubeChipPoints.Count;
//                break;
//            case StockInfo.StockType.Fruit:
//                toReturn = fruitPoints.Count;
//                break;
//            case StockInfo.StockType.FruitLarge:
//                toReturn = largeFruitPoints.Count;
//                break;
//            case StockInfo.StockType.Vegetable:
//                toReturn = vegetablePoints.Count;
//                break;
//            default:
//                break;
//        }

//        return toReturn;
//    }
//    public void SetupBox(StockInfo stockType)
//    {
//        stockInfo = stockType;

//        List<Transform> activePoints = new();

//        switch (stockInfo.typeOfStock)
//        {
//            case StockInfo.StockType.BigDrink:
//                activePoints.AddRange(bigDrinkPoints);
//                break;
//            case StockInfo.StockType.Cereal:
//                activePoints.AddRange(cerealPoints);
//                break;
//            case StockInfo.StockType.TubeChips:
//                activePoints.AddRange(tubeChipPoints);
//                break;
//            case StockInfo.StockType.Fruit:
//                activePoints.AddRange(fruitPoints);
//                break;
//            case StockInfo.StockType.FruitLarge:
//                activePoints.AddRange(largeFruitPoints);
//                break;
//            case StockInfo.StockType.Vegetable:
//                activePoints.AddRange(vegetablePoints);
//                break;
//        }

//        if (stockInBox.Count == 0)
//        {
//            for (int i = 0; i < activePoints.Count; i++)
//            {
//                StockObject stock = ObjectPool<StockObject>.GetFromPool(stockType.stockObject, activePoints[i]);
//                stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//                stockInBox.Add(stock);
//                stock.PlaceInBox();
//            }
//        }
//    }

//    public void Pickup(Transform holdPoint)
//    {
//        Rb.isKinematic = true;
//        transform.SetParent(holdPoint);
//        Col.enabled = false;
//        _isHeld = true;
//    }

//    public void Release()
//    {
//        Rb.isKinematic = false;
//        Col.enabled = true;
//        _isHeld = false;
//    }

//    public void OpenClose()
//    {
//        OpenBox = !OpenBox;
//        Anim.SetBool("openBox", OpenBox);
//    }

//    public void PlaceStockOnShelf(ShelfSpaceController shelf)
//    {
//        if (stockInBox.Count > 0)
//        {
//            shelf.PlaceStock(stockInBox[^1]);

//            if (stockInBox[^1].IsPlaced)
//            {
//                stockInBox.RemoveAt(stockInBox.Count - 1);
//            }
//        }

//        if (!OpenBox)
//            OpenClose();
//    }

//    public void TrashObject()
//    {
//        ObjectPool<StockBoxController>.ReturnToPool(this);
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}
#endregion