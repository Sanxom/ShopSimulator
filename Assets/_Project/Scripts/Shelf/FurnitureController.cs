using System;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureController : MonoBehaviour, IPlaceable
{
    #region Event Fields
    #endregion

    #region Public Fields
    public List<ShelfSpaceController> shelves;

    public GameObject mainObject;
    public GameObject placingObject;
    public Transform customerStandPoint;
    public Collider col;
    public float price;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        if (shelves.Count > 0)
        {
            StoreController.Instance.shelvingCases.Add(this);
        }
    }
    #endregion

    #region Public Methods
    public void MakePlaceable()
    {
        mainObject.SetActive(false);
        placingObject.SetActive(true);
        col.enabled = false;
    }

    public void PlaceObject()
    {
        mainObject.SetActive(true);
        placingObject.SetActive(false);
        col.enabled = true;
        transform.SetParent(null);

    }
    #endregion

    #region Private Methods
    #endregion
}