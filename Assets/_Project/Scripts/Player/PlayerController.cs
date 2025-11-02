using masonbell;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    #region Event Fields
    public static event Action OnUIPanelClosedWithCancelAction;
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera cameraTransform;

    [Header("Interaction")]
    [SerializeField] private Transform _holdPoint;

    [SerializeField] private StockBoxController heldBox;
    [SerializeField] private Transform boxHoldPoint;

    [SerializeField] private FurnitureController _heldFurniture;
    [SerializeField] private Transform _furnitureHoldPoint;

    [SerializeField] private LayerMask whatIsStock;
    [SerializeField] private LayerMask whatIsShelf;
    [SerializeField] private LayerMask whatIsPriceLabel;
    [SerializeField] private LayerMask whatIsStockBox;
    [SerializeField] private LayerMask whatIsTrash;
    [SerializeField] private LayerMask whatIsFurniture;

    [SerializeField] private float interactionRange;
    [SerializeField] private float throwForce;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float lookSpeed;
    [SerializeField] private float jumpForce;
    [SerializeField] private float _minLookAngle;
    [SerializeField] private float _maxLookAngle;
    #endregion

    #region Private Fields
    [Header("Input")]
    private InputSystem_Actions _gameInput;
    private InputAction _playerMapMoveAction;
    private InputAction _playerMapLookAction;
    private InputAction _playerMapJumpAction;
    private InputAction _playerMapInteractAction;
    private InputAction _playerMapDropAction;
    private InputAction _playerMapOpenBoxAction;
    private InputAction _playerMapPickupFurnitureAction;

    private InputAction _uiMapSubmitAction;
    private InputAction _uiMapCancelAction;

    [Header("Interaction")]
    private StockObject _heldObject;
    private float _placeStockCounter;
    private bool _canPlaceBoxObjectFast;

    [Header("Movement")]
    private float _ySpeed;
    private float _horizontalRotation;
    private float _verticalRotation;
    #endregion

    #region Public Properties
    
    [field: SerializeField, Header("Properties")] public float WaitToPlaceStock { get; private set; }

    public bool HasHeldObject => _heldObject != null;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;

            _gameInput = new();
        _gameInput.Enable();
        #region Player Map Actions
        _playerMapMoveAction = _gameInput.Player.Move;
        _playerMapLookAction = _gameInput.Player.Look;   
        _playerMapJumpAction = _gameInput.Player.Jump;
        _playerMapInteractAction = _gameInput.Player.Interact;
        _playerMapDropAction = _gameInput.Player.DropHeldItem;
        _playerMapOpenBoxAction = _gameInput.Player.OpenBox;
        _playerMapPickupFurnitureAction = _gameInput.Player.PickupFurniture;
        #endregion

        #region UI Map Actions
        _uiMapSubmitAction = _gameInput.UI.Submit;
        _uiMapCancelAction = _gameInput.UI.Cancel;
        #endregion
    }

    private void OnEnable()
    {
        _playerMapInteractAction.performed += CheckForInteraction;
        _playerMapInteractAction.canceled += CheckForInteractionCancel;
        _playerMapDropAction.performed += CheckForDrop;
        _playerMapOpenBoxAction.performed += CheckForOpenCloseBox;
        _playerMapPickupFurnitureAction.performed += CheckForFurniturePickup;

        _uiMapSubmitAction.performed += UIApplyWithSubmit;
        _uiMapCancelAction.performed += UICloseWithCancelAction;
        UIController.OnUIPanelClosed += DisableUIEnablePlayer;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Move();
        Rotate();

        if (_canPlaceBoxObjectFast)
        {
            PlaceBoxObjectsFast();
        }

        if (_heldFurniture != null)
            KeepHeldFurnitureAboveGround(); // TODO: Maybe come up with a better way to do this?
    }

    private void OnDisable()
    {
        _playerMapInteractAction.performed -= CheckForInteraction;
        _playerMapInteractAction.canceled -= CheckForInteractionCancel;
        _playerMapDropAction.performed -= CheckForDrop;
        _playerMapOpenBoxAction.performed -= CheckForOpenCloseBox;

        _uiMapSubmitAction.performed -= UIApplyWithSubmit;
        _uiMapCancelAction.performed -= UICloseWithCancelAction;
        UIController.OnUIPanelClosed -= DisableUIEnablePlayer;

        _gameInput.UI.Disable();
        _gameInput.Player.Disable();
    }
    #endregion

    #region Public Methods
    public void DisableUIEnablePlayer()
    {
        _gameInput.UI.Disable();
        _gameInput.Player.Enable();
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void DisablePlayerEnableUI()
    {
        _gameInput.Player.Disable();
        _gameInput.UI.Enable();
        Cursor.lockState = CursorLockMode.None;
    }
    #endregion

    #region Private Methods
    private void Move()
    {
        Vector2 moveInput = _playerMapMoveAction.ReadValue<Vector2>();
        Vector3 verticalMove = transform.forward * moveInput.y;
        Vector3 horizontalMove = transform.right * moveInput.x;
        Vector3 moveAmount = (horizontalMove + verticalMove).normalized;

        moveAmount *= moveSpeed;
        if (controller.isGrounded) 
            _ySpeed = 0f;
        _ySpeed += (Physics.gravity.y * Time.deltaTime);
        Jump();
        moveAmount.y = _ySpeed;
        controller.Move(moveAmount * Time.deltaTime);
    }

    private void Rotate()
    {
        Vector2 lookInput = _playerMapLookAction.ReadValue<Vector2>();
        _horizontalRotation += lookInput.x * Time.deltaTime * lookSpeed;
        transform.rotation = Quaternion.Euler(0f, _horizontalRotation, 0f);
        _verticalRotation -= lookInput.y * Time.deltaTime * lookSpeed;
        _verticalRotation = Mathf.Clamp(_verticalRotation, _minLookAngle, _maxLookAngle);
        cameraTransform.transform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }

    private void Jump()
    {
        if (_playerMapJumpAction.IsPressed() && controller.isGrounded)
        {
            _ySpeed = jumpForce;
        }
    }

    #region Input Callbacks
    private void CheckForInteraction(InputAction.CallbackContext context) // Interact Key Performed
    {
        Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (_heldObject == null && heldBox == null && _heldFurniture == null)
        {
            if (Physics.Raycast(ray, out hit, interactionRange, whatIsStock))
            {
                _heldObject = hit.collider.GetComponent<StockObject>();
                _heldObject.Pickup(_holdPoint);
                return;
            }

            if (Physics.Raycast(ray, out hit, interactionRange, whatIsStockBox))
            {
                heldBox = hit.collider.GetComponent<StockBoxController>();
                heldBox.Pickup(boxHoldPoint);

                if (!heldBox.OpenBox) // TODO: You can remove this if you don't want to Open box immediately when picking up
                    heldBox.OpenClose();
                return;
            }

            if (Physics.Raycast(ray, out hit, interactionRange, whatIsPriceLabel))
            {
                hit.collider.GetComponentInParent<ShelfSpaceController>().StartPriceUpdate();
                return;
            }
            
            if (Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
            {
                _heldObject = hit.collider.GetComponent<ShelfSpaceController>().GetStock();
                if (_heldObject != null)
                    _heldObject.Pickup(_holdPoint);
                return;
            }
        }
        else
        {
            if (_heldObject != null) // Holding Stock
            {
                if (Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
                {
                    hit.collider.GetComponent<ShelfSpaceController>().PlaceStock(_heldObject);

                    if (_heldObject.IsPlaced)
                        _heldObject = null;
                    return;
                }

                if (Physics.Raycast(ray, out hit, interactionRange, whatIsTrash))
                {
                    _heldObject.TrashObject();
                    _heldObject = null;
                }
            }

            if (heldBox != null) // Holding Box
            {
                if (heldBox.stockInBox.Count > 0) // Interact with Shelf
                {
                    if (Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
                    {
                        heldBox.PlaceStockOnShelf(hit.collider.GetComponent<ShelfSpaceController>());
                        _placeStockCounter = WaitToPlaceStock;
                        _canPlaceBoxObjectFast = true;
                    }
                }
                else // Detect Trash Can
                {
                    if (Physics.Raycast(ray, out _, interactionRange, whatIsTrash))
                    {
                        heldBox.Release();
                        heldBox.TrashObject();
                        heldBox = null;
                    }
                }
            }

            if (_heldFurniture != null) // Holding Furniture
            {
                SetFurnitureDownAndNull();
            }
        }
    }

    private void CheckForInteractionCancel(InputAction.CallbackContext context)
    {
        if (_heldObject == null && heldBox != null)
        {
            if (_canPlaceBoxObjectFast)
                _canPlaceBoxObjectFast = false;
        }
    }
    
    private void CheckForDrop(InputAction.CallbackContext context)
    {
        if (_heldObject != null)
        {
            _heldObject.Release();
            _heldObject.Rb.AddForce(cameraTransform.transform.forward * throwForce, ForceMode.Impulse);
            _heldObject.transform.SetParent(null);
            _heldObject = null;
            return;
        }

        if (heldBox != null)
        {
            heldBox.Release();
            heldBox.Rb.AddForce(cameraTransform.transform.forward * throwForce, ForceMode.Impulse);
            heldBox.transform.SetParent(null);
            heldBox = null;
            return;
        }
    }

    private void CheckForOpenCloseBox(InputAction.CallbackContext obj)
    {
        if (heldBox == null)
        {
            Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactionRange, whatIsStockBox))
            {
                hit.collider.GetComponent<StockBoxController>().OpenClose();
                return;
            }
        }
        else
        {
            heldBox.OpenClose();
        }
    }

    private void CheckForFurniturePickup(InputAction.CallbackContext context)
    {
        Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, interactionRange, whatIsFurniture))
        {
            _heldFurniture = hit.transform.GetComponent<FurnitureController>();
            hit.collider.enabled = false;
            _heldFurniture.transform.SetParent(_furnitureHoldPoint);
            _heldFurniture.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            _heldFurniture.MakePlaceable();
            return;
        }

        if (_heldFurniture != null) // Press Pickup Button while holding already to set down
        {
            SetFurnitureDownAndNull();
        }
    }

    private void UIApplyWithSubmit(InputAction.CallbackContext context)
    {
        UIController.Instance.ApplyPriceUpdate();
    }

    private void UICloseWithCancelAction(InputAction.CallbackContext context)
    {
        OnUIPanelClosedWithCancelAction?.Invoke();
        DisableUIEnablePlayer();
    }
    #endregion

    #region Box Methods
    private void PlaceBoxObjectsFast()
    {
        if (_heldObject == null && heldBox != null)
        {
            Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));

            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, whatIsShelf))
            {
                _placeStockCounter -= Time.deltaTime;
                if (_placeStockCounter <= 0 && _playerMapInteractAction.IsPressed())
                {
                    heldBox.PlaceStockOnShelf(hit.collider.GetComponent<ShelfSpaceController>());
                    _placeStockCounter = WaitToPlaceStock;
                }
            }
        }
    }
    #endregion

    #region Furniture Methods
    private void KeepHeldFurnitureAboveGround()
    {
        _heldFurniture.transform.position = new(_furnitureHoldPoint.position.x, 0f, _furnitureHoldPoint.position.z);
        _heldFurniture.transform.LookAt(new Vector3(transform.position.x, 0f, transform.position.z));
    }

    private void SetFurnitureDownAndNull()
    {
        if (_heldFurniture == null) return;

        _heldFurniture.PlaceObject();
        _heldFurniture = null;
    }
    #endregion
    #endregion
}