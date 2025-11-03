using masonbell;
using System;
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
    #endregion

    #region Private Fields
    private const string IS_MOVING_ANIMATOR_NAME = "isMoving";

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

        //navPoints.AddRange(CustomerManager.Instance.GetExitPoints());
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
                    //StartLeaving();
                    currentState = CustomerState.Browsing;
                    _browsePointsRemaining = UnityEngine.Random.Range(1, maxBrowsePoints + 1);
                    _browsePointsRemaining = Mathf.Clamp(_browsePointsRemaining, 1, StoreController.Instance.shelvingCases.Count);
                    GetBrowsePoint();
                }
                    break;
            case CustomerState.Browsing:
                MoveToPoints();

                if (navPoints.Count == 0)
                {
                    if (!_hasGrabbedItem)
                        GrabStock();
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
                            StartLeaving(); // TODO: Change this; testing for now
                        }
                    }
                }
                break;
            case CustomerState.Queueing:
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
        bool isMoving = true;
        currentStandPoint = navPoints[0].point;
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
        shoppingBag.SetActive(true);
        _hasGrabbedItem = true;

        navPoints.Clear();
        navPoints.Add(new NavPoint());

        navPoints[0].point = currentStandPoint;
        navPoints[0].waitTime = waitAfterGrabbing * UnityEngine.Random.Range(0.75f, 1.25f);
        _currentWaitTime = navPoints[0].waitTime;
    }
    #endregion

    #region Private Methods
    private void GetBrowsePoint()
    {
        navPoints.Clear();
        // TODO: Fix this because right now, it is a little messy trying to select between one side of a double-sided shelf or the other
        int selectedShelf = UnityEngine.Random.Range(0, StoreController.Instance.shelvingCases.Count);
        FurnitureController temp = StoreController.Instance.shelvingCases[selectedShelf];
        navPoints.Add(new NavPoint());
        if (!temp.IsDoubleSided)
        {
            navPoints[0].point = temp.customerStandPointFront;
        }
        else
        {
            float tempRandom = UnityEngine.Random.Range(0f, 1f);
            if (tempRandom > 0.5f)
                navPoints[0].point = temp.customerStandPointBack;
            else
                navPoints[0].point = temp.customerStandPointFront;

            currentStandPoint = navPoints[0].point;
            navPoints[0].waitTime = browseTime * UnityEngine.Random.Range(0.75f, 1.25f);
            _currentWaitTime = navPoints[0].waitTime;
            currentShelfCase = temp;
        }
    }
    #endregion
}

[Serializable]
public class NavPoint
{
    public Transform point;
    public float waitTime;
}