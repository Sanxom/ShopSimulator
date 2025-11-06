using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    #region Events
    public static Action OnUIPanelClosed;
    #endregion

    #region Serialized Fields
    [Header("Price Update Panel")]
    [SerializeField] private GameObject _updatePricePanel;
    [SerializeField] private TMP_Text _basePriceText;
    [SerializeField] private TMP_Text _currentPriceText;
    [SerializeField] private TMP_InputField _priceInputField;

    [Header("Buy Menu")]
    [SerializeField] private GameObject _buyMenuPanel;

    [Header("Money Display")]
    [SerializeField] private TMP_Text _moneyText;
    #endregion

    #region Private Fields
    private StockInfo _activeStockInfo;
    #endregion

    #region Properties
    public GameObject UpdatePricePanel => _updatePricePanel;
    public GameObject BuyMenuPanel => _buyMenuPanel;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }

    private void OnEnable()
    {
        SubscribeToEvents();
    }

    private void Update()
    {
        HandleBuyMenuInput();
    }

    private void OnDisable()
    {
        UnsubscribeFromEvents();
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

    private void SubscribeToEvents()
    {
        PlayerController.OnUIPanelClosedWithCancelAction += CloseUpdatePricePanelWithCancelAction;
        PlayerController.OnUIPanelClosedWithCancelAction += CloseBuyMenuPanelWithCancelAction;
    }

    private void UnsubscribeFromEvents()
    {
        PlayerController.OnUIPanelClosedWithCancelAction -= CloseUpdatePricePanelWithCancelAction;
        PlayerController.OnUIPanelClosedWithCancelAction -= CloseBuyMenuPanelWithCancelAction;
    }
    #endregion

    #region Price Update Panel
    public void OnOpenUpdatePricePanel(StockInfo stockToUpdate)
    {
        if (stockToUpdate == null)
        {
            return;
        }

        _updatePricePanel.SetActive(true);
        _activeStockInfo = stockToUpdate;

        UpdatePriceDisplayTexts(stockToUpdate);
        SetInputFieldValue(stockToUpdate.currentPrice);
    }

    private void UpdatePriceDisplayTexts(StockInfo stock)
    {
        if (_basePriceText != null)
        {
            _basePriceText.text = $"{stock.basePrice:0.00}";
        }

        if (_currentPriceText != null)
        {
            _currentPriceText.text = $"{stock.currentPrice:0.00}";
        }
    }

    private void SetInputFieldValue(float price)
    {
        if (_priceInputField != null)
        {
            _priceInputField.text = $"{price:0.00}";
        }
    }

    public void ApplyPriceUpdate()
    {
        if (_activeStockInfo == null)
        {
            return;
        }

        if (float.TryParse(_priceInputField.text, out float newPrice))
        {
            _activeStockInfo.currentPrice = newPrice;
        }

        if (_currentPriceText != null)
        {
            _currentPriceText.text = $"${_activeStockInfo.currentPrice:0.00}";
        }

        if (StockInfoController.Instance != null)
        {
            StockInfoController.Instance.UpdatePrice(_activeStockInfo.Name, _activeStockInfo.currentPrice);
        }

        CloseUpdatePricePanel();
    }

    public void CloseUpdatePricePanel()
    {
        OnUIPanelClosed?.Invoke();

        if (_updatePricePanel != null)
        {
            _updatePricePanel.SetActive(false);
        }
    }

    private void CloseUpdatePricePanelWithCancelAction()
    {
        if (_updatePricePanel != null && _updatePricePanel.activeSelf)
        {
            _updatePricePanel.SetActive(false);
        }
    }
    #endregion

    #region Buy Menu
    public void OpenCloseBuyMenu()
    {
        if (_buyMenuPanel == null || PlayerController.Instance == null)
        {
            return;
        }

        bool isOpen = _buyMenuPanel.activeSelf;

        if (isOpen)
        {
            CloseBuyMenu();
        }
        else
        {
            OpenBuyMenu();
        }
    }

    private void OpenBuyMenu()
    {
        _buyMenuPanel.SetActive(true);
        PlayerController.Instance.DisablePlayerEnableUI();
    }

    private void CloseBuyMenu()
    {
        _buyMenuPanel.SetActive(false);
        PlayerController.Instance.DisableUIEnablePlayer();
    }

    private void CloseBuyMenuPanelWithCancelAction()
    {
        if (_buyMenuPanel != null && _buyMenuPanel.activeSelf)
        {
            CloseBuyMenu();
        }
    }

    private void HandleBuyMenuInput()
    {
        if (Keyboard.current != null && Keyboard.current.tabKey.wasPressedThisFrame)
        {
            OpenCloseBuyMenu();
        }
    }
    #endregion

    #region Money Display
    public void UpdateMoney(float currentMoney)
    {
        if (_moneyText != null)
        {
            _moneyText.text = $"${currentMoney:0.00}";
        }
    }
    #endregion
}

