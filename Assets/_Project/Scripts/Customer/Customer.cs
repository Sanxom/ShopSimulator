using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Burst;
using UnityEngine;

public class Customer : MonoBehaviour
{
    public enum CustomerState
    {
        Entering,
        Browsing,
        Queueing,
        AtCheckout,
        Leaving
    }

    #region Serialized Fields
    [Header("Navigation")]
    [SerializeField] private List<NavPoint> _navPoints = new();
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _browseTime = 3f;
    [SerializeField] private float _waitAfterGrabbing = 0.5f;
    [SerializeField] private int _maxBrowsePoints = 5;

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _shoppingBag;
    [SerializeField] private Transform _shoppingBagDefaultParent;
    [SerializeField] private List<StockObject> _stockInBag = new();
    #endregion

    #region Private Fields
    private const string IS_MOVING_ANIMATOR_PARAMETER = "isMoving";

    private CustomerState _currentState;
    private FurnitureController _currentShelfCase;
    private Transform _shoppingBagDefaultTransform;
    private Vector3 _queuePoint;
    private Vector3 _queuePointAhead;
    private float _currentWaitTime;
    private int _browsePointsRemaining;
    private bool _hasGrabbedItem;
    private bool _isMoving = false;
    #endregion

    #region Properties
    public CustomerState CurrentState => _currentState;
    public float MoveSpeed => _moveSpeed;
    public float BrowseTime => _browseTime;
    public int MaxBrowsePoints => _maxBrowsePoints;

