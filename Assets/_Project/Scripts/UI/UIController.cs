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
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    private StockInfo _activeStockInfo;
    #endregion

    #region Public Properties
    [field: SerializeField] public GameObject UpdatePricePanel { get; private set; }
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
    }

    private void OnDisable()
    {
        PlayerController.OnUIPanelClosedWithCancelAction -= CloseUpdatePricePanelWithCancelAction;
    }
    #endregion

    #region Public Methods
    public void OnOpenUpdatePricePanel(StockInfo stockToUpdate)
    {
        UpdatePricePanel.SetActive(true);
        basePriceText.text = stockToUpdate.basePrice.ToString("F2");
        currentPriceText.text = stockToUpdate.currentPrice.ToString("F2");

        _activeStockInfo = stockToUpdate;

        priceInputField.text = stockToUpdate.currentPrice.ToString("F2");
    }

    public void CloseUpdatePricePanelWithCancelAction()
    {
        UpdatePricePanel.SetActive(false);
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

        currentPriceText.text = "$" + _activeStockInfo.currentPrice.ToString("F2");

        StockInfoController.Instance.UpdatePrice(_activeStockInfo.name, _activeStockInfo.currentPrice);

        CloseUpdatePricePanel();
    }
    #endregion

    #region Private Methods
    #endregion
}