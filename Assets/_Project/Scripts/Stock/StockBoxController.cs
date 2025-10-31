using masonbell;
using System;
using System.Collections.Generic;
using UnityEngine;

public class StockBoxController : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    public StockInfo stockInfo;
    public List<Transform> bigDrinkPoints;
    public List<Transform> cerealPoints;
    public List<Transform> tubeChipPoints;
    public List<Transform> fruitPoints;
    public List<Transform> largeFruitPoints;
    public List<Transform> vegetablePoints;
    public List<StockObject> stockInBox;

    public bool testFill;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    private const float moveSpeed = 10f;

    private bool _isHeld;
    #endregion

    #region Public Properties
    public Rigidbody Rb { get; private set; }
    public Collider Col { get; private set; }
    public Animator Anim { get; private set; }
    public bool OpenBox { get; private set; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Col = GetComponent<Collider>();
        Anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (testFill)
        {
            testFill = false;
            SetupBox(stockInfo);
        }

        if (_isHeld)
        {
            transform.SetLocalPositionAndRotation(Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime),
                Quaternion.Slerp(transform.localRotation, Quaternion.identity, moveSpeed * Time.deltaTime));
        }
    }
    #endregion

    #region Public Methods
    public int GetStockAmount(StockInfo.StockType type)
    {
        int toReturn = 0;

        switch (type)
        {
            case StockInfo.StockType.Cereal:
                toReturn = cerealPoints.Count;
                break;
            case StockInfo.StockType.BigDrink:
                toReturn = bigDrinkPoints.Count;
                break;
            case StockInfo.StockType.TubeChips:
                toReturn = tubeChipPoints.Count;
                break;
            case StockInfo.StockType.Fruit:
                toReturn = fruitPoints.Count;
                break;
            case StockInfo.StockType.FruitLarge:
                toReturn = largeFruitPoints.Count;
                break;
            case StockInfo.StockType.Vegetable:
                toReturn = vegetablePoints.Count;
                break;
            default:
                break;
        }

        return toReturn;
    }
    public void SetupBox(StockInfo stockType)
    {
        stockInfo = stockType;

        List<Transform> activePoints = new();

        switch (stockInfo.typeOfStock)
        {
            case StockInfo.StockType.BigDrink:
                activePoints.AddRange(bigDrinkPoints);
                break;
            case StockInfo.StockType.Cereal:
                activePoints.AddRange(cerealPoints);
                break;
            case StockInfo.StockType.TubeChips:
                activePoints.AddRange(tubeChipPoints);
                break;
            case StockInfo.StockType.Fruit:
                activePoints.AddRange(fruitPoints);
                break;
            case StockInfo.StockType.FruitLarge:
                activePoints.AddRange(largeFruitPoints);
                break;
            case StockInfo.StockType.Vegetable:
                activePoints.AddRange(vegetablePoints);
                break;
        }

        if (stockInBox.Count == 0)
        {
            for (int i = 0; i < activePoints.Count; i++)
            {
                StockObject stock = ObjectPool<StockObject>.GetFromPool(stockType.stockObject, activePoints[i]);
                stock.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
                stockInBox.Add(stock);
                stock.PlaceInBox();
            }
        }
    }

    public void Pickup(Transform holdPoint)
    {
        Rb.isKinematic = true;
        transform.SetParent(holdPoint);
        Col.enabled = false;
        _isHeld = true;
    }

    public void Release()
    {
        Rb.isKinematic = false;
        Col.enabled = true;
        _isHeld = false;
    }

    public void OpenClose()
    {
        OpenBox = !OpenBox;
        Anim.SetBool("openBox", OpenBox);
    }

    public void PlaceStockOnShelf(ShelfSpaceController shelf)
    {
        if (stockInBox.Count > 0)
        {
            shelf.PlaceStock(stockInBox[^1]);

            if (stockInBox[^1].IsPlaced)
            {
                stockInBox.RemoveAt(stockInBox.Count - 1);
            }
        }

        if (!OpenBox)
            OpenClose();
    }
    #endregion

    #region Private Methods
    #endregion
}