using System.Collections.Generic;
using UnityEngine;

public class FurnitureController : MonoBehaviour, IInteractable, IPlaceable
{
    #region Serialized Fields
    [Header("Furniture Settings")]
    [SerializeField] private float _price;
    [SerializeField] private List<ShelfSpaceController> _shelves;

    [Header("References")]
    [SerializeField] private GameObject _mainObject;
    [SerializeField] private GameObject _placingObject;
    [SerializeField] private Transform _customerStandPoint;
    [SerializeField] private Collider _collider;
    #endregion

    #region Properties
    public GameObject MyObject { get; set; }
    public List<ShelfSpaceController> Shelves => _shelves;
    public Transform CustomerStandPoint => _customerStandPoint;
    public float Price => _price;

    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        MyObject = gameObject;
    }

    private void Start()
    {
        RegisterWithStore();
    }
    #endregion

    #region Initialization
    private void RegisterWithStore()
    {
        if (_shelves.Count > 0 && StoreController.Instance != null)
        {
            StoreController.Instance.ShelvingCases.Add(this);
        }
    }
    #endregion

    #region IPlaceable Implementation
    public void MakePlaceable()
    {
        SetObjectState(false, true, false);
    }

    public void PlaceObject()
    {
        SetObjectState(true, false, true);
        transform.SetParent(null);
    }
    #endregion

    #region Private Methods
    private void SetObjectState(bool mainActive, bool placingActive, bool colliderEnabled)
    {
        if (_mainObject != null)
        {
            _mainObject.SetActive(mainActive);
        }

        if (_placingObject != null)
        {
            _placingObject.SetActive(placingActive);
        }

        if (_collider != null)
        {
            _collider.enabled = colliderEnabled;
        }
    }

    public void OnInteract(Transform holdPoint = null)
    {

    }

    public string GetInteractionPrompt()
    {
        return $"Pickup {MyObject.name}";
    }
    #endregion
}

//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class FurnitureController : MonoBehaviour, IPlaceable
//{
//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public List<ShelfSpaceController> shelves;

//    public GameObject mainObject;
//    public GameObject placingObject;
//    public Transform customerStandPoint;
//    public Collider col;
//    public float price;
//    #endregion

//    #region Serialized Private Fields
//    #endregion

//    #region Private Fields
//    #endregion

//    #region Public Properties
//    #endregion

//    #region Unity Callbacks
//    private void Start()
//    {
//        if (shelves.Count > 0)
//        {
//            StoreController.Instance.shelvingCases.Add(this);
//        }
//    }
//    #endregion

//    #region Public Methods
//    public void MakePlaceable()
//    {
//        mainObject.SetActive(false);
//        placingObject.SetActive(true);
//        col.enabled = false;
//    }

//    public void PlaceObject()
//    {
//        mainObject.SetActive(true);
//        placingObject.SetActive(false);
//        col.enabled = true;
//        transform.SetParent(null);

//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}