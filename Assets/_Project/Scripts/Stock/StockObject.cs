using PrimeTween;
using System.Collections;
using UnityEngine;

public class StockObject : MonoBehaviour, IInteractable, ITrashable
{
    #region Serialized Fields
    [SerializeField] private StockInfo _stockInfo;
    #endregion

    #region Private Fields
    private const float MOVE_SPEED = 10f;

    private Transform _bagPositionInWorld;
    private bool _isHeld;
    private bool _isPlaced;
    private bool _isInBag;
    private bool _isOnCheckoutCounter;
    private Rigidbody _rigidbody;
    private Collider _collider;
    #endregion

    #region Properties
    public GameObject MyObject { get; set; }
    public StockInfo StockInfo => _stockInfo;
    public Rigidbody Rb => _rigidbody;
    public Collider Col => _collider;
    public string InteractionPrompt { get; private set; }
    public bool IsPlaced => _isPlaced;

    public bool IsOnCheckoutCounter { get => _isOnCheckoutCounter; set => _isOnCheckoutCounter = value; }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        CacheComponents();
        InteractionPrompt = gameObject.name;
    }

    private void OnEnable() => ResetState();

    private void Start() => RefreshStockInfo();
    #endregion

    #region Initialization
    private void CacheComponents()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void ResetState()
    {
        _isHeld = false;
        _isPlaced = false;
        _isInBag = false;
        _isOnCheckoutCounter = false;
    }

    private void RefreshStockInfo()
    {
        if (StockInfo != null && StockInfoController.Instance != null)
            _stockInfo = StockInfoController.Instance.GetStockInfo(StockInfo.Name);
    }
    #endregion

    #region Movement
    private void MoveToPlacedPosition(Transform placementPoint)
    {
        Tween.LocalPosition(transform, Vector3.zero, StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration);
        Tween.LocalRotation(transform, new Vector3(0f, 90f, 0f), StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration);
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
        StartCoroutine(PickupCoroutine(holdPoint));
    }

    private IEnumerator PickupCoroutine(Transform holdPoint)
    {
        _isPlaced = false;
        SetPhysicsState(true, false);
        transform.SetParent(holdPoint);
        _isHeld = true;
        MoveToPlacedPosition(holdPoint);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(6);
        yield return StockInfoController.Instance.StockPickupAndPlaceWaitTime;
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
        _isInBag = true;
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

    public void TrashObject() => ObjectPool<StockObject>.ReturnToPool(this);
    #endregion

    #region Private Methods
    private void SetPhysicsState(bool isKinematic, bool colliderEnabled)
    {
        if (_rigidbody != null)
            _rigidbody.isKinematic = isKinematic;

        if (_collider != null)
            _collider.enabled = colliderEnabled;
    }

    public void OnInteract(Transform holdPoint) => Pickup(holdPoint);

    public string GetInteractionPrompt() => $"{_stockInfo.Name}";
    #endregion
}