//using masonbell;
//using System;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class UIController : MonoBehaviour
//{
//    public static UIController Instance { get; private set; }

//    #region Event Fields
//    public static Action OnUIPanelClosed;
//    #endregion

//    #region Public Fields
//    public TMP_Text basePriceText;
//    public TMP_Text currentPriceText;
//    public TMP_InputField priceInputField;

//    public TMP_Text moneyText;
//    #endregion

//    #region Serialized Private Fields
//    #endregion

//    #region Private Fields
//    private StockInfo _activeStockInfo;
//    #endregion

//    #region Public Properties
//    [field: SerializeField] public GameObject UpdatePricePanel { get; private set; }
//    [field:SerializeField] public GameObject BuyMenuPanel { get; private set; }
//    #endregion

//    #region Unity Callbacks
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//            Destroy(gameObject);
//        else
//            Instance = this;
//    }

//    private void OnEnable()
//    {
//        PlayerController.OnUIPanelClosedWithCancelAction += CloseUpdatePricePanelWithCancelAction;
//        PlayerController.OnUIPanelClosedWithCancelAction += CloseBuyMenuPanelWithCancelAction;
//    }

//    private void Update()
//    {
//        if (Keyboard.current.tabKey.wasPressedThisFrame)
//        {
//            OpenCloseBuyMenu();
//        }
//    }

//    private void OnDisable()
//    {
//        PlayerController.OnUIPanelClosedWithCancelAction -= CloseUpdatePricePanelWithCancelAction;
//        PlayerController.OnUIPanelClosedWithCancelAction -= CloseBuyMenuPanelWithCancelAction;
//    }
//    #endregion

//    #region Public Methods
//    public void OnOpenUpdatePricePanel(StockInfo stockToUpdate)
//    {
//        UpdatePricePanel.SetActive(true);
//        basePriceText.text = $"{stockToUpdate.basePrice:0.00}";
//        currentPriceText.text = $"{stockToUpdate.currentPrice:0.00}";

//        _activeStockInfo = stockToUpdate;

//        priceInputField.text = $"{stockToUpdate.currentPrice:0.00}";
//    }

//    public void CloseUpdatePricePanelWithCancelAction()
//    {
//        if (UpdatePricePanel.activeSelf)
//            UpdatePricePanel.SetActive(false);
//    }

//    public void CloseBuyMenuPanelWithCancelAction()
//    {
//        if (BuyMenuPanel.activeSelf)
//            BuyMenuPanel.SetActive(false);
//    }

//    public void CloseUpdatePricePanel()
//    {
//        OnUIPanelClosed?.Invoke();
//        UpdatePricePanel.SetActive(false);
//    }

//    public void ApplyPriceUpdate()
//    {
//        if (float.TryParse(priceInputField.text, out float price))
//            _activeStockInfo.currentPrice = price;

//        currentPriceText.text = $"${_activeStockInfo.currentPrice:0.00}";

//        StockInfoController.Instance.UpdatePrice(_activeStockInfo.name, _activeStockInfo.currentPrice);

//        CloseUpdatePricePanel();
//    }

//    public void UpdateMoney(float currentMoney)
//    {
//        moneyText.text = $"${currentMoney:0.00}";
//    }

//    public void OpenCloseBuyMenu()
//    {
//        if (!BuyMenuPanel.activeSelf)
//        {
//            BuyMenuPanel.SetActive(true);
//            PlayerController.Instance.DisablePlayerEnableUI();
//        }
//        else
//        {
//            BuyMenuPanel.SetActive(false);
//            PlayerController.Instance.DisableUIEnablePlayer();
//        }
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}