    public GameObject ShoppingBag { get => _shoppingBag; private set => _shoppingBag = value; }
    public Transform ShoppingBagDefaultTransform { get => _shoppingBagDefaultTransform; private set => _shoppingBagDefaultTransform = value; }
    public Transform ShoppingBagDefaultParent { get => _shoppingBagDefaultParent; private set => _shoppingBagDefaultParent = value; }
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        ShoppingBag.SetActive(false);
        InitializeCustomer();
    }

    private void Start()
    {
        SetupEntryPath();
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        switch (_currentState)
        {
            case CustomerState.Entering:
                //ProcessEntering(deltaTime);
                break;
            case CustomerState.Browsing:
                //ProcessBrowsing(deltaTime);
                break;
            case CustomerState.Queueing:
                //ProcessQueueing();
                break;
            case CustomerState.AtCheckout:
                //ProcessCheckout();
                break;
            case CustomerState.Leaving:
                //ProcessLeaving();
                break;
        }

        if (_currentState == CustomerState.AtCheckout && Checkout.Instance.CustomersInQueue[0] != this)
        {
            StartCoroutine(MoveToCheckoutCoroutine());
        }
    }
    #endregion

    #region Initialization
    private void InitializeCustomer()
    {
        _currentState = CustomerState.Entering;
        _navPoints.Clear();
        _stockInBag.Clear();
        _hasGrabbedItem = false;
    }

    private void SetupEntryPath()
    {
        _navPoints.AddRange(CustomerManager.Instance.GetEntryPoints());

        if (_navPoints.Count > 0)
        {
            transform.position = _navPoints[0].point.position;
            _currentWaitTime = _navPoints[0].waitTime;
            StartCoroutine(MoveToEntryPointCoroutine());
        }
    }

    private IEnumerator MoveToEntryPointCoroutine()
    {
        for (int i = 1; i < _navPoints.Count - 1; i++)
        {
            transform.LookAt(_navPoints[i].point.position);
            _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, true);
            yield return Tween.PositionAtSpeed(transform, _navPoints[i].point.position, MoveSpeed, Ease.Linear).ToYieldInstruction();
            _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, false);
            yield return new WaitForSeconds(_currentWaitTime);
        }

        TransitionToBrowsing();
    }
    #endregion

    #region Browsing
    private void TransitionToBrowsing()
    {
        _currentState = CustomerState.Browsing;
        _browsePointsRemaining = Random.Range(1, _maxBrowsePoints + 1);
        _browsePointsRemaining = Mathf.Clamp(_browsePointsRemaining, 1, StoreController.Instance.ShelvingCases.Count);
        GetBrowsePoint();
    }

    private IEnumerator BrowsingCoroutine()
    {
        Vector3 targetPosition;

        if (_navPoints.Count == 0)
        {
            TransitionToLeaving();
            yield break;
        }

        if (_currentShelfCase != null && _currentShelfCase.IsHeld)
        {
            CompleteBrowsePoint();
            yield break;
        }

        targetPosition = new
        (
            _navPoints[0].point.position.x,
            transform.position.y,
            _navPoints[0].point.position.z
        );

        transform.LookAt(targetPosition);
        _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, true);
        yield return Tween.PositionAtSpeed(transform, targetPosition, MoveSpeed, Ease.Linear).ToYieldInstruction();
        transform.LookAt(_currentShelfCase.transform);
        _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, false);

        if (_currentShelfCase != null && _currentShelfCase.IsHeld)
        {
            CompleteBrowsePoint();
            yield break;
        }

        yield return new WaitForSeconds(_currentWaitTime);

        if (_currentShelfCase != null && _currentShelfCase.IsHeld)
        {
            CompleteBrowsePoint();
            yield break;
        }

        StartNextPoint();

        if (_navPoints.Count == 0)
        {
            if (!_hasGrabbedItem)
                GrabStock();
            else
                CompleteBrowsePoint();
        }
    }

    private void GetBrowsePoint()
    {
        _navPoints.Clear();

        if ((StoreController.Instance.ShelvingCases.Count == 0) || (StoreController.Instance.ShelvingCases.Count == 1 && StoreController.Instance.ShelvingCases[0].IsHeld))
        {
            TransitionToLeaving();
            return;
        }

        int selectedShelf = Random.Range(0, StoreController.Instance.ShelvingCases.Count);
        _currentShelfCase = StoreController.Instance.ShelvingCases[selectedShelf];

        NavPoint browsePoint = new()
        {
            point = _currentShelfCase.CustomerStandPoint,
            waitTime = _browseTime * Random.Range(0.75f, 1.25f)
        };

        _navPoints.Add(browsePoint);
        _currentWaitTime = browsePoint.waitTime;

        StartCoroutine(BrowsingCoroutine());
    }

    public void GrabStock()
    {
        _hasGrabbedItem = true;

        if (_currentShelfCase == null || _currentShelfCase.Shelves.Count == 0)
        {
            return;
        }

        int randomShelf = Random.Range(0, _currentShelfCase.Shelves.Count);
        StockObject stock = _currentShelfCase.Shelves[randomShelf].GetStock();

        if (stock != null)
        {
            PlaceStockInBag(stock);
            SetupPostGrabWait();
        }
        else
            CompleteBrowsePoint();
    }

    private void PlaceStockInBag(StockObject stock)
    {
        ShoppingBag.SetActive(true);
        ShoppingBagDefaultTransform = ShoppingBag.transform;
        stock.transform.SetParent(ShoppingBag.transform);
        _stockInBag.Add(stock);
        stock.PlaceInBag(ShoppingBag.transform);
    }

    private void SetupPostGrabWait()
    {
        _navPoints.Clear();

        NavPoint waitPoint = new()
        {
            point = transform,
            waitTime = _waitAfterGrabbing * Random.Range(0.75f, 1.25f)
        };

        _navPoints.Add(waitPoint);
        _currentWaitTime = waitPoint.waitTime;
        StartCoroutine(BrowsingCoroutine());
    }

    private void CompleteBrowsePoint()
    {
        _hasGrabbedItem = false;
        _browsePointsRemaining--;

        if (_browsePointsRemaining > 0)
        {
            GetBrowsePoint();
        }
        else
        {
            DecideNextAction();
        }
    }

    private void DecideNextAction()
    {
        if (_stockInBag.Count > 0)
        {
            TransitionToQueueing();
        }
        else
        {
            TransitionToLeaving();
        }
    }

    private void StartNextPoint()
    {
        if (_navPoints.Count > 0)
        {
            _navPoints.RemoveAt(0);

            if (_navPoints.Count > 0)
            {
                _currentWaitTime = _navPoints[0].waitTime;
            }
        }
    }
    #endregion

    #region Queueing
    private void TransitionToQueueing()
    {
        _currentState = CustomerState.Queueing;
        Checkout.Instance.AddCustomerToQueue(this);
        StartCoroutine(MoveToQueueCoroutine());
    }

    private IEnumerator MoveToQueueCoroutine()
    {
        Tween.StopAll(this);
        _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, true);
        transform.LookAt(_queuePoint);
        yield return Tween.PositionAtSpeed(transform, _queuePoint, MoveSpeed, Ease.Linear).ToYieldInstruction();
        transform.LookAt(_queuePointAhead);
        Checkout.Instance.CustomersInQueue[0].transform.LookAt(Checkout.Instance.transform);
        _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, false);
        TransitionToCheckout();
    }
    #endregion

    #region AtCheckout
    private void TransitionToCheckout()
    {
        _currentState = CustomerState.AtCheckout;
        StartCoroutine(MoveToCheckoutCoroutine());
    }

    private IEnumerator MoveToCheckoutCoroutine()
    {
        Tween.StopAll(this);
        _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, true);
        transform.LookAt(_queuePoint);
        yield return Tween.PositionAtSpeed(transform, _queuePoint, MoveSpeed, Ease.Linear).ToYieldInstruction();
        transform.LookAt(_queuePointAhead);
        _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, false);
        StartCoroutine(AtCheckoutCoroutine());
    }

    private IEnumerator AtCheckoutCoroutine()
    {
        if (Checkout.Instance.CustomersInQueue[0] != this) yield break;

        Checkout.Instance.CustomersInQueue[0].transform.LookAt(Checkout.Instance.transform);
        Checkout.Instance.ShowCheckoutScreen();
        List<Transform> tempStockPositions = Checkout.Instance.CheckoutStockPositions;
        int stockCount = _stockInBag.Count;
        for (int i = 0; i < stockCount; i++)
        {
            StockObject tempStock = _stockInBag[0];
            tempStock.transform.SetParent(tempStockPositions[i]);
            yield return Tween.Scale(tempStock.transform, tempStock.StockInfo.defaultScale, 0.1f).ToYieldInstruction();
            yield return Tween.LocalPosition(tempStock.transform, Vector3.zero, 0.2f).ToYieldInstruction();
            tempStock.PlaceOnCheckoutCounter(Checkout.Instance);
            _stockInBag.RemoveAt(0);
        }
        ShoppingBag.transform.SetParent(Checkout.Instance.ShoppingBagPlacementPoint);
        yield return Tween.Scale(ShoppingBag.transform, new Vector3(1.2f, 1.2f, 1.2f), 0.1f).ToYieldInstruction();
        yield return Tween.LocalPosition(ShoppingBag.transform, Vector3.zero, 0.1f).ToYieldInstruction();
        yield return Tween.LocalRotation(ShoppingBag.transform, Quaternion.identity, 0.1f).ToYieldInstruction();
    }
    #endregion

    #region Leaving
    private IEnumerator LeavingCoroutine()
    {
        for (int i = 0; i < _navPoints.Count; i++)
        {
            transform.LookAt(_navPoints[i].point.position);
            _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, true);
            yield return Tween.PositionAtSpeed(transform, _navPoints[i].point.position, MoveSpeed, Ease.Linear).ToYieldInstruction();
            _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, false);
        }

        ObjectPool<Customer>.ReturnToPool(this);
    }
    #endregion

    #region Public Methods
    public void TransitionToLeaving()
    {
        _currentState = CustomerState.Leaving;
        _navPoints.Clear();
        _navPoints.AddRange(CustomerManager.Instance.GetExitPoints());
        StartCoroutine(LeavingCoroutine());
    }

    public void UpdateQueuePoint(Vector3 newPoint, Vector3 pointAhead)
    {
        _queuePoint = newPoint;
        _queuePointAhead = pointAhead;
        transform.LookAt(pointAhead);
    }

    public float GetTotalSpendAmount()
    {
        float total = 0f;

        foreach (StockObject stock in _stockInBag)
        {
            if (stock != null && stock.StockInfo != null)
            {
                total += stock.StockInfo.currentPrice;
            }
        }

        return total;
    }
    #endregion
}

[System.Serializable]
public class NavPoint
{
    public Transform point;
    public float waitTime;
}