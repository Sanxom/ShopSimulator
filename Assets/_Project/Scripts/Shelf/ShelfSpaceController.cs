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

    [Header("Box Collider Outline")]
    private BoxCollider boxCollider;
    private LineRenderer bottomFaceRenderer;
    private LineRenderer topFaceRenderer;
    private LineRenderer verticalEdge1Renderer;
    private LineRenderer verticalEdge2Renderer;
    private LineRenderer verticalEdge3Renderer;
    private LineRenderer verticalEdge4Renderer;
    [SerializeField] private List<LineRenderer> _allLineRenderersList;

    [SerializeField] Material outlineMaterial;
    [SerializeField] Color outlineColor = Color.green;
    [SerializeField] float outlineWidth = 0.04f;
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

        boxCollider = GetComponent<BoxCollider>();

        // Configure Line Renderer
        bottomFaceRenderer = CreateLineRenderer("BottomFace");
        topFaceRenderer = CreateLineRenderer("TopFace");
        verticalEdge1Renderer = CreateLineRenderer("VerticalEdge1");
        verticalEdge2Renderer = CreateLineRenderer("VerticalEdge2");
        verticalEdge3Renderer = CreateLineRenderer("VerticalEdge3");
        verticalEdge4Renderer = CreateLineRenderer("VerticalEdge4");

        _allLineRenderersList.Add(bottomFaceRenderer);
        _allLineRenderersList.Add(topFaceRenderer);
        _allLineRenderersList.Add(verticalEdge1Renderer);
        _allLineRenderersList.Add(verticalEdge2Renderer);
        _allLineRenderersList.Add(verticalEdge3Renderer);
        _allLineRenderersList.Add(verticalEdge4Renderer);

        foreach (LineRenderer lineRenderer in _allLineRenderersList)
        {
            lineRenderer.useWorldSpace = true; // Use world coordinates for the lines
            outlineMaterial.color = outlineColor;
            lineRenderer.material = outlineMaterial;
            lineRenderer.startColor = outlineColor;
            lineRenderer.endColor = outlineColor;
            lineRenderer.startWidth = outlineWidth;
            lineRenderer.endWidth = outlineWidth;
            lineRenderer.enabled = false;
        }

        DrawBoxColliderOutline();
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

    public override string GetInteractionPrompt()
    {
        if (StockInfo == null)
            return $"Empty Shelf";
        int count = GetCountForListOfStockType(StockInfo.typeOfStock);

        if (count == 1)
            return $"{count} {StockInfo.name}";
        else
            return $"{GetCountForListOfStockType(StockInfo.typeOfStock)} {StockInfo.name}s";
    }

    public override void OnFocusGained()
    {
        base.OnFocusGained();
        foreach (LineRenderer lineRenderer in _allLineRenderersList)
        {
            lineRenderer.enabled = true;
        }
    }

    public override void OnFocusLost()
    {
        base.OnFocusLost();
        foreach (LineRenderer lineRenderer in _allLineRenderersList)
        {
            lineRenderer.enabled = false;
        }
    }

    #region Utility Methods
    void DrawBoxColliderOutline()
    {
        Bounds bounds = boxCollider.bounds;

        // Calculate the 8 corners
        Vector3[] corners = new Vector3[8];
        corners[0] = bounds.min;
        corners[1] = new Vector3(bounds.max.x, bounds.min.y, bounds.min.z);
        corners[2] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
        corners[3] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
        corners[4] = new Vector3(bounds.min.x, bounds.min.y, bounds.max.z);
        corners[5] = new Vector3(bounds.max.x, bounds.min.y, bounds.max.z);
        corners[6] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
        corners[7] = bounds.max;

        // Bottom face loop
        bottomFaceRenderer.positionCount = 5;
        bottomFaceRenderer.SetPositions(new Vector3[] { corners[0], corners[1], corners[3], corners[2], corners[0] });

        // Top face loop
        topFaceRenderer.positionCount = 5;
        topFaceRenderer.SetPositions(new Vector3[] { corners[4], corners[5], corners[7], corners[6], corners[4] });

        // Vertical edges
        verticalEdge1Renderer.positionCount = 2;
        verticalEdge1Renderer.SetPositions(new Vector3[] { corners[0], corners[4] });

        verticalEdge2Renderer.positionCount = 2;
        verticalEdge2Renderer.SetPositions(new Vector3[] { corners[1], corners[5] });

        verticalEdge3Renderer.positionCount = 2;
        verticalEdge3Renderer.SetPositions(new Vector3[] { corners[2], corners[6] });

        verticalEdge4Renderer.positionCount = 2;
        verticalEdge4Renderer.SetPositions(new Vector3[] { corners[3], corners[7] });
    }

    private LineRenderer CreateLineRenderer(string name)
    {
        GameObject lineObj = new GameObject(name);
        lineObj.transform.SetParent(transform);
        lineObj.transform.localPosition = Vector3.zero;
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        return lr;
    }
    #endregion
}