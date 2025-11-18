using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

public class StockBoxController : InteractableObject, ITrashable
{
    #region Serialized Fields
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

    private WaitForSeconds _stockPickupAndPlaceWaitTime;
    #endregion

    #region Properties
    [field: SerializeField] public List<StockObject> StockInBox { get; private set; }
    [field: SerializeField] public StockInfo StockInfo { get; private set; }

    public Rigidbody Rb { get; private set; }
    public Collider Col { get; private set; }
    public Animator Anim { get; private set; }
    public int MaxCapacity { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsBoxOpen { get; private set; }
    public bool IsTaking { get; private set; }
    public bool IsPlacingStock { get; private set; }
    public bool CanTrash { get; private set; }
    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        CacheComponents();
    }

    private void Start()
    {
        _stockPickupAndPlaceWaitTime = new(StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration);
    }

    private void Update()
    {
        if (_testFill)
        {
            _testFill = false;
            SetupBox(StockInfo);
        }
    }

    private void OnDisable() => MaxCapacity = 0;
    #endregion

    #region Initialization
    private void CacheComponents()
    {
        Rb = GetComponent<Rigidbody>();
        Col = GetComponent<Collider>();
        Anim = GetComponent<Animator>();
    }
    #endregion

    #region Setup
    public void SetupBox(StockInfo stockType)
    {
        StockInfo = stockType;

        if (StockInfo == null) return;

        List<Transform> activePoints = GetListOfPointsForStockType(StockInfo.typeOfStock);

        if (StockInBox.Count == 0)
            FillBoxWithStock(activePoints);

        MaxCapacity = activePoints.Count;
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
        if (StockInfo == null || StockInfo.stockObject == null) return;

        foreach (Transform point in points)
        {
            StockObject stock = ObjectPool<StockObject>.GetFromPool(StockInfo.stockObject, point);
            stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            stock.PlaceInBox();
            StockInBox.Add(stock);
        }
    }

    public int GetMaxPossibleStockAmount(StockInfo.StockType type) => GetCountForListOfStockType(type);
    #endregion

    #region Movement
    private void MoveToHoldPosition()
    {
        Tween.LocalPosition(transform, Vector3.zero, MOVE_SPEED * Time.deltaTime);
        Tween.LocalRotation(transform, Quaternion.identity, MOVE_SPEED * Time.deltaTime);
    }
    #endregion

    #region Pickup/Release
    public void Pickup(Transform holdPoint)
    {
        SetPhysicsState(true, false);
        transform.SetParent(holdPoint);
        MoveToHoldPosition();
        if (!IsBoxOpen)
            OpenClose();
    }

    public void Release()
    {
        SetPhysicsState(false, true);
        if (StockInBox.Count == MaxCapacity && IsBoxOpen)
            OpenClose();
    }

    private void SetPhysicsState(bool isKinematic, bool colliderEnabled)
    {
        if (Rb != null)
            Rb.isKinematic = isKinematic;

        if (Col != null)
            Col.enabled = colliderEnabled;
    }
    #endregion

    #region Box Operations
    public void OpenClose()
    {
        IsBoxOpen = !IsBoxOpen;

        if (Anim != null)
            Anim.SetBool(OPEN_BOX_ANIMATOR_PARAMETER, IsBoxOpen);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(2);
    }

    public void PlaceStockOnShelf(ShelfSpaceController shelf)
    {
        StartCoroutine(PlaceStockOnShelfCoroutine(shelf));
    }

    private IEnumerator PlaceStockOnShelfCoroutine(ShelfSpaceController shelf)
    {
        if (IsPlacingStock || IsTaking) yield break;
        if (StockInBox.Count == 0 || shelf == null) yield break;

        int lastIndex = StockInBox.Count - 1;
        StockObject stockToPlace = StockInBox[lastIndex];

        if (stockToPlace == null || stockToPlace.StockInfo == null)
        {
            StockInBox.RemoveAt(lastIndex);
            yield break;
        }

        if (!IsBoxOpen)
            OpenClose();

        IsPlacingStock = true;
        shelf.PlaceStock(stockToPlace);

        if (stockToPlace.IsPlaced)
        {
            StockInBox.RemoveAt(lastIndex);

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(7);
        }

        if (StockInBox.Count == 0)
            StockInfo = null;
        yield return _stockPickupAndPlaceWaitTime;
        IsPlacingStock = false;
    }

