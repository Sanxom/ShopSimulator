#region Claude Code v2
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShelfSpaceController : MonoBehaviour, IInteractable
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

    #region Properties
    public GameObject MyObject { get; set; }
    public List<StockObject> ObjectsOnShelf => _objectsOnShelf;
    public StockInfo StockInfo
    {
        get => _stockInfo;
        set => _stockInfo = value;
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        MyObject = gameObject;
    }

    private void OnEnable()
    {
        UpdateShelfDisplay();
    }

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
        {
            StockInfo = null;
        }
        UpdateShelfDisplay();

        return objectToReturn;
    }

    public void PlaceStock(StockObject objectToPlace)
    {
        // Add null checks at the beginning
        if (objectToPlace == null || objectToPlace.StockInfo == null)
        {
            Debug.LogWarning("Cannot place stock: objectToPlace or its StockInfo is null");
            return;
        }

        if (!CanPlaceStock(objectToPlace)) return;

        _objectsOnShelf.Add(objectToPlace);
        PlaceStockAtPoint(objectToPlace);
        UpdateShelfDisplay();
    }

    private bool CanPlaceStock(StockObject objectToPlace)
    {
        // Null check already done in PlaceStock, but keeping for safety
        if (objectToPlace == null || objectToPlace.StockInfo == null)
        {
            return false;
        }

        if (IsShelfEmpty())
        {
            StockInfo = objectToPlace.StockInfo;
            return true;
        }

        // Ensure StockInfo is not null before comparing
        if (StockInfo == null)
        {
            StockInfo = objectToPlace.StockInfo;
            return true;
        }

        return StockInfo.name == objectToPlace.StockInfo.name && !IsShelfFull(StockInfo.typeOfStock);
    }

    private bool IsShelfFull(StockInfo.StockType stockType)
    {
        return _objectsOnShelf.Count >= GetCountForListOfStockType(stockType);
    }

    private bool IsShelfEmpty() => _objectsOnShelf.Count == 0;

    private void PlaceStockAtPoint(StockObject stock)
    {
        // Add safety check
        if (stock == null || StockInfo == null)
        {
            Debug.LogWarning("Cannot place stock at point: stock or StockInfo is null");
            return;
        }

        stock.MakePlaced();
        List<Transform> points = GetListOfPointsForStockType(StockInfo.typeOfStock);

        if (points != null && points.Count > 0 && _objectsOnShelf.Count <= points.Count)
        {
            int pointIndex = _objectsOnShelf.Count - 1; // We already added the stock to the list
            if (pointIndex >= 0 && pointIndex < points.Count)
            {
                stock.transform.SetParent(points[pointIndex]);
                stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
        }
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

        if (_shelfNameText != null) _shelfNameText.text = StockInfo.name;
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

    public void OnInteract(Transform heldObject)
    {

    }

    public string GetInteractionPrompt()
    {
        return "Shelf Space";
    }
    #endregion
}
#endregion

#region Claude Code v1
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class ShelfSpaceController : MonoBehaviour
//{
//    #region Serialized Fields
//    [Header("Stock Management")]
//    [SerializeField] private List<StockObject> _objectsOnShelf = new();
//    [SerializeField] private StockInfo _info;

//    [Header("Placement Points")]
//    [SerializeField] private List<Transform> _bigDrinkPoints;
//    [SerializeField] private List<Transform> _cerealPoints;
//    [SerializeField] private List<Transform> _tubeChipPoints;
//    [SerializeField] private List<Transform> _fruitPoints;
//    [SerializeField] private List<Transform> _largeFruitPoints;
//    [SerializeField] private List<Transform> _vegetablePoints;

//    [Header("UI References")]
//    [SerializeField] private TMP_Text _shelfNameText;
//    [SerializeField] private TMP_Text _shelfPriceText;
//    [SerializeField] private TMP_Text _shelfCountText;
//    #endregion

//    #region Properties
//    public List<StockObject> ObjectsOnShelf => _objectsOnShelf;
//    public StockInfo Info
//    {
//        get => _info;
//        set => _info = value;
//    }
//    #endregion

//    #region Unity Lifecycle
//    private void OnEnable()
//    {
//        UpdateShelfDisplay();
//    }

//    private void Start()
//    {
//        UpdateShelfDisplay();
//    }
//    #endregion

//    #region Stock Management
//    public StockObject GetStock()
//    {
//        if (_objectsOnShelf.Count == 0)
//        {
//            UpdateShelfDisplay();
//            return null;
//        }

//        int lastIndex = _objectsOnShelf.Count - 1;
//        StockObject objectToReturn = _objectsOnShelf[lastIndex];
//        _objectsOnShelf.RemoveAt(lastIndex);

//        UpdateShelfDisplay();
//        return objectToReturn;
//    }

//    public void PlaceStock(StockObject objectToPlace)
//    {
//        if (!CanPlaceStock(objectToPlace, out bool isShelfFull))
//        {
//            return;
//        }

//        if (isShelfFull)
//        {
//            return;
//        }

//        PlaceStockAtPoint(objectToPlace);
//        _objectsOnShelf.Add(objectToPlace);
//        UpdateShelfDisplay();
//    }

//    private bool CanPlaceStock(StockObject objectToPlace, out bool isShelfFull)
//    {
//        isShelfFull = false;

//        if (_objectsOnShelf.Count == 0)
//        {
//            _info = objectToPlace.Info;
//            return true;
//        }

//        if (_info.name != objectToPlace.Info.name)
//        {
//            return false;
//        }

//        isShelfFull = IsShelfFull(_info.typeOfStock);
//        return !isShelfFull;
//    }

//    private bool IsShelfFull(StockInfo.StockType stockType)
//    {
//        List<Transform> points = GetPointsForStockType(stockType);
//        return _objectsOnShelf.Count >= points.Count;
//    }

//    private void PlaceStockAtPoint(StockObject stock)
//    {
//        stock.MakePlaced();
//        List<Transform> points = GetPointsForStockType(_info.typeOfStock);
//        if (points.Count > 0 && _objectsOnShelf.Count < points.Count)
//        {
//            stock.transform.SetParent(points[_objectsOnShelf.Count]);
//        }
//    }

//    private List<Transform> GetPointsForStockType(StockInfo.StockType type)
//    {
//        return type switch
//        {
//            StockInfo.StockType.BigDrink => _bigDrinkPoints,
//            StockInfo.StockType.Cereal => _cerealPoints,
//            StockInfo.StockType.TubeChips => _tubeChipPoints,
//            StockInfo.StockType.Fruit => _fruitPoints,
//            StockInfo.StockType.FruitLarge => _largeFruitPoints,
//            StockInfo.StockType.Vegetable => _vegetablePoints,
//            _ => new List<Transform>()
//        };
//    }
//    #endregion

//    #region Price Update
//    public void StartPriceUpdate()
//    {
//        if (_objectsOnShelf.Count <= 0)
//        {
//            return;
//        }

//        if (PlayerController.Instance != null)
//        {
//            PlayerController.Instance.DisablePlayerEnableUI();
//        }

//        if (UIController.Instance != null)
//        {
//            UIController.Instance.OnOpenUpdatePricePanel(_info);
//        }
//    }

//    public void SetShelfLabelText(float newPrice)
//    {
//        if (_objectsOnShelf.Count == 0)
//        {
//            DisplayEmptyShelf();
//        }
//        else
//        {
//            DisplayStockedShelf(newPrice);
//        }
//    }

//    private void DisplayEmptyShelf()
//    {
//        if (_shelfNameText != null) _shelfNameText.text = "-";
//        if (_shelfPriceText != null) _shelfPriceText.text = "$0.00";
//        if (_shelfCountText != null) _shelfCountText.text = "0";
//    }

//    private void DisplayStockedShelf(float newPrice)
//    {
//        _info.currentPrice = newPrice;

//        if (_shelfNameText != null) _shelfNameText.text = _info.name;
//        if (_shelfPriceText != null) _shelfPriceText.text = $"${_info.currentPrice:0.00}";
//        if (_shelfCountText != null) _shelfCountText.text = $"{_objectsOnShelf.Count}";
//    }

//    private void UpdateShelfDisplay()
//    {
//        if (_info != null)
//        {
//            SetShelfLabelText(_info.currentPrice);
//        }
//        else
//        {
//            DisplayEmptyShelf();
//        }
//    }
//    #endregion
//}
#endregion

#region James' Code
//using System;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;

//public class ShelfSpaceController : MonoBehaviour
//{
//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public List<StockObject> objectsOnShelf = new();
//    public List<Transform> bigDrinkPoints;
//    public List<Transform> cerealPoints;
//    public List<Transform> tubeChipPoints;
//    public List<Transform> fruitPoints;
//    public List<Transform> largeFruitPoints;
//    public List<Transform> vegetablePoints;
//    public TMP_Text shelfNameText;
//    public TMP_Text shelfPriceText;
//    public TMP_Text shelfCountText;
//    public StockInfo Info;
//    #endregion

//    #region Serialized Private Fields
//    #endregion

//    #region Private Fields
//    #endregion

//    #region Public Properties
//    #endregion

//    #region Unity Callbacks
//    private void OnEnable()
//    {
//        SetShelfLabelText(Info.currentPrice);
//    }

//    private void Start()
//    {
//        SetShelfLabelText(Info.currentPrice);
//    }
//    #endregion

//    #region Public Methods
//    public StockObject GetStock()
//    {
//        StockObject objectToReturn = null;

//        if (objectsOnShelf.Count > 0)
//        {
//            print("Removing Stock");
//            objectToReturn = objectsOnShelf[objectsOnShelf.Count - 1];
//            objectsOnShelf.RemoveAt(objectsOnShelf.Count - 1);
//            SetShelfLabelText(Info.currentPrice);
//        }
//        else if (objectsOnShelf.Count == 0)
//        {
//            SetShelfLabelText(Info.currentPrice);
//        }

//        return objectToReturn;
//    }

//    public void PlaceStock(StockObject objectToPlace)
//    {
//        bool preventPlacing = true;
//        if (objectsOnShelf.Count == 0)
//        {
//            Info = objectToPlace.Info;
//            preventPlacing = false;
//        }
//        else if (Info.name == objectToPlace.Info.name)
//        {
//            preventPlacing = false;

//            switch (Info.typeOfStock)
//            {
//                case StockInfo.StockType.BigDrink:
//                    if (objectsOnShelf.Count >= bigDrinkPoints.Count)
//                    {
//                        preventPlacing = true;
//                    }
//                    break;
//                case StockInfo.StockType.Cereal:
//                    if (objectsOnShelf.Count >= cerealPoints.Count)
//                    {
//                        preventPlacing = true;
//                    }
//                    break;
//                case StockInfo.StockType.TubeChips:
//                    if (objectsOnShelf.Count >= tubeChipPoints.Count)
//                    {
//                        preventPlacing = true;
//                    }
//                    break;
//                case StockInfo.StockType.Fruit:
//                    if (objectsOnShelf.Count >= fruitPoints.Count)
//                    {
//                        preventPlacing = true;
//                    }
//                    break;
//                case StockInfo.StockType.FruitLarge:
//                    if (objectsOnShelf.Count >= largeFruitPoints.Count)
//                    {
//                        preventPlacing = true;
//                    }
//                    break;
//                case StockInfo.StockType.Vegetable:
//                    if (objectsOnShelf.Count >= vegetablePoints.Count)
//                    {
//                        preventPlacing = true;
//                    }
//                    break;
//            }

//        }

//        if (!preventPlacing)
//        {
//            objectToPlace.MakePlaced();

//            switch (Info.typeOfStock)
//            {
//                case StockInfo.StockType.BigDrink:
//                        objectToPlace.transform.SetParent(bigDrinkPoints[objectsOnShelf.Count]);
//                    break;
//                case StockInfo.StockType.Cereal:
//                        objectToPlace.transform.SetParent(cerealPoints[objectsOnShelf.Count]);
//                    break;
//                case StockInfo.StockType.TubeChips:
//                        objectToPlace.transform.SetParent(tubeChipPoints[objectsOnShelf.Count]);
//                    break;
//                case StockInfo.StockType.Fruit:
//                        objectToPlace.transform.SetParent(fruitPoints[objectsOnShelf.Count]);
//                    break;
//                case StockInfo.StockType.FruitLarge:
//                        objectToPlace.transform.SetParent(largeFruitPoints[objectsOnShelf.Count]);
//                    break;
//                case StockInfo.StockType.Vegetable:
//                        objectToPlace.transform.SetParent(vegetablePoints[objectsOnShelf.Count]);
//                    break;
//            }

//            objectsOnShelf.Add(objectToPlace);
//            SetShelfLabelText(Info.currentPrice);
//        }
//    }

//    public void StartPriceUpdate()
//    {
//        if (objectsOnShelf.Count <= 0) return;

//        PlayerController.Instance.DisablePlayerEnableUI();
//        UIController.Instance.OnOpenUpdatePricePanel(Info);
//    }

//    public void SetShelfLabelText(float newPrice)
//    {
//        if (objectsOnShelf.Count == 0)
//        {
//            shelfNameText.text = "-";
//            shelfPriceText.text = "$0.00";
//            shelfCountText.text = "0";
//        }
//        else if (objectsOnShelf.Count > 0)
//        {
//            Info.currentPrice = newPrice;
//            shelfNameText.text = $"{objectsOnShelf[0].Info.name}";
//            shelfPriceText.text = $"${Info.currentPrice:0.00}";
//            shelfCountText.text = $"{objectsOnShelf.Count}";
//        }
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}
#endregion