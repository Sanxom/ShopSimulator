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
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        CacheComponents();
        InteractionPrompt = gameObject.name;
    }

    private void OnEnable() => ResetState();

    private void Start() => RefreshStockInfo();

    private void Update()
    {
        //float deltaTime = Time.deltaTime;

        //if (_isPlaced || _isHeld)
        //    MoveToPlacedPosition(deltaTime);

        //if (_isInBag)
        //    MoveToBagPosition(deltaTime);
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
        _isHeld = false;
        _isPlaced = false;
        _isInBag = false;
    }

    private void RefreshStockInfo()
    {
        if (StockInfo != null && StockInfoController.Instance != null)
            _stockInfo = StockInfoController.Instance.GetStockInfo(StockInfo.Name);
    }
    #endregion

    #region Movement
    private void MoveToPlacedPosition(float deltaTime)
    {
        print("Hey");
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

    private void MoveToBagPosition(float deltaTime)
    {
        if (_bagPositionInWorld == null) return;

        transform.SetPositionAndRotation(Vector3.MoveTowards(
            transform.position,
            _bagPositionInWorld.position,
            deltaTime
        ), Quaternion.Slerp(
            transform.localRotation,
            Quaternion.identity,
            deltaTime
        ));
        transform.localScale = Vector3.MoveTowards(
            transform.localScale,
            Vector3.zero,
            deltaTime
        );
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
        _isPlaced = false;
        SetPhysicsState(true, false);
        transform.SetParent(holdPoint);
        _isHeld = true;
        //transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(6);
    }

    public void Release()
    {
        _isHeld = false;
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
        MakePlaced();
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

//using masonbell;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class StockObject : MonoBehaviour, ITrashable
//{
//    #region Event Fields
//    #endregion

//    #region Public Fields
//    #endregion

//    #region Serialized Private Fields
//    #endregion

//    #region Private Fields
//    private const float moveSpeed = 10f;

//    private Transform _bagPositionInWorld;
//    private bool _isPlaced;
//    private bool _isInBag;
//    #endregion

//    #region Public Properties
//    [field: SerializeField] public StockInfo Info { get; private set; }

//    public Rigidbody Rb { get; private set; }
//    public Collider Col { get; private set; }
//    public string InteractionPrompt { get; set; }
//    public bool IsPlaced { get => _isPlaced; private set => _isPlaced = value; }
//    #endregion

//    #region Unity Callbacks
//    private void Awake()
//    {
//        Rb = GetComponent<Rigidbody>();
//        Col = GetComponent<Collider>();
//        InteractionPrompt = gameObject.name;
//    }

//    private void OnEnable()
//    {
//        _isPlaced = false;
//    }

//    private void Start()
//    {
//        Info = StockInfoController.Instance.GetStockInfo(Info.name);
//    }

//    private void Update()
//    {
//        if (_isPlaced)
//        {
//            transform.SetLocalPositionAndRotation(Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime), 
//                Quaternion.Slerp(transform.localRotation, Quaternion.identity, moveSpeed * Time.deltaTime));
//        }

//        if (_isInBag)
//        {
//            transform.SetPositionAndRotation(Vector3.MoveTowards(transform.position, _bagPositionInWorld.position, moveSpeed * Time.deltaTime),
//                Quaternion.Slerp(transform.rotation, Quaternion.identity, Time.deltaTime));
//            transform.localScale = Vector3.MoveTowards(transform.localScale, Vector3.zero, Time.deltaTime);
//        }
//    }
//    #endregion

//    #region Public Methods
//    public void MakePlaced()
//    {
//        _isPlaced = true;
//        Rb.isKinematic = true;
//        Col.enabled = false;
//    }

//    public void Pickup(Transform holdPoint)
//    {
//        Rb.isKinematic = true;
//        transform.SetParent(holdPoint);
//        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//        Col.enabled = false;
//        _isPlaced = false;
//    }

//    public void Release()
//    {
//        Rb.isKinematic = false;
//        Col.enabled = true;
//    }

//    public void PlaceInBox()
//    {
//        Rb.isKinematic = true;
//        Col.enabled = false;
//    }

//    public void PlaceInBag(Transform bagPosition)
//    {
//        _bagPositionInWorld = bagPosition;
//        _isInBag = true;
//        MakePlaced();
//    }

//    public void TrashObject()
//    {
//        ObjectPool<StockObject>.ReturnToPool(this);
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}