    private IEnumerator TakeStockFromShelfCoroutine(StockObject stock)
    {
        if (IsTaking || IsPlacingStock) yield break;
        if (stock == null || stock.StockInfo == null) yield break;

        if (StockInBox.Count == 0)
            StockInfo = stock.StockInfo;

        if (StockInfo == null) yield break;

        List<Transform> points = GetListOfPointsForStockType(StockInfo.typeOfStock);

        if (StockInBox.Count == points.Count || points.Count == 0) yield break;

        Transform targetPoint = points[StockInBox.Count];

        if (targetPoint == null) yield break;

        if (!IsBoxOpen)
            OpenClose();

        IsTaking = true;
        stock.transform.SetParent(targetPoint);
        MoveToPlacementPoint(stock, Vector3.zero, Quaternion.identity);
        StockInBox.Add(stock);
        stock.PlaceInBox();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(6);

        if (StockInBox.Count == 1)
            MaxCapacity = GetMaxPossibleStockAmount(stock.StockInfo.typeOfStock);
        if (StockInBox.Count == MaxCapacity)
            OpenClose();
        yield return _stockPickupAndPlaceWaitTime;
        IsTaking = false;
    }

    public void TakeStockFromShelf(StockObject stock)
    {
        StartCoroutine(TakeStockFromShelfCoroutine(stock));
    }

    private void MoveToPlacementPoint(StockObject stock, Vector3 endPointPosition, Quaternion endPointRotation)
    {
        StartCoroutine(MoveToPlacementPointCoroutine(stock, endPointPosition, endPointRotation));
    }

    private IEnumerator MoveToPlacementPointCoroutine(StockObject stock, Vector3 endPointPosition, Quaternion endPointRotation)
    {
        yield return Tween.LocalRotation(stock.transform, endPointRotation, 0f).ToYieldInstruction();
        yield return Tween.LocalPosition(stock.transform, endPointPosition, StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration).ToYieldInstruction();
    }

    public bool CanTakeStockFromHand(StockObject stock)
    {
        if (stock == null || stock.StockInfo == null) return false;
        if (stock.IsMoving) return false;

        if (StockInBox.Count == 0)
            StockInfo = stock.StockInfo;

        if (StockInfo == null) return false;

        if (StockInfo != stock.StockInfo) return false;

        List<Transform> points = GetListOfPointsForStockType(StockInfo.typeOfStock);

        if (StockInBox.Count == points.Count || points.Count == 0)
            return false;

        Transform targetPoint = points[StockInBox.Count];

        if (targetPoint == null) return false;

        stock.transform.SetParent(targetPoint);
        MoveToPlacementPoint(stock, Vector3.zero, Quaternion.identity);
        stock.PlaceInBox();
        StockInBox.Add(stock);

        if (!IsBoxOpen)
            OpenClose();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(6);

        if (StockInBox.Count == 1)
            MaxCapacity = GetMaxPossibleStockAmount(stock.StockInfo.typeOfStock);
        if (StockInBox.Count == MaxCapacity)
            OpenClose();

        return true;
    }

    public StockObject TakeStockFromBoxIntoHand()
    {
        if (StockInBox.Count == 0) return null;

        int lastIndex = StockInBox.Count - 1;
        StockObject objectToReturn = StockInBox[lastIndex];
        StockInBox.RemoveAt(lastIndex);

        if (StockInBox.Count == 0)
            StockInfo = null;

        return objectToReturn;
    }

    public void TrashObject()
    {
        if (StockInBox.Count > 0) return;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(10);
        ObjectPool<StockBoxController>.ReturnToPool(this);
    }

    public override string GetInteractionPrompt()
    {
        if (StockInfo == null) return "Box of Stock";
        UIController.Instance.ShowInteractionPrompt();
        StringBuilder sb = new();
        sb.Append($"Box of {StockInBox.Count} {StockInfo.name}");
        UIController.Instance.SetInteractionText(sb.ToString());
        return sb.ToString();
    }

    public override void OnInteract(PlayerInteraction player)
    {
        if (!player.IsHoldingSomething)
        {
            Pickup(player.BoxHoldPoint);
            player.HeldBox = this;
            player.HeldObject = MyObject;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(1);
            return;
        }

        if (!CanTakeStockFromHand(player.HeldStock)) return;

        player.RemoveHeldObjectReference();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(7);
    }

    public override void OnTake(PlayerInteraction player)
    {
        if (!player.IsHoldingSomething)
        {
            StockObject temp = TakeStockFromBoxIntoHand();
            if (temp == null) return;

            if (!IsBoxOpen)
                OpenClose();

            player.HeldStock = temp;
            player.HeldObject = temp.gameObject;
            temp.Pickup(player.StockHoldPoint);
        }
    }
    #endregion
}