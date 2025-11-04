using masonbell;
using System.Collections.Generic;
using System.Linq;
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

    #region Event Fields
    #endregion

    #region Public Fields
    public List<NavPoint> navPoints = new();
    public FurnitureController currentShelfCase;
    public Transform currentStandPoint;
    public GameObject shoppingBag;
    public CustomerState currentState;  
    public float moveSpeed;
    public float browseTime;
    public float waitAfterGrabbing = 0.5f;
    public int maxBrowsePoints = 5;
    #endregion

    #region Serialized Private Fields
    [SerializeField] private List<StockObject> stockInBag = new();
    #endregion

    #region Private Fields
    private const string IS_MOVING_ANIMATOR_NAME = "isMoving";

    private Vector3 queuePoint;
    private float _currentWaitTime = 0f;
    private int _browsePointsRemaining;
    private bool _hasGrabbedItem;
    #endregion

    #region Public Properties
    [field: SerializeField] public Animator Anim { get; private set; }
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        shoppingBag.SetActive(false);
    }

    private void Start()
    {
        navPoints.Clear();
        navPoints.AddRange(CustomerManager.Instance.GetEntryPoints());
        if (navPoints.Count > 0)
        {
            transform.position = navPoints[0].point.position;
            _currentWaitTime = navPoints[0].waitTime;
        }
    }

    private void Update()
    {
        switch (currentState)
        {
            case CustomerState.Entering:
                if (navPoints.Count > 0)
                {
                    MoveToPoints();
                }
                else
                {
                    currentState = CustomerState.Browsing;
                    _browsePointsRemaining = Random.Range(1, maxBrowsePoints + 1);
                    _browsePointsRemaining = Mathf.Clamp(_browsePointsRemaining, 1, StoreController.Instance.shelvingCases.Count);
                    GetBrowsePoint();
                }
                    break;
            case CustomerState.Browsing:
                MoveToPoints();

                if (navPoints.Count == 0)
                {
                    if (!_hasGrabbedItem)
                    {
                        GrabStock();
                    }
                    else
                    {
                        _hasGrabbedItem = false;
                        _browsePointsRemaining--;
                        if (_browsePointsRemaining > 0)
                        {
                            GetBrowsePoint();
                        }
                        else
                        {
                            if (stockInBag.Count > 0)
                            {
                                Checkout.Instance.AddCustomerToQueue(this);
                                currentState = CustomerState.Queueing;
                            }
                            else
                            {
                                StartLeaving();
                            }
                        }
                    }
                }
                break;
            case CustomerState.Queueing:
                transform.position = Vector3.MoveTowards(transform.position, queuePoint, moveSpeed * Time.deltaTime);
                if (Vector3.Distance(transform.position, queuePoint) > 0.1f)
                {
                    Anim.SetBool(IS_MOVING_ANIMATOR_NAME, true);
                }
                else
                {
                    Anim.SetBool(IS_MOVING_ANIMATOR_NAME, false);
                }
                    break;
            case CustomerState.AtCheckout:
                break;
            case CustomerState.Leaving:
                if (navPoints.Count > 0)
                {
                    MoveToPoints();
                }
                else
                {
                    ObjectPool<Customer>.ReturnToPool(this);
                }
                break;
            default:
                break;
        }
    }
    #endregion

    #region Public Methods
    public void MoveToPoints()
    {
        if (navPoints.Count > 0)
        {
            bool isMoving = true;
            Vector3 targetPosition = new(navPoints[0].point.position.x, transform.position.y, navPoints[0].point.position.z);
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            transform.LookAt(targetPosition);

            if (Vector3.Distance(transform.position, targetPosition) < 0.25f)
            {
                isMoving = false;

                _currentWaitTime -= Time.deltaTime;

                if (_currentWaitTime <= 0)
                {
                    StartNextPoint();
                }
            }
            Anim.SetBool(IS_MOVING_ANIMATOR_NAME, isMoving);
        }
        else
        {
            StartNextPoint();
        }
    }

    public void StartNextPoint()
    {
        if (navPoints.Count > 0)
        {
            navPoints.RemoveAt(0);

            if (navPoints.Count > 0)
            {
                _currentWaitTime = navPoints[0].waitTime;
            }
        }
    }

    public void StartLeaving()
    {
        currentState = CustomerState.Leaving;

        navPoints.Clear();

        navPoints.AddRange(CustomerManager.Instance.GetExitPoints());
    }

    public void GrabStock()
    {
        _hasGrabbedItem = true;

        int shelf = Random.Range(0, currentShelfCase.shelves.Count);
        StockObject stock = currentShelfCase.shelves[shelf].GetStock();

        if (stock != null)
        {
            stock.transform.SetParent(shoppingBag.transform);
            stockInBag.Add(stock);
            stock.PlaceInBag(shoppingBag.transform);
            shoppingBag.SetActive(true);
            navPoints.Clear();
            navPoints.Add(new NavPoint());
            navPoints[0].point = currentShelfCase.customerStandPoint;
            navPoints[0].waitTime = waitAfterGrabbing * Random.Range(0.75f, 1.25f);
            _currentWaitTime = navPoints[0].waitTime;
        }
    }

    public void UpdateQueuePoint(Vector3 newPoint)
    {
        queuePoint = newPoint;
        transform.LookAt(queuePoint);
    }

    public float GetTotalSpendAmount()
    {
        float total = 0f;

        foreach (StockObject stock in stockInBag)
        {
            total += stock.Info.currentPrice;
        }

        return total;
    }
    #endregion

    #region Private Methods
    private void GetBrowsePoint()
    {
        navPoints.Clear();
        int selectedShelf = Random.Range(0, StoreController.Instance.shelvingCases.Count);
        currentShelfCase = StoreController.Instance.shelvingCases[selectedShelf];
        navPoints.Add(new NavPoint());
        navPoints[0].point = currentShelfCase.customerStandPoint;

        navPoints[0].waitTime = browseTime * Random.Range(0.75f, 1.25f);
        _currentWaitTime = navPoints[0].waitTime;
    }
    #endregion
}

[System.Serializable]
public class NavPoint
{
    public Transform point;
    public float waitTime;
}