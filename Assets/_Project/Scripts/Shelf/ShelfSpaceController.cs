using PrimeTween;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class ShelfSpaceController : InteractableObject
{
    #region Serialized Fields
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
    [field: SerializeField] public List<StockObject> ObjectsOnShelf { get; set; }
    public StockInfo StockInfo { get; set; }
    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
        _outline.OutlineMode = Outline.Mode.OutlineAll;
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

        int lastIndex = ObjectsOnShelf.Count - 1;
        StockObject objectToReturn = ObjectsOnShelf[lastIndex];
        ObjectsOnShelf.RemoveAt(lastIndex);

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

    private bool IsShelfFull(StockInfo.StockType stockType) => ObjectsOnShelf.Count >= GetCountForListOfStockType(stockType);

    private bool IsShelfEmpty() => ObjectsOnShelf.Count == 0;

    private void PlaceStockAtPoint(StockObject stock)
    {
        if (stock == null || StockInfo == null) return;

        List<Transform> points = GetListOfPointsForStockType(StockInfo.typeOfStock);

        if (points == null || points.Count == 0 || ObjectsOnShelf.Count == points.Count)
        {
            return;
        }
        ObjectsOnShelf.Add(stock);
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
        StartCoroutine(MoveToPlacementPointCoroutine(stock, endPointPosition, endPointRotation));
    }

    private IEnumerator MoveToPlacementPointCoroutine(StockObject stock, Vector3 endPointPosition, Quaternion endPointRotation)
    {
        yield return Tween.Rotation(stock.transform, endPointRotation, 0f).ToYieldInstruction();
        yield return Tween.Position(stock.transform, endPointPosition, StockInfoController.Instance.StockPickupAndPlaceWaitTimeDuration).ToYieldInstruction();
    }

    private int GetCountForListOfStockType(StockInfo.StockType stockType)
    {
        return stockType switch
        {
            global::StockInfo.StockType.BigDrink => _bigDrinkPoints?.Count ?? 0,
            global::StockInfo.StockType.Cereal => _cerealPoints?.Count ?? 0,
            global::StockInfo.StockType.TubeChips => _tubeChipPoints?.Count ?? 0,
            global::StockInfo.StockType.Fruit => _fruitPoints?.Count ?? 0,
            global::StockInfo.StockType.FruitLarge => _largeFruitPoints?.Count ?? 0,
            global::StockInfo.StockType.Vegetable => _vegetablePoints?.Count ?? 0,
            _ => 0
        };
    }

    private List<Transform> GetListOfPointsForStockType(StockInfo.StockType type)
    {
        return type switch
        {
            global::StockInfo.StockType.BigDrink => _bigDrinkPoints,
            global::StockInfo.StockType.Cereal => _cerealPoints,
            global::StockInfo.StockType.TubeChips => _tubeChipPoints,
            global::StockInfo.StockType.Fruit => _fruitPoints,
            global::StockInfo.StockType.FruitLarge => _largeFruitPoints,
            global::StockInfo.StockType.Vegetable => _vegetablePoints,
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
        if (_shelfCountText != null) _shelfCountText.text = $"{ObjectsOnShelf.Count}";
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

    private void TryTakeStockFromShelfIntoBox(PlayerInteraction player)
    {
        if (player.HeldBox == null || StockInfo == null) return;
        if (player.HeldBox.IsTaking || player.HeldBox.IsPlacingStock) return;
        if (ObjectsOnShelf == null || ObjectsOnShelf.Count == 0) return;

        bool canTakeStock = (player.HeldBox.StockInfo == null && player.HeldBox.StockInBox.Count == 0)
                            || (player.HeldBox.StockInBox.Count > 0
                            && player.HeldBox.StockInBox.Count < player.HeldBox.MaxCapacity
                            && player.HeldBox.StockInfo.Name == StockInfo.Name);

        if (!canTakeStock) return;

        StockObject stockFromShelf = GetStock();
        if (stockFromShelf != null)
        {
            player.HeldBox.TakeStockFromShelf(stockFromShelf);
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
            if (player.HeldStock.IsMoving) return;

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
            if (box.IsTaking || box.IsPlacingStock) return;
            if (box.StockInBox.Count == 0) return;

            box.PlaceStockOnShelf(this);

            player.IsFastPlacementActive = true; // TODO: Find a way around setting this here later maybe.
            return;
        }
    }

    public override void OnTake(PlayerInteraction player)
    {
        if (!player.IsHoldingSomething && StockInfo != null)
        {
            StockObject temp = GetStock();
            if (temp == null) return;
            if (temp.IsMoving) return;
            player.HeldStock = temp;
            player.HeldObject = temp.gameObject;
            temp.Pickup(player.StockHoldPoint);
            return;
        }

        if (player.HeldBox == null) return;

        player.IsFastTakeActive = true;
        TryTakeStockFromShelfIntoBox(player);

    }

    public override string GetInteractionPrompt(PlayerInteraction player)
    {
        UIController.Instance.ShowInteractionPrompt();
        if (StockInfo == null)
            return UIController.Instance.SetInteractionText($"{DisplayName}");

        int count = ObjectsOnShelf.Count;
        if (count == 1)
            return UIController.Instance.SetInteractionText($"{count} {StockInfo.name}");
        else
            return UIController.Instance.SetInteractionText($"{count} {StockInfo.name}s");
    }

    public override void OnFocusGained()
    {
        base.OnFocusGained();
    }

    public override void OnFocusLost()
    {
        base.OnFocusLost();
    }

    #region Utility Methods
    #endregion
}