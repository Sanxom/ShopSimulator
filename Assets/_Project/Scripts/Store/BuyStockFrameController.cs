using TMPro;
using UnityEngine;

public class BuyStockFrameController : MonoBehaviour
{
    #region Serialized Fields
    [Header("Stock Information")]
    [SerializeField] private StockInfo _stockInfo;

    [Header("UI References")]
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _priceText;
    [SerializeField] private TMP_Text _amountInBoxText;
    [SerializeField] private TMP_Text _boxPriceText;
    [SerializeField] private TMP_Text _buttonText;

    [Header("Box Prefab")]
    [SerializeField] private StockBoxController _boxToSpawn;
    #endregion

    #region Private Fields
    private float _boxCost;
    #endregion

    #region Properties
    public StockInfo StockInfo => _stockInfo;
    #endregion

    #region Unity Lifecycle
    private void Start() => UpdateFrameInfo();
    #endregion

    #region Public Methods
    public void UpdateFrameInfo()
    {
        RefreshStockInfo();
        UpdateDisplayTexts();
        CalculateBoxCost();
    }

    public void BuyBox()
    {
        if (!CanAffordBox()) return;

        PurchaseBox();
        SpawnBox();
    }
    #endregion

    #region Private Methods
    private void RefreshStockInfo()
    {
        if (StockInfoController.Instance != null)
            _stockInfo = StockInfoController.Instance.GetStockInfo(_stockInfo.Name);
    }

    private void UpdateDisplayTexts()
    {
        if (_stockInfo == null) return;

        if (_nameText != null) 
            _nameText.text = _stockInfo.Name;

        if (_priceText != null) 
            _priceText.text = $"${_stockInfo.currentPrice:0.00}";
    }

    private void CalculateBoxCost()
    {
        if (_boxToSpawn == null || _stockInfo == null) return;

        int boxAmount = _boxToSpawn.GetMaxPossibleStockAmount(_stockInfo.typeOfStock);
        _boxCost = boxAmount * _stockInfo.currentPrice;

        if (_amountInBoxText != null) 
            _amountInBoxText.text = $"{boxAmount} per box";

        if (_boxPriceText != null) 
            _boxPriceText.text = $"Box: ${_boxCost:0.00}";

        if (_buttonText != null) 
            _buttonText.text = $"PAY: ${_boxCost:0.00}";
    }

    private bool CanAffordBox() => StoreController.Instance != null && StoreController.Instance.CheckMoneyAvailable(_boxCost);

    private void PurchaseBox()
    {
        if (StoreController.Instance != null)
            StoreController.Instance.SpendMoney(_boxCost);
    }

    private void SpawnBox()
    {
        if (_boxToSpawn == null || StoreController.Instance == null || _stockInfo == null) return;

        Vector3 spawnPosition = StoreController.Instance.StockSpawnPoint.position;
        StockBoxController spawnedBox = ObjectPool<StockBoxController>.GetFromPool(
            _boxToSpawn,
            spawnPosition,
            Quaternion.identity
        );

        spawnedBox.SetupBox(_stockInfo);
    }
    #endregion
}