using System.Collections.Generic;
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
    [SerializeField] private List<StockObject> _stockInBag = new();
    #endregion

    #region Private Fields
    private const string IS_MOVING_ANIMATOR_PARAMETER = "isMoving";
    private const float ARRIVED_AT_POINT_THRESHOLD = 0.25f;
    private const float QUEUE_POSITION_THRESHOLD = 0.1f;

    private CustomerState _currentState;
    private FurnitureController _currentShelfCase;
    private Transform _currentStandPoint;
    private Vector3 _queuePoint;
    private float _currentWaitTime;
    private int _browsePointsRemaining;
    private bool _hasGrabbedItem;
    #endregion

    #region Properties
    public CustomerState CurrentState => _currentState;
    public float MoveSpeed => _moveSpeed;
    public float BrowseTime => _browseTime;
    public int MaxBrowsePoints => _maxBrowsePoints;
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        _shoppingBag.SetActive(false);
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
                ProcessEntering(deltaTime);
                break;
            case CustomerState.Browsing:
                ProcessBrowsing(deltaTime);
                break;
            case CustomerState.Queueing:
                ProcessQueueing(deltaTime);
                break;
            case CustomerState.AtCheckout:
                break;
            case CustomerState.Leaving:
                ProcessLeaving();
                break;
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
        }
    }
    #endregion

    #region State Processing
    private void ProcessEntering(float deltaTime)
    {
        if (_navPoints.Count > 0)
        {
            MoveToPoints(deltaTime);
        }
        else
        {
            TransitionToBrowsing();
        }
    }

    private void ProcessBrowsing(float deltaTime)
    {
        MoveToPoints(deltaTime);

        if (_navPoints.Count == 0)
        {
            if (!_hasGrabbedItem)
            {
                GrabStock();
            }
            else
            {
                CompleteBrowsePoint();
            }
        }
    }

    private void ProcessQueueing(float deltaTime)
    {
        MoveTowardsQueue(deltaTime);
    }

    private void ProcessLeaving()
    {
        if (_navPoints.Count > 0)
        {
            MoveToPoints(Time.deltaTime);
        }
        else
        {
            ObjectPool<Customer>.ReturnToPool(this);
        }
    }
    #endregion

    #region Movement
    private void MoveToPoints(float deltaTime)
    {
        if (_navPoints.Count == 0)
        {
            StartNextPoint();
            return;
        }

        Vector3 targetPosition = new(
            _navPoints[0].point.position.x,
            transform.position.y,
            _navPoints[0].point.position.z
        );

        transform.position = Vector3.MoveTowards(transform.position, targetPosition, _moveSpeed * deltaTime);
        transform.LookAt(targetPosition);

        bool isMoving = Vector3.Distance(transform.position, targetPosition) >= ARRIVED_AT_POINT_THRESHOLD;

        if (!isMoving)
        {
            _currentWaitTime -= deltaTime;
            if (_currentWaitTime <= 0f)
            {
                StartNextPoint();
            }
        }

        _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, isMoving);
    }

    private void MoveTowardsQueue(float deltaTime)
    {
        transform.position = Vector3.MoveTowards(transform.position, _queuePoint, _moveSpeed * deltaTime);

        bool isMoving = Vector3.Distance(transform.position, _queuePoint) > QUEUE_POSITION_THRESHOLD;

        if (isMoving)
        {
            transform.LookAt(_queuePoint);
        }

        _animator.SetBool(IS_MOVING_ANIMATOR_PARAMETER, isMoving);
    }

    public void StartNextPoint()
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

    #region Browsing
    private void TransitionToBrowsing()
    {
        _currentState = CustomerState.Browsing;
        _browsePointsRemaining = Random.Range(1, _maxBrowsePoints + 1);
        _browsePointsRemaining = Mathf.Clamp(_browsePointsRemaining, 1, StoreController.Instance.ShelvingCases.Count);
        GetBrowsePoint();
    }

    private void GetBrowsePoint()
    {
        _navPoints.Clear();

        if (StoreController.Instance.ShelvingCases.Count == 0 || StoreController.Instance.ShelvingCases.Count == 1 && StoreController.Instance.ShelvingCases[0].IsHeld)
        {
            StartLeaving();
            return;
        }

        int selectedShelf = Random.Range(0, StoreController.Instance.ShelvingCases.Count);
        _currentShelfCase = StoreController.Instance.ShelvingCases[selectedShelf];
        if (_currentShelfCase.IsHeld)
        {
            CompleteBrowsePoint();
        }

        NavPoint browsePoint = new()
        {
            point = _currentShelfCase.CustomerStandPoint,
            waitTime = _browseTime * Random.Range(0.75f, 1.25f)
        };

        _navPoints.Add(browsePoint);
        _currentWaitTime = browsePoint.waitTime;
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
    }

    private void PlaceStockInBag(StockObject stock)
    {
        _shoppingBag.SetActive(true);
        stock.transform.SetParent(_shoppingBag.transform);
        _stockInBag.Add(stock);
        stock.PlaceInBag(_shoppingBag.transform);
    }

    private void SetupPostGrabWait()
    {
        _navPoints.Clear();

        NavPoint waitPoint = new NavPoint
        {
            point = _currentShelfCase.CustomerStandPoint,
            waitTime = _waitAfterGrabbing * Random.Range(0.75f, 1.25f)
        };

        _navPoints.Add(waitPoint);
        _currentWaitTime = waitPoint.waitTime;
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
            Checkout.Instance.AddCustomerToQueue(this);
            _currentState = CustomerState.Queueing;
        }
        else
        {
            StartLeaving();
        }
    }
    #endregion

    #region Public Methods
    public void StartLeaving()
    {
        _currentState = CustomerState.Leaving;
        _navPoints.Clear();
        _navPoints.AddRange(CustomerManager.Instance.GetExitPoints());
    }

    public void UpdateQueuePoint(Vector3 newPoint)
    {
        _queuePoint = newPoint;
        transform.LookAt(_queuePoint);
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

//using masonbell;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;

//public class Customer : MonoBehaviour
//{
//    public enum CustomerState
//    {
//        Entering,
//        Browsing,
//        Queueing,
//        AtCheckout,
//        Leaving
//    }

//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public List<NavPoint> navPoints = new();
//    public FurnitureController currentShelfCase;
//    public Transform currentStandPoint;
//    public GameObject shoppingBag;
//    public CustomerState currentState;  
//    public float moveSpeed;
//    public float browseTime;
//    public float waitAfterGrabbing = 0.5f;
//    public int maxBrowsePoints = 5;
//    #endregion

//    #region Serialized Private Fields
//    [SerializeField] private List<StockObject> stockInBag = new();
//    #endregion

//    #region Private Fields
//    private const string IS_MOVING_ANIMATOR_NAME = "isMoving";

//    private Vector3 queuePoint;
//    private float _currentWaitTime = 0f;
//    private int _browsePointsRemaining;
//    private bool _hasGrabbedItem;
//    #endregion

//    #region Public Properties
//    [field: SerializeField] public Animator Anim { get; private set; }
//    #endregion

//    #region Unity Callbacks
//    private void OnEnable()
//    {
//        shoppingBag.SetActive(false);
//    }

//    private void Start()
//    {
//        navPoints.Clear();
//        navPoints.AddRange(CustomerManager.Instance.GetEntryPoints());
//        if (navPoints.Count > 0)
//        {
//            transform.position = navPoints[0].point.position;
//            _currentWaitTime = navPoints[0].waitTime;
//        }
//    }

//    private void Update()
//    {
//        switch (currentState)
//        {
//            case CustomerState.Entering:
//                if (navPoints.Count > 0)
//                {
//                    MoveToPoints();
//                }
//                else
//                {
//                    currentState = CustomerState.Browsing;
//                    _browsePointsRemaining = Random.Range(1, maxBrowsePoints + 1);
//                    _browsePointsRemaining = Mathf.Clamp(_browsePointsRemaining, 1, StoreController.Instance.shelvingCases.Count);
//                    GetBrowsePoint();
//                }
//                    break;
//            case CustomerState.Browsing:
//                MoveToPoints();

//                if (navPoints.Count == 0)
//                {
//                    if (!_hasGrabbedItem)
//                    {
//                        GrabStock();
//                    }
//                    else
//                    {
//                        _hasGrabbedItem = false;
//                        _browsePointsRemaining--;
//                        if (_browsePointsRemaining > 0)
//                        {
//                            GetBrowsePoint();
//                        }
//                        else
//                        {
//                            if (stockInBag.Count > 0)
//                            {
//                                Checkout.Instance.AddCustomerToQueue(this);
//                                currentState = CustomerState.Queueing;
//                            }
//                            else
//                            {
//                                StartLeaving();
//                            }
//                        }
//                    }
//                }
//                break;
//            case CustomerState.Queueing:
//                transform.position = Vector3.MoveTowards(transform.position, queuePoint, moveSpeed * Time.deltaTime);
//                if (Vector3.Distance(transform.position, queuePoint) > 0.1f)
//                {
//                    Anim.SetBool(IS_MOVING_ANIMATOR_NAME, true);
//                }
//                else
//                {
//                    Anim.SetBool(IS_MOVING_ANIMATOR_NAME, false);
//                }
//                    break;
//            case CustomerState.AtCheckout:
//                break;
//            case CustomerState.Leaving:
//                if (navPoints.Count > 0)
//                {
//                    MoveToPoints();
//                }
//                else
//                {
//                    ObjectPool<Customer>.ReturnToPool(this);
//                }
//                break;
//            default:
//                break;
//        }
//    }
//    #endregion

//    #region Public Methods
//    public void MoveToPoints()
//    {
//        if (navPoints.Count > 0)
//        {
//            bool isMoving = true;
//            Vector3 targetPosition = new(navPoints[0].point.position.x, transform.position.y, navPoints[0].point.position.z);
//            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
//            transform.LookAt(targetPosition);

//            if (Vector3.Distance(transform.position, targetPosition) < 0.25f)
//            {
//                isMoving = false;

//                _currentWaitTime -= Time.deltaTime;

//                if (_currentWaitTime <= 0)
//                {
//                    StartNextPoint();
//                }
//            }
//            Anim.SetBool(IS_MOVING_ANIMATOR_NAME, isMoving);
//        }
//        else
//        {
//            StartNextPoint();
//        }
//    }

//    public void StartNextPoint()
//    {
//        if (navPoints.Count > 0)
//        {
//            navPoints.RemoveAt(0);

//            if (navPoints.Count > 0)
//            {
//                _currentWaitTime = navPoints[0].waitTime;
//            }
//        }
//    }

//    public void StartLeaving()
//    {
//        currentState = CustomerState.Leaving;

//        navPoints.Clear();

//        navPoints.AddRange(CustomerManager.Instance.GetExitPoints());
//    }

//    public void GrabStock()
//    {
//        _hasGrabbedItem = true;

//        int shelf = Random.Range(0, currentShelfCase.shelves.Count);
//        StockObject stock = currentShelfCase.shelves[shelf].GetStock();

//        if (stock != null)
//        {
//            stock.transform.SetParent(shoppingBag.transform);
//            stockInBag.Add(stock);
//            stock.PlaceInBag(shoppingBag.transform);
//            shoppingBag.SetActive(true);
//            navPoints.Clear();
//            navPoints.Add(new NavPoint());
//            navPoints[0].point = currentShelfCase.customerStandPoint;
//            navPoints[0].waitTime = waitAfterGrabbing * Random.Range(0.75f, 1.25f);
//            _currentWaitTime = navPoints[0].waitTime;
//        }
//    }

//    public void UpdateQueuePoint(Vector3 newPoint)
//    {
//        queuePoint = newPoint;
//        transform.LookAt(queuePoint);
//    }

//    public float GetTotalSpendAmount()
//    {
//        float total = 0f;

//        foreach (StockObject stock in stockInBag)
//        {
//            total += stock.Info.currentPrice;
//        }

//        return total;
//    }
//    #endregion

//    #region Private Methods
//    private void GetBrowsePoint()
//    {
//        navPoints.Clear();
//        int selectedShelf = Random.Range(0, StoreController.Instance.shelvingCases.Count);
//        currentShelfCase = StoreController.Instance.shelvingCases[selectedShelf];
//        navPoints.Add(new NavPoint());
//        navPoints[0].point = currentShelfCase.customerStandPoint;

//        navPoints[0].waitTime = browseTime * Random.Range(0.75f, 1.25f);
//        _currentWaitTime = navPoints[0].waitTime;
//    }
//    #endregion
//}

//[System.Serializable]
//public class NavPoint
//{
//    public Transform point;
//    public float waitTime;
//}