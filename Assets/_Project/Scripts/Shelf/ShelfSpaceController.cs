using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShelfSpaceController : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    public List<StockObject> objectsOnShelf = new();
    public List<Transform> bigDrinkPoints;
    public List<Transform> cerealPoints;
    public List<Transform> tubeChipPoints;
    public List<Transform> fruitPoints;
    public List<Transform> largeFruitPoints;
    public List<Transform> vegetablePoints;
    public TMP_Text shelfNameText;
    public TMP_Text shelfPriceText;
    public TMP_Text shelfCountText;
    public StockInfo Info;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        SetShelfLabelText(Info.currentPrice);
    }

    private void Start()
    {
        SetShelfLabelText(Info.currentPrice);
    }
    #endregion

    #region Public Methods
    public StockObject GetStock()
    {
        StockObject objectToReturn = null;

        if (objectsOnShelf.Count > 0)
        {
            objectToReturn = objectsOnShelf[^1];
            objectsOnShelf.RemoveAt(objectsOnShelf.Count - 1);
            SetShelfLabelText(Info.currentPrice);
        }
        else if (objectsOnShelf.Count == 0)
        {
            SetShelfLabelText(Info.currentPrice);
        }

        return objectToReturn;
    }

    public void PlaceStock(StockObject objectToPlace)
    {
        bool preventPlacing = true;
        if (objectsOnShelf.Count == 0)
        {
            Info = objectToPlace.Info;
            preventPlacing = false;
        }
        else if (Info.name == objectToPlace.Info.name)
        {
            preventPlacing = false;

            switch (Info.typeOfStock)
            {
                case StockInfo.StockType.BigDrink:
                    if (objectsOnShelf.Count >= bigDrinkPoints.Count)
                    {
                        preventPlacing = true;
                    }
                    break;
                case StockInfo.StockType.Cereal:
                    if (objectsOnShelf.Count >= cerealPoints.Count)
                    {
                        preventPlacing = true;
                    }
                    break;
                case StockInfo.StockType.TubeChips:
                    if (objectsOnShelf.Count >= tubeChipPoints.Count)
                    {
                        preventPlacing = true;
                    }
                    break;
                case StockInfo.StockType.Fruit:
                    if (objectsOnShelf.Count >= fruitPoints.Count)
                    {
                        preventPlacing = true;
                    }
                    break;
                case StockInfo.StockType.FruitLarge:
                    if (objectsOnShelf.Count >= largeFruitPoints.Count)
                    {
                        preventPlacing = true;
                    }
                    break;
                case StockInfo.StockType.Vegetable:
                    if (objectsOnShelf.Count >= vegetablePoints.Count)
                    {
                        preventPlacing = true;
                    }
                    break;
            }

        }

        if (!preventPlacing)
        {
            objectToPlace.MakePlaced();

            switch (Info.typeOfStock)
            {
                case StockInfo.StockType.BigDrink:
                        objectToPlace.transform.SetParent(bigDrinkPoints[objectsOnShelf.Count]);
                    break;
                case StockInfo.StockType.Cereal:
                        objectToPlace.transform.SetParent(cerealPoints[objectsOnShelf.Count]);
                    break;
                case StockInfo.StockType.TubeChips:
                        objectToPlace.transform.SetParent(tubeChipPoints[objectsOnShelf.Count]);
                    break;
                case StockInfo.StockType.Fruit:
                        objectToPlace.transform.SetParent(fruitPoints[objectsOnShelf.Count]);
                    break;
                case StockInfo.StockType.FruitLarge:
                        objectToPlace.transform.SetParent(largeFruitPoints[objectsOnShelf.Count]);
                    break;
                case StockInfo.StockType.Vegetable:
                        objectToPlace.transform.SetParent(vegetablePoints[objectsOnShelf.Count]);
                    break;
            }

            objectsOnShelf.Add(objectToPlace);
            SetShelfLabelText(Info.currentPrice);
        }
    }

    public void StartPriceUpdate()
    {
        if (objectsOnShelf.Count <= 0) return;

        PlayerController.Instance.DisablePlayerEnableUI();
        UIController.Instance.OnOpenUpdatePricePanel(Info);
    }

    public void SetShelfLabelText(float newPrice)
    {
        if (objectsOnShelf.Count == 0)
        {
            shelfNameText.text = "-";
            shelfPriceText.text = "$0.00";
            shelfCountText.text = "0";
        }
        else if (objectsOnShelf.Count > 0)
        {
            Info.currentPrice = newPrice;
            shelfNameText.text = $"{objectsOnShelf[0].Info.name}";
            shelfPriceText.text = $"${Info.currentPrice:0.00}";
            shelfCountText.text = $"{objectsOnShelf.Count}";
        }
    }
    #endregion

    #region Private Methods
    #endregion
}