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

    #region Private Fields
    private bool _isHeld = false;
    #endregion

    #region Properties
    public GameObject MyObject { get; set; }
    public List<ShelfSpaceController> Shelves => _shelves;
    public Transform CustomerStandPoint => _customerStandPoint;
    public float Price => _price;
    public bool IsHeld => _isHeld;

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
        if (Shelves.Count > 0 && StoreController.Instance != null)
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

        if (_mainObject.activeSelf)
            _isHeld = false;
        else if (_placingObject.activeSelf)
            _isHeld = true;
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