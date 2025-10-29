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
    #endregion

    #region Serialized Private Fields
    [SerializeField] private StockInfo info;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void OnEnable()
    {
        SetShelfLabelText();
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
            SetShelfLabelText();
        }
        else if (objectsOnShelf.Count == 0)
        {
            SetShelfLabelText();
        }

        return objectToReturn;
    }

    public void PlaceStock(StockObject objectToPlace)
    {
        bool preventPlacing = true;
        if (objectsOnShelf.Count == 0)
        {
            info = objectToPlace.Info;
            preventPlacing = false;
        }
        else if (info.name == objectToPlace.Info.name)
        {
            preventPlacing = false;

            switch (info.typeOfStock)
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

            switch (info.typeOfStock)
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
            SetShelfLabelText();
        }
    }
    #endregion

    #region Private Methods
    private void SetShelfLabelText()
    {
        if (objectsOnShelf.Count == 0)
        {
            shelfNameText.text = "-";
            shelfPriceText.text = "$0.00";
            shelfCountText.text = "0";
        }
        else if (objectsOnShelf.Count > 0)
        {
            shelfNameText.text = $"{objectsOnShelf[0].Info.name}";
            shelfPriceText.text = $"${objectsOnShelf[0].Info.price}";
            shelfCountText.text = $"{objectsOnShelf.Count}";
        }
    }
    #endregion
}