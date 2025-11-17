using System.Collections.Generic;
using UnityEngine;

public class FurnitureController : InteractableObject, IPlaceable
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
    private PlayerInteraction _player;
    #endregion

    #region Properties
    public List<ShelfSpaceController> Shelves => _shelves;
    public Transform CustomerStandPoint => _customerStandPoint;
    public float Price => _price;
    public bool IsHeld { get; private set; }

    #endregion

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        RegisterWithStore();
    }

    private void Update()
    {
        if (IsHeld)
            KeepFurnitureAboveGround();
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
        _player = null;
        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(5);
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
            IsHeld = false;
        else if (_placingObject.activeSelf)
            IsHeld = true;
    }

    public void Pickup(PlayerInteraction player)
    {
        _player = player;
        transform.SetParent(player.FurnitureHoldPoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        player.HeldFurniture = this;
        player.HeldObject = gameObject;
        _collider.enabled = false;
        MakePlaceable();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(4);
    }

    private void KeepFurnitureAboveGround() // TODO: Kinda' Janky getting a temp reference to player in Pickup. Maybe do this better later.
    {
        Vector3 holdPosition = _player.FurnitureHoldPoint.position;
        Vector3 playerPosition = _player.transform.position;

        transform.position = new Vector3(holdPosition.x, 0f, holdPosition.z);
        transform.LookAt(new Vector3(playerPosition.x, 0f, playerPosition.z));
    }
    #endregion
}