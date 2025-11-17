using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ShelfSpaceController : InteractableObject
{
    #region Serialized Fields
    [Header("Stock Management")]
    [SerializeField] private List<StockObject> _objectsOnShelf = new();
    [SerializeField] private StockInfo _stockInfo;

    [Header("Placement Points")]
    [SerializeField] private List<Transform> _bigDrinkPoints;
    [SerializeField] private List<Transform> _cerealPoints;
    [SerializeField] private List<Transform> _tubeChipPoints;
    [SerializeField] private List<Transform> _fruitPoints;
    [SerializeField] private List<Transform> _largeFruitPoints;
    [SerializeField] private List<Transform> _vegetablePoints;

    [Header("UI References")]
    [SerializeField] private TMP_Text _shelfNameText;
    [SerializeField] private TMP_Text _shelfPriceText;
    [SerializeField] private TMP_Text _shelfCountText;
    #endregion

    #region Private Fields
    #endregion

    #region Properties
    public List<StockObject> ObjectsOnShelf => _objectsOnShelf;
    public StockInfo StockInfo
    {
        get => _stockInfo;
        set => _stockInfo = value;
    }
    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
    }
    private void OnEnable() => UpdateShelfDisplay();

    private void Start()
    {
        UpdateShelfDisplay();
    }
    #endregion

    #region Stock Management
    public StockObject GetStock()
    {
        if (IsShelfEmpty()) return null;

        int lastIndex = _objectsOnShelf.Count - 1;
        StockObject objectToReturn = _objectsOnShelf[lastIndex];
        _objectsOnShelf.RemoveAt(lastIndex);

        if (IsShelfEmpty())
            StockInfo = null;

        UpdateShelfDisplay();

        return objectToReturn;
    }

    public void PlaceStock(StockObject objectToPlace)
    {
        if (objectToPlace == null || objectToPlace.StockInfo == null) return;

        if (!CanPlaceStock(objectToPlace)) return;

        PlaceStockAtPoint(objectToPlace);
    }

    private bool CanPlaceStock(StockObject objectToPlace)
    {
        if (objectToPlace == null || objectToPlace.StockInfo == null) return false;

        if (IsShelfEmpty())
        {
            StockInfo = objectToPlace.StockInfo;
            return true;
        }

        if (StockInfo == null)
        {
            StockInfo = objectToPlace.StockInfo;
            return true;
        }

        return StockInfo.Name == objectToPlace.StockInfo.Name && !IsShelfFull(StockInfo.typeOfStock);
    }

    private bool IsShelfFull(StockInfo.StockType stockType) => _objectsOnShelf.Count >= GetCountForListOfStockType(stockType);

    private bool IsShelfEmpty() => _objectsOnShelf.Count == 0;

    private void PlaceStockAtPoint(StockObject stock)
    {
        if (stock == null || StockInfo == null) return;

        List<Transform> points = GetListOfPointsForStockType(StockInfo.typeOfStock);

        if (points == null || points.Count == 0 || _objectsOnShelf.Count == points.Count)
        {
            return;
        }
        _objectsOnShelf.Add(stock);
        int index = ObjectsOnShelf.Count - 1;
        if (ObjectsOnShelf.Count >= 0 && index < points.Count)
        {
            stock.transform.SetParent(points[index]);
            MoveToPlacementPoint(stock, points[index].position, points[index].rotation);
            stock.MakePlaced();
            UpdateShelfDisplay();
        }
    }

    private void MoveToPlacementPoint(StockObject stock, Vector3 endPointPosition, Quaternion endPointRotation)
    {
        Tween.Position(stock.transform, endPointPosition, StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration);
        Tween.Rotation(stock.transform, endPointRotation, StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration);
    }

    private int GetCountForListOfStockType(StockInfo.StockType stockType)
    {
        return stockType switch
        {
            StockInfo.StockType.BigDrink => _bigDrinkPoints?.Count ?? 0,
            StockInfo.StockType.Cereal => _cerealPoints?.Count ?? 0,
            StockInfo.StockType.TubeChips => _tubeChipPoints?.Count ?? 0,
            StockInfo.StockType.Fruit => _fruitPoints?.Count ?? 0,
            StockInfo.StockType.FruitLarge => _largeFruitPoints?.Count ?? 0,
            StockInfo.StockType.Vegetable => _vegetablePoints?.Count ?? 0,
            _ => 0
        };
    }

    private List<Transform> GetListOfPointsForStockType(StockInfo.StockType type)
    {
        return type switch
        {
            StockInfo.StockType.BigDrink => _bigDrinkPoints,
            StockInfo.StockType.Cereal => _cerealPoints,
            StockInfo.StockType.TubeChips => _tubeChipPoints,
            StockInfo.StockType.Fruit => _fruitPoints,
            StockInfo.StockType.FruitLarge => _largeFruitPoints,
            StockInfo.StockType.Vegetable => _vegetablePoints,
            _ => new List<Transform>()
        };
    }
    #endregion

    #region Price Update
    public void StartPriceUpdate()
    {
        if (IsShelfEmpty())
        {
            return;
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.DisablePlayerEnableUI();
        }

        if (UIController.Instance != null)
        {
            UIController.Instance.OnOpenUpdatePricePanel(StockInfo);
        }
    }

    public void SetShelfLabelText(float newPrice)
    {
        if (IsShelfEmpty())
        {
            DisplayEmptyShelf();
        }
        else
        {
            DisplayStockedShelf(newPrice);
        }
    }

    private void DisplayEmptyShelf()
    {
        if (_shelfNameText != null) _shelfNameText.text = "-";
        if (_shelfPriceText != null) _shelfPriceText.text = "$0.00";
        if (_shelfCountText != null) _shelfCountText.text = "0";
    }

    private void DisplayStockedShelf(float newPrice)
    {
        if (StockInfo == null)
        {
            DisplayEmptyShelf();
            return;
        }

        StockInfo.currentPrice = newPrice;

        if (_shelfNameText != null) _shelfNameText.text = StockInfo.Name;
        if (_shelfPriceText != null) _shelfPriceText.text = $"${StockInfo.currentPrice:0.00}";
        if (_shelfCountText != null) _shelfCountText.text = $"{_objectsOnShelf.Count}";
    }

    private void UpdateShelfDisplay()
    {
        if (StockInfo != null)
        {
            SetShelfLabelText(StockInfo.currentPrice);
        }
        else
        {
            DisplayEmptyShelf();
        }
    }
    #endregion

    public override void OnInteract(PlayerInteraction player)
    {
        if (!player.IsHoldingSomething)
        {
            StockObject temp = GetStock();
            if (temp == null) return;
            player.HeldStock = temp;
            player.HeldObject = temp.gameObject;
            temp.Pickup(player.StockHoldPoint);
            return;
        }

        if (player.HeldStock != null)
        {
            StockObject temp = player.HeldStock;
            PlaceStock(temp);

            if (temp.IsPlaced)
            {
                player.RemoveHeldObjectReference();
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(7);
            }
            return;
        }

        if (player.HeldBox != null)
        {
            StockBoxController box = player.HeldBox;
            if (box.IsTaking || box.IsPlacing) return;
            if (box.StockInBox.Count == 0) return;

            box.PlaceStockOnShelf(this);

            player.IsFastPlacementActive = true; // TODO: Find a way around setting this here later maybe.
            return;
        }
    }
}