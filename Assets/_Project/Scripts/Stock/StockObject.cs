using PrimeTween;
using System.Collections;
using UnityEngine;

public class StockObject : InteractableObject, ITrashable
{
    #region Serialized Fields
    [SerializeField] private StockInfo _stockInfo;
    #endregion

    #region Private Fields
    private Transform _bagPositionInWorld;
    private bool _isPlaced;
    private Rigidbody _rigidbody;
    private Collider _collider;
    #endregion

    #region Properties
    public StockInfo StockInfo => _stockInfo;
    public Rigidbody Rb => _rigidbody;
    public Collider Col => _collider;
    public bool IsPlaced => _isPlaced;

    public bool IsMoving { get; private set; }
    public bool IsOnCheckoutCounter { get; private set; }
    public bool CanTrash { get; private set; }
    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        CacheComponents();
    }

    private void OnEnable() => ResetState();

    private void Start()
    {
        RefreshStockInfo();
    }
    #endregion

    #region Initialization
    private void CacheComponents()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void ResetState()
    {
        _isPlaced = false;
        IsOnCheckoutCounter = false;
    }

    private void RefreshStockInfo()
    {
        if (StockInfo != null && StockInfoController.Instance != null)
            _stockInfo = StockInfoController.Instance.GetStockInfo(StockInfo.Name);
    }
    #endregion

    #region Movement
    private void MoveToPickupPosition(Transform placementPoint)
    {
        StartCoroutine(MoveToPickupPositionCoroutine(placementPoint));
    }

    private IEnumerator MoveToPickupPositionCoroutine(Transform placementPoint)
    {
        yield return Tween.LocalRotation(transform, new Vector3(0f, 90f, 0f), 0f).ToYieldInstruction();
        yield return Tween.LocalPosition(transform, Vector3.zero, StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration).ToYieldInstruction();
    }

    private void MoveToBagPosition(Transform bagPosition)
    {
        if (bagPosition == null) return;
        Tween.Position(transform, bagPosition.position, StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration);
        Tween.LocalRotation(transform, Quaternion.identity, StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration);
        Tween.Scale(transform, Vector3.zero, StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration);
    }
    #endregion

    #region Public Methods
    public void MakePlaced()
    {
        _isPlaced = true;
        SetPhysicsState(true, false);
    }

    public void Pickup(Transform holdPoint)
    {
        if (IsMoving) return;

        StartCoroutine(PickupCoroutine(holdPoint));
    }

    private IEnumerator PickupCoroutine(Transform holdPoint)
    {
        IsMoving = true;
        _isPlaced = false;
        SetPhysicsState(true, false);
        transform.SetParent(holdPoint);
        MoveToPickupPosition(holdPoint);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(6);
        yield return StockInfoController.Instance.StockPickupAndPlaceWaitTime;
        IsMoving = false;
    }

    public void Release()
    {
        ResetState();
        SetPhysicsState(false, true);
    }

    public void PlaceInBox()
    {
        SetPhysicsState(true, false);
    }

    public void PlaceInBag(Transform bagPosition)
    {
        _bagPositionInWorld = bagPosition;
        MoveToBagPosition(_bagPositionInWorld);
        MakePlaced();
    }

    public void PlaceOnCheckoutCounter(Checkout checkout)
    {
        IsOnCheckoutCounter = true;
        checkout.StockObjectsOnCounter.Add(this);
        SetPhysicsState(true, true);
    }

    public void MoveToCheckoutBag()
    {
        PlaceInBag(_bagPositionInWorld);
        Checkout.Instance.AddToTotalPrice(_stockInfo.currentPrice);
        AudioManager.Instance.PlaySFX(3);
        Invoke(nameof(TrashObject), StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration);
    }

    public void TrashObject()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(10);

        ObjectPool<StockObject>.ReturnToPool(this);
    }
    #endregion

    #region Private Methods
    private void SetPhysicsState(bool isKinematic, bool colliderEnabled)
    {
        if (_rigidbody != null)
            _rigidbody.isKinematic = isKinematic;

        if (_collider != null)
            _collider.enabled = colliderEnabled;
    }

    public override void OnInteract(PlayerInteraction player)
    {
        if (player.IsHoldingSomething) return;

        if (IsOnCheckoutCounter)
        {
            MoveToCheckoutBag();
            return;
        }

        player.HeldStock = this;
        player.HeldObject = gameObject;
        Pickup(player.StockHoldPoint);
    }
    #endregion
}