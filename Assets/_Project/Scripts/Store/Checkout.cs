using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Checkout : MonoBehaviour, IInteractable
{
    public static Checkout Instance { get; private set; }

    #region Serialized Fields
    [Header("UI References")]
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private GameObject _checkoutScreen;

    [Header("Queue Settings")]
    [SerializeField] private Transform _queuePoint;
    [SerializeField] private float _customerQueueOffset = 0.6f;

    [Header("Queue State")]
    [SerializeField] private List<Customer> _customersInQueue = new();
    #endregion

    #region Private Fields
    private const float CUSTOMER_ARRIVAL_THRESHOLD = 0.2f;
    #endregion

    #region Properties
    public GameObject MyObject { get; set; }
    public List<Customer> CustomersInQueue => _customersInQueue;

    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        MyObject = gameObject;
        InitializeSingleton();
    }

    private void Start()
    {
        HidePrice();
    }

    private void Update()
    {
        ProcessQueuedCustomers();
    }
    #endregion

    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    #endregion

    #region Queue Management
    public void AddCustomerToQueue(Customer customer)
    {
        if (customer == null)
        {
            return;
        }

        _customersInQueue.Add(customer);
        UpdateQueue();
    }

    public void UpdateQueue()
    {
        for (int i = 0; i < _customersInQueue.Count; i++)
        {
            Vector3 queuePosition = _queuePoint.position + (_customerQueueOffset * i * _queuePoint.forward);
            _customersInQueue[i].UpdateQueuePoint(queuePosition);
        }
    }

    private void ProcessQueuedCustomers()
    {
        if (_customersInQueue.Count == 0 || _checkoutScreen.activeSelf)
        {
            return;
        }

        Customer firstCustomer = _customersInQueue[0];
        float distanceToQueue = Vector3.Distance(firstCustomer.transform.position, _queuePoint.position);

        if (distanceToQueue <= CUSTOMER_ARRIVAL_THRESHOLD)
        {
            firstCustomer.transform.LookAt(transform);
            ShowPrice(firstCustomer.GetTotalSpendAmount());
        }
    }
    #endregion

    #region Checkout Process
    public void CheckoutCustomer()
    {
        if (!_checkoutScreen.activeSelf || _customersInQueue.Count == 0)
        {
            return;
        }

        Customer customer = _customersInQueue[0];
        float totalSpent = customer.GetTotalSpendAmount();

        HidePrice();
        StoreController.Instance.AddMoney(totalSpent);

        customer.StartLeaving();
        _customersInQueue.RemoveAt(0);

        UpdateQueue();
    }
    #endregion

    #region Price Display
    private void ShowPrice(float priceTotal)
    {
        if (_checkoutScreen != null)
        {
            _checkoutScreen.SetActive(true);
        }

        if (_priceText != null)
        {
            _priceText.text = $"${priceTotal:0.00}";
        }
    }

    private void HidePrice()
    {
        if (_checkoutScreen != null)
        {
            _checkoutScreen.SetActive(false);
        }
    }

    public void OnInteract(Transform holdPoint = null)
    {
        CheckoutCustomer();
    }

    public string GetInteractionPrompt()
    {
        return $"Checkout Customer";
    }
    #endregion
}

//using System;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class Checkout : MonoBehaviour
//{
//    public static Checkout Instance { get; private set; } // TODO: We could either remove this to make more Checkout counters or create a List of counters later

//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public TMP_Text priceText;
//    public GameObject checkoutScreen;
//    public Transform queuePoint;

//    public List<Customer> customersInQueue = new();
//    #endregion

//    #region Serialized Private Fields
//    [SerializeField] private float _customerQueueOffset = 0.6f;
//    #endregion

//    #region Private Fields
//    #endregion

//    #region Public Properties
//    #endregion

//    #region Unity Callbacks
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//        }
//        else
//        {
//            Instance = this;
//        }
//    }

//    private void Start()
//    {
//        HidePrice();
//    }

//    private void Update()
//    {
//        if (customersInQueue.Count > 0 && !checkoutScreen.activeSelf)
//        {

//            if (Vector3.Distance(customersInQueue[0].transform.position, queuePoint.position) <= 0.2f)
//            {
//                customersInQueue[0].transform.LookAt(transform);
//                ShowPrice(customersInQueue[0].GetTotalSpendAmount());
//            }
//        }
//    }
//    #endregion

//    #region Public Methods
//    public void ShowPrice(float priceTotal)
//    {
//        checkoutScreen.SetActive(true);
//        priceText.text = $"${priceTotal:0.00}";
//    }

//    public void HidePrice()
//    {
//        checkoutScreen.SetActive(false);
//    }

//    public void CheckoutCustomer()
//    {
//        if (checkoutScreen.activeSelf && customersInQueue.Count > 0)
//        {
//            HidePrice();
//            StoreController.Instance.AddMoney(customersInQueue[0].GetTotalSpendAmount());
//            customersInQueue[0].StartLeaving();
//            customersInQueue.RemoveAt(0);
//            UpdateQueue();
//        }
//    }

//    public void AddCustomerToQueue(Customer customer)
//    {
//        customersInQueue.Add(customer);

//        UpdateQueue();
//    }

//    public void UpdateQueue()
//    {
//        for (int i = 0; i < customersInQueue.Count; i++)
//        {
//            customersInQueue[i].UpdateQueuePoint(queuePoint.position + (queuePoint.forward * i * _customerQueueOffset));
//        }
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}