using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class Checkout : InteractableObject
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

    [Header("Checkout Stock Positions")]
    [SerializeField] private List<Transform> _checkoutStockPositions;
    [SerializeField] private List<StockObject> _stockObjectsOnCounter;
    [SerializeField] private Transform _shoppingBagPlacementPoint;
    #endregion

    #region Private Fields
    private WaitForEndOfFrame _waitForEndOfFrame;
    private float _totalCurrentPriceAmount = 0f;
    #endregion

    #region Properties
    [Header("Card Position")]
    [field: SerializeField] public Transform CardMoveToPoint { get; private set; }

    public List<Customer> CustomersInQueue => _customersInQueue;
    public List<Transform> CheckoutStockPositions { get => _checkoutStockPositions; private set => _checkoutStockPositions = value; }
    public List<StockObject> StockObjectsOnCounter { get => _stockObjectsOnCounter; set => _stockObjectsOnCounter = value; }
    public Transform ShoppingBagPlacementPoint { get => _shoppingBagPlacementPoint; private set => _shoppingBagPlacementPoint = value; }

    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        InitializeSingleton();
        _waitForEndOfFrame = new WaitForEndOfFrame();
    }

    private void Start() => HideCheckoutScreen();
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
        if (customer == null) return;

        _customersInQueue.Add(customer);
        UpdateQueue();
    }

    public void UpdateQueue()
    {
        for (int i = 0; i < _customersInQueue.Count; i++)
        {
            Vector3 queuePosition = _queuePoint.position + (_customerQueueOffset * i * _queuePoint.forward);
            Vector3 nextQueuePosition = i > 0 && i <= _customersInQueue.Count
                ? _queuePoint.position + (_customerQueueOffset * (i - 1) * _queuePoint.forward)
                : queuePosition;
            if (_customersInQueue[0].transform.position != _queuePoint.position)
            {
                Tween.Position(_customersInQueue[0].transform, _queuePoint.position, 0.1f, Ease.Linear);
            }
            _customersInQueue[i].UpdateQueuePoint(queuePosition, nextQueuePosition);
        }
    }
    #endregion

    #region Checkout Process
    public void CheckoutCustomer()
    {
        if (!_checkoutScreen.activeSelf || _customersInQueue.Count == 0) return;

        Customer customer = _customersInQueue[0];

        HideCheckoutScreen();
        StoreController.Instance.AddMoney(_totalCurrentPriceAmount);
        _totalCurrentPriceAmount = 0;
        customer.ShoppingBag.transform.SetParent(customer.ShoppingBagDefaultParent);
        Tween.LocalPosition(customer.ShoppingBag.transform, customer.ShoppingBagDefaultTransform.localPosition, 0.1f, Ease.Linear);
        Tween.LocalRotation(customer.ShoppingBag.transform, customer.ShoppingBagDefaultTransform.localRotation, 0.1f, Ease.Linear);
        Tween.Scale(customer.ShoppingBag.transform, customer.ShoppingBagDefaultTransform.localScale, 0.1f, Ease.Linear);
        customer.TransitionToLeaving();
        _customersInQueue.RemoveAt(0);

        UpdateQueue();
    }

    private IEnumerator CheckoutCustomerCoroutine()
    {
        if (!_checkoutScreen.activeSelf || _customersInQueue.Count == 0) yield break;

        Customer customer = _customersInQueue[0];
        customer.TransitionToPaymentOption();
        while (customer.IsPaying)
        {
            yield return _waitForEndOfFrame;
            print("Waiting...");
        }
        // TODO:
        // Customer chooses to pay with cash or card
        // Take Cash/Card and do appropriate functions
        // Transition to Change-giving if paid with Cash and press Enter to give change (can give wrong amount) and Checkout Customer
        // Or Transition to Card swipe/amount entering (can enter wrong amount) and press Enter to Checkout Customer
        // Change will not be infinite like other games, you have to go to the bank and get change (Or have it delivered with an upgrade?)
        HideCheckoutScreen();
        StoreController.Instance.AddMoney(_totalCurrentPriceAmount);
        _totalCurrentPriceAmount = 0;
        _priceText.text = $"${_totalCurrentPriceAmount:0.00}";
        customer.ShoppingBag.transform.SetParent(customer.ShoppingBagDefaultParent);
        yield return new WaitForSeconds(0.5f);
        yield return Tween.LocalPosition(customer.ShoppingBag.transform, Vector3.zero, 0.1f, Ease.Linear).ToYieldInstruction();
        yield return Tween.Rotation(customer.ShoppingBag.transform, Quaternion.identity, 0.1f, Ease.Linear).ToYieldInstruction();
        yield return Tween.Scale(customer.ShoppingBag.transform, Vector3.zero, 0.1f, Ease.Linear).ToYieldInstruction();
        customer.TransitionToLeaving();
        _customersInQueue.RemoveAt(0);

        UpdateQueue();
    }
    #endregion

    #region Price Display
    public void AddToTotalPrice(float priceToAdd)
    {
        _checkoutScreen.SetActive(true);
        _priceText.text = $"${(_totalCurrentPriceAmount += priceToAdd):0.00}";
        _stockObjectsOnCounter.RemoveAt(0);
        if (_stockObjectsOnCounter.Count == 0)
        {
            StartCoroutine(CheckoutCustomerCoroutine());
        }
    }

    public void ShowCheckoutScreen()
    {
        _checkoutScreen.SetActive(true);
    }

    private void ShowPrice(float priceTotal)
    {
        if (_checkoutScreen != null)
            _checkoutScreen.SetActive(true);

        if (_priceText != null)
            _priceText.text = $"${priceTotal:0.00}";
    }

    private void HideCheckoutScreen()
    {
        if (_checkoutScreen != null)
            _checkoutScreen.SetActive(false);
    }

    public override string GetInteractionPrompt(PlayerInteraction player)
    {
        return base.GetInteractionPrompt(player);
    }
    #endregion
}