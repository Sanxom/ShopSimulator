using PrimeTween;
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
    private void Awake() => CacheComponents();

    private void Update()
    {
        if (_testFill)
        {
            _testFill = false;
            SetupBox(_stockInfo);
        }

        if (_isHeld)
            MoveToHoldPosition(Time.deltaTime);
    }

    private void OnDisable() => _maxCapacity = 0;
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

        if (_stockInfo == null) return;

        List<Transform> activePoints = GetListOfPointsForStockType(_stockInfo.typeOfStock);

        if (_stockInBox.Count == 0)
            FillBoxWithStock(activePoints);

        _maxCapacity = activePoints.Count;
    }

    /// <summary>
    /// Returns the max number of Stock of the StockType that can be in this box.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    private int GetCountForListOfStockType(StockInfo.StockType type) => type switch
    {
        StockInfo.StockType.BigDrink => _bigDrinkPoints.Count,
        StockInfo.StockType.Cereal => _cerealPoints.Count,
        StockInfo.StockType.TubeChips => _tubeChipPoints.Count,
        StockInfo.StockType.Fruit => _fruitPoints.Count,
        StockInfo.StockType.FruitLarge => _largeFruitPoints.Count,
        StockInfo.StockType.Vegetable => _vegetablePoints.Count,
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
        if (_stockInfo == null || _stockInfo.stockObject == null) return;

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
            _rigidbody.isKinematic = isKinematic;

        if (_collider != null)
            _collider.enabled = colliderEnabled;
    }
    #endregion

    #region Box Operations
    public void OpenClose()
    {
        _isOpen = !_isOpen;

        if (_animator != null)
            _animator.SetBool(OPEN_BOX_ANIMATOR_PARAMETER, _isOpen);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(2);
    }

    public void PlaceStockOnShelf(ShelfSpaceController shelf)
    {
        if (_stockInBox.Count == 0 || shelf == null) return;

        int lastIndex = _stockInBox.Count - 1;
        StockObject stockToPlace = _stockInBox[lastIndex];

        if (stockToPlace == null || stockToPlace.StockInfo == null)
        {
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
            OpenClose();
    }

    public void TakeStockFromShelf(StockObject stock)
    {
        if (stock == null || stock.StockInfo == null) return;

        if (_stockInBox.Count == 0) 
            _stockInfo = stock.StockInfo;

        if (_stockInfo == null) return;

        List<Transform> points = GetListOfPointsForStockType(_stockInfo.typeOfStock);

        if (_stockInBox.Count == points.Count || points.Count == 0) return;

        Transform targetPoint = points[_stockInBox.Count];

        if (targetPoint == null) return;

        stock.transform.SetParent(targetPoint);
        //stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        MoveToPlacementPoint(stock, Vector3.zero, Quaternion.identity);
        stock.PlaceInBox();
        _stockInBox.Add(stock);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(6);

        if (_stockInBox.Count == 1) 
            _maxCapacity = GetMaxPossibleStockAmount(stock.StockInfo.typeOfStock);

        if (!_isOpen) 
            OpenClose();
    }

    private void MoveToPlacementPoint(StockObject stock, Vector3 endPointPosition, Quaternion endPointRotation)
    {
        TweenSettings settings = new(duration: 0f);
        Tween.LocalPositionAtSpeed(stock.transform, endPointPosition, MOVE_SPEED);
        Tween.LocalRotation(stock.transform, endPointRotation, settings);
    }

    public bool CanTakeStockFromHand(StockObject stock)
    {
        if (stock == null || stock.StockInfo == null) return false;

        if (_stockInBox.Count == 0)
            _stockInfo = stock.StockInfo;

        if (_stockInfo == null) return false;

        List<Transform> points = GetListOfPointsForStockType(_stockInfo.typeOfStock);

        if (_stockInBox.Count == points.Count || points.Count == 0)
            return false;

        Transform targetPoint = points[_stockInBox.Count];

        if (targetPoint == null) return false;

        stock.transform.SetParent(targetPoint);
        stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        stock.PlaceInBox();
        _stockInBox.Add(stock);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(6);

        if (_stockInBox.Count == 1)
            _maxCapacity = GetMaxPossibleStockAmount(stock.StockInfo.typeOfStock);

        if (!_isOpen)
            OpenClose();

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
            return $"Box of {_stockInBox[0].StockInfo.Name}";

        return "Empty Box";
    }
    #endregion
}