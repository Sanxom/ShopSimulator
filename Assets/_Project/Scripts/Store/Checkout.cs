using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Checkout : MonoBehaviour
{
    public static Checkout Instance { get; private set; } // TODO: We could either remove this to make more Checkout counters or create a List of counters later

    #region Event Fields
    #endregion

    #region Public Fields
    public TMP_Text priceText;
    public GameObject checkoutScreen;
    public Transform queuePoint;

    public List<Customer> customersInQueue = new();
    #endregion

    #region Serialized Private Fields
    [SerializeField] private float _customerQueueOffset = 0.6f;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        HidePrice();
    }

    private void Update()
    {
        if (customersInQueue.Count > 0 && !checkoutScreen.activeSelf)
        {

            if (Vector3.Distance(customersInQueue[0].transform.position, queuePoint.position) <= 0.2f)
            {
                customersInQueue[0].transform.LookAt(transform);
                ShowPrice(customersInQueue[0].GetTotalSpendAmount());
            }
        }
    }
    #endregion

    #region Public Methods
    public void ShowPrice(float priceTotal)
    {
        print("He's here.");

        checkoutScreen.SetActive(true);
        priceText.text = $"${priceTotal:0.00}";
    }

    public void HidePrice()
    {
        checkoutScreen.SetActive(false);
    }

    public void CheckoutCustomer()
    {
        if (checkoutScreen.activeSelf && customersInQueue.Count > 0)
        {
            HidePrice();
            StoreController.Instance.AddMoney(customersInQueue[0].GetTotalSpendAmount());
            customersInQueue[0].StartLeaving();
            customersInQueue.RemoveAt(0);
            UpdateQueue();
        }
    }

    public void AddCustomerToQueue(Customer customer)
    {
        customersInQueue.Add(customer);

        UpdateQueue();
    }

    public void UpdateQueue()
    {
        for (int i = 0; i < customersInQueue.Count; i++)
        {
            customersInQueue[i].UpdateQueuePoint(queuePoint.position + (queuePoint.forward * i * _customerQueueOffset));
        }
    }
    #endregion

    #region Private Methods
    #endregion
}