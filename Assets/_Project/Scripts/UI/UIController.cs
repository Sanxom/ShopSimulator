using masonbell;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIController : MonoBehaviour
{
    public static UIController Instance { get; private set; }

    #region Event Fields
    public static Action OnUIPanelClosed;
    #endregion

    #region Public Fields
    public TMP_Text basePriceText;
    public TMP_Text currentPriceText;
    public TMP_InputField priceInputField;

    public TMP_Text moneyText;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    private StockInfo _activeStockInfo;
    #endregion

    #region Public Properties
    [field: SerializeField] public GameObject UpdatePricePanel { get; private set; }
    [field:SerializeField] public GameObject BuyMenuPanel { get; private set; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void OnEnable()
    {
        PlayerController.OnUIPanelClosedWithCancelAction += CloseUpdatePricePanelWithCancelAction;
        PlayerController.OnUIPanelClosedWithCancelAction += CloseBuyMenuPanelWithCancelAction;
    }

    private void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            OpenCloseBuyMenu();
        }
    }

    private void OnDisable()
    {
        PlayerController.OnUIPanelClosedWithCancelAction -= CloseUpdatePricePanelWithCancelAction;
        PlayerController.OnUIPanelClosedWithCancelAction -= CloseBuyMenuPanelWithCancelAction;
    }
    #endregion

    #region Public Methods
    public void OnOpenUpdatePricePanel(StockInfo stockToUpdate)
    {
        UpdatePricePanel.SetActive(true);
        basePriceText.text = $"{stockToUpdate.basePrice:0.00}";
        currentPriceText.text = $"{stockToUpdate.currentPrice:0.00}";

        _activeStockInfo = stockToUpdate;

        priceInputField.text = $"{stockToUpdate.currentPrice:0.00}";
    }

    public void CloseUpdatePricePanelWithCancelAction()
    {
        if (UpdatePricePanel.activeSelf)
            UpdatePricePanel.SetActive(false);
    }

    public void CloseBuyMenuPanelWithCancelAction()
    {
        if (BuyMenuPanel.activeSelf)
            BuyMenuPanel.SetActive(false);
    }

    public void CloseUpdatePricePanel()
    {
        OnUIPanelClosed?.Invoke();
        UpdatePricePanel.SetActive(false);
    }

    public void ApplyPriceUpdate()
    {
        if (float.TryParse(priceInputField.text, out float price))
            _activeStockInfo.currentPrice = price;

        currentPriceText.text = $"${_activeStockInfo.currentPrice:0.00}";

        StockInfoController.Instance.UpdatePrice(_activeStockInfo.name, _activeStockInfo.currentPrice);

        CloseUpdatePricePanel();
    }

    public void UpdateMoney(float currentMoney)
    {
        moneyText.text = $"${currentMoney:0.00}";
    }

    public void OpenCloseBuyMenu()
    {
        if (!BuyMenuPanel.activeSelf)
        {
            BuyMenuPanel.SetActive(true);
            PlayerController.Instance.DisablePlayerEnableUI();
        }
        else
        {
            BuyMenuPanel.SetActive(false);
            PlayerController.Instance.DisableUIEnablePlayer();
        }
    }
    #endregion

    #region Private Methods
    #endregion
}