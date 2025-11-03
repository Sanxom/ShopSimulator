using System;
using System.Collections.Generic;
using UnityEngine;

public class FurnitureController : MonoBehaviour, IPlaceable
{
    #region Event Fields
    #endregion

    #region Public Fields
    public List<ShelfSpaceController> frontShelves;
    public List<ShelfSpaceController> backShelves;
    public List<ShelfSpaceController> allShelves;

    public GameObject mainObject;
    public GameObject placingObject;
    public Transform customerStandPointFront;
    public Transform customerStandPointBack;
    public Collider col;
    public float price;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    public bool IsDoubleSided { get; private set; } = false;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        allShelves.AddRange(frontShelves);
        if (backShelves.Count > 0)
        {
            IsDoubleSided = true;
            allShelves.AddRange(backShelves);
        }
    }
    private void Start()
    {
        if (allShelves.Count > 0)
        {
            StoreController.Instance.shelvingCases.Add(this);
        }

        // TODO: Make this more efficient.  Right now, we are adding two copies of this instance to StoreController List if this is double-sided
        if (IsDoubleSided)
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