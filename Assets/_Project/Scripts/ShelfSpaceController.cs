using System;
using System.Collections.Generic;
using UnityEngine;

public class ShelfSpaceController : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    public List<StockObject> objectsOnShelf;
    public List<Transform> bigDrinkPoints;
    #endregion

    #region Serialized Private Fields
    [SerializeField] private StockInfo info;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    #endregion

    #region Public Methods
    public StockObject GetStock()
    {
        StockObject objectToReturn = null;

        if (objectsOnShelf.Count > 0)
        {
            objectToReturn = objectsOnShelf[^1];
            objectsOnShelf.RemoveAt(objectsOnShelf.Count - 1);
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

            if (objectsOnShelf.Count >= bigDrinkPoints.Count)
            {
                preventPlacing = true;
            }
        }

        if (!preventPlacing)
        {
            //objectToPlace.transform.SetParent(transform);
            objectToPlace.MakePlaced();

            objectToPlace.transform.SetParent(bigDrinkPoints[objectsOnShelf.Count]);

            objectsOnShelf.Add(objectToPlace);
        }
    }
    #endregion

    #region Private Methods
    #endregion
}