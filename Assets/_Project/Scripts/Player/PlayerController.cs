#region Claude Code v2
using masonbell;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    #region Events
    public static event Action OnUIPanelClosedWithCancelAction;
    #endregion

    #region Serialized Fields
    [Header("References")]
    [SerializeField] private CharacterController _controller;
    [SerializeField] private Camera _camera;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _lookSpeed = 2f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _minLookAngle = -80f;
    [SerializeField] private float _maxLookAngle = 80f;

    [Header("Interaction Settings")]
    [SerializeField] private float _interactionRange = 3f;
    [SerializeField] private float _throwForce = 10f;
    [SerializeField] private float _waitToPlaceStock = 0.2f;
    [SerializeField] private Transform _stockHoldPoint;
    [SerializeField] private Transform _boxHoldPoint;
    [SerializeField] private Transform _furnitureHoldPoint;

    [Header("Interaction Layers")]
    [SerializeField] private LayerMask _stockLayer;
    [SerializeField] private LayerMask _shelfLayer;
    [SerializeField] private LayerMask _priceLabelLayer;
    [SerializeField] private LayerMask _stockBoxLayer;
    [SerializeField] private LayerMask _trashLayer;
    [SerializeField] private LayerMask _furnitureLayer;
    [SerializeField] private LayerMask _checkoutLayer;
    [SerializeField] private LayerMask _storeSignLayer;
    [SerializeField] private LayerMask _interactableLayer;

    [Header("NUMBER OF ALL INTERACTABLE OBJECTS")]
    [SerializeField] private int _maxPossibleInteractableObjects; //TODO: PLEASE REMEMBER TO CHANGE THIS IF YOU ADD MORE INTERACTABLE LAYERS/OBJECTS
    #endregion

    #region Private Fields
    private InputSystem_Actions _gameInput;
    private PlayerMovement _movement;
    private PlayerInteraction _interaction;
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _interactAction;
    private InputAction _dropAction;
    private InputAction _openBoxAction;
    private InputAction _pickupFurnitureAction;
    private InputAction _takeStockAction;
    private InputAction _submitAction;
    private InputAction _cancelAction;
    #endregion

    #region Properties
    public bool IsHoldingSomething => _interaction.IsHoldingSomething;

    public int MaxPossibleInteractableObjects { get => _maxPossibleInteractableObjects; private set => _maxPossibleInteractableObjects = value; }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
        InitializeInput();
        InitializeComponents();
    }

    private void OnEnable()
    {
        SubscribeToInputEvents();
        UIController.OnUIPanelClosed += DisableUIEnablePlayer;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        _movement.UpdateMovement(deltaTime);
        _interaction.UpdateInteraction(deltaTime);
    }

    private void OnDisable()
    {
        UnsubscribeFromInputEvents();
        UIController.OnUIPanelClosed -= DisableUIEnablePlayer;

        _gameInput?.UI.Disable();
        _gameInput?.Player.Disable();
    }
    #endregion

    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void InitializeInput()
    {
        _gameInput = new InputSystem_Actions();
        _gameInput.Enable();

        _moveAction = _gameInput.Player.Move;
        _lookAction = _gameInput.Player.Look;
        _jumpAction = _gameInput.Player.Jump;
        _interactAction = _gameInput.Player.Interact;
        _dropAction = _gameInput.Player.DropHeldItem;
        _openBoxAction = _gameInput.Player.OpenBox;
        _pickupFurnitureAction = _gameInput.Player.PickupFurniture;
        _takeStockAction = _gameInput.Player.TakeStock;
        _submitAction = _gameInput.UI.Submit;
        _cancelAction = _gameInput.UI.Cancel;
    }

    private void InitializeComponents()
    {
        var movementConfig = new PlayerMovement.Config
        {
            Controller = _controller,
            CameraTransform = _camera.transform,
            Transform = transform,
            MoveSpeed = _moveSpeed,
            LookSpeed = _lookSpeed,
            JumpForce = _jumpForce,
            MinLookAngle = _minLookAngle,
            MaxLookAngle = _maxLookAngle
        };
        _movement = new PlayerMovement(movementConfig, _moveAction, _lookAction, _jumpAction);

        var interactionConfig = new PlayerInteraction.Config
        {
            camera = _camera,
            interactionRange = _interactionRange,
            throwForce = _throwForce,
            waitToPlaceStock = _waitToPlaceStock,
            stockHoldPoint = _stockHoldPoint,
            boxHoldPoint = _boxHoldPoint,
            furnitureHoldPoint = _furnitureHoldPoint,
            stockLayer = _stockLayer,
            shelfLayer = _shelfLayer,
            priceLabelLayer = _priceLabelLayer,
            stockBoxLayer = _stockBoxLayer,
            trashLayer = _trashLayer,
            furnitureLayer = _furnitureLayer,
            checkoutLayer = _checkoutLayer,
            storeSignLayer = _storeSignLayer,
            interactableLayer = _interactableLayer,
        };
        _interaction = new PlayerInteraction(interactionConfig);
        _interaction.SetInputActions(_interactAction, _takeStockAction);
    }
    #endregion

    #region Input Event Subscriptions
    private void SubscribeToInputEvents()
    {
        _interactAction.performed += _interaction.OnInteractPerformed;
        _interactAction.canceled += _interaction.OnInteractCanceled;
        _dropAction.performed += _interaction.OnDropPerformed;
        _openBoxAction.performed += _interaction.OnOpenBoxPerformed;
        _pickupFurnitureAction.performed += _interaction.OnPickupFurniturePerformed;
        _takeStockAction.performed += _interaction.OnTakeStockPerformed;
        _takeStockAction.canceled += _interaction.OnTakeStockCanceled;
        _submitAction.performed += OnSubmitPerformed;
        _cancelAction.performed += OnCancelPerformed;
    }

    private void UnsubscribeFromInputEvents()
    {
        _interactAction.performed -= _interaction.OnInteractPerformed;
        _interactAction.canceled -= _interaction.OnInteractCanceled;
        _dropAction.performed -= _interaction.OnDropPerformed;
        _openBoxAction.performed -= _interaction.OnOpenBoxPerformed;
        _pickupFurnitureAction.performed -= _interaction.OnPickupFurniturePerformed;
        _takeStockAction.performed -= _interaction.OnTakeStockPerformed;
        _takeStockAction.canceled -= _interaction.OnTakeStockCanceled;
        _submitAction.performed -= OnSubmitPerformed;
        _cancelAction.performed -= OnCancelPerformed;
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

    #region Input Callbacks
    private void OnSubmitPerformed(InputAction.CallbackContext context)
    {
        if (UIController.Instance != null)
        {
            UIController.Instance.ApplyPriceUpdate();
        }
    }

    private void OnCancelPerformed(InputAction.CallbackContext context)
    {
        OnUIPanelClosedWithCancelAction?.Invoke();
        DisableUIEnablePlayer();
    }
    #endregion
}

#region Movement Component
public class PlayerMovement
{
    public struct Config
    {
        public CharacterController Controller;
        public Transform CameraTransform;
        public Transform Transform;
        public float MoveSpeed;
        public float LookSpeed;
        public float JumpForce;
        public float MinLookAngle;
        public float MaxLookAngle;
    }

    private readonly Config _config;
    private readonly InputAction _moveAction;
    private readonly InputAction _lookAction;
    private readonly InputAction _jumpAction;

    private float _ySpeed;
    private float _horizontalRotation;
    private float _verticalRotation;

    public PlayerMovement(Config config, InputAction moveAction, InputAction lookAction, InputAction jumpAction)
    {
        _config = config;
        _moveAction = moveAction;
        _lookAction = lookAction;
        _jumpAction = jumpAction;
    }

    public void UpdateMovement(float deltaTime)
    {
        HandleMovement(deltaTime);
        HandleRotation(deltaTime);
    }

    private void HandleMovement(float deltaTime)
    {
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
        Vector3 moveDirection = CalculateMoveDirection(moveInput);

        ApplyGravity(deltaTime);
        HandleJump();

        moveDirection.y = _ySpeed;
        _config.Controller.Move(moveDirection * deltaTime);
    }

    private Vector3 CalculateMoveDirection(Vector2 input)
    {
        Vector3 forward = _config.Transform.forward * input.y;
        Vector3 right = _config.Transform.right * input.x;
        return (forward + right).normalized * _config.MoveSpeed;
    }

    private void ApplyGravity(float deltaTime)
    {
        if (_config.Controller.isGrounded)
        {
            _ySpeed = 0f;
        }
        _ySpeed += Physics.gravity.y * deltaTime;
    }

    private void HandleJump()
    {
        if (_jumpAction.IsPressed() && _config.Controller.isGrounded)
        {
            _ySpeed = _config.JumpForce;
        }
    }

    private void HandleRotation(float deltaTime)
    {
        Vector2 lookInput = _lookAction.ReadValue<Vector2>();

        _horizontalRotation += lookInput.x * deltaTime * _config.LookSpeed;
        _config.Transform.rotation = Quaternion.Euler(0f, _horizontalRotation, 0f);

        _verticalRotation -= lookInput.y * deltaTime * _config.LookSpeed;
        _verticalRotation = Mathf.Clamp(_verticalRotation, _config.MinLookAngle, _config.MaxLookAngle);
        _config.CameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }
}
#endregion

#region Interaction Component
public class PlayerInteraction
{
    public struct Config
    {
        public Camera camera;
        public float interactionRange;
        public float throwForce;
        public float waitToPlaceStock;
        public Transform stockHoldPoint;
        public Transform boxHoldPoint;
        public Transform furnitureHoldPoint;
        public LayerMask stockLayer;
        public LayerMask shelfLayer;
        public LayerMask priceLabelLayer;
        public LayerMask stockBoxLayer;
        public LayerMask trashLayer;
        public LayerMask furnitureLayer;
        public LayerMask checkoutLayer;
        public LayerMask storeSignLayer;
        public LayerMask interactableLayer;
    }

    private readonly Config _config;

    private StockObject _heldStock;
    private StockBoxController _heldBox;
    private FurnitureController _heldFurniture;
    private RaycastHit[] hits = new RaycastHit[PlayerController.Instance.MaxPossibleInteractableObjects];

    private float _placeStockTimer;
    private float _takeStockTimer;
    private bool _isFastPlacementActive;
    private bool _isFastTakeActive;
    private InputAction _interactAction;
    private InputAction _takeStockAction;

    public bool IsHoldingSomething => _heldStock != null || _heldBox != null || _heldFurniture != null;

    public PlayerInteraction(Config config)
    {
        _config = config;
    }

    public void SetInputActions(InputAction interactAction, InputAction takeStockAction)
    {
        _interactAction = interactAction;
        _takeStockAction = takeStockAction;
    }

    public void UpdateInteraction(float deltaTime)
    {
        if (_isFastPlacementActive)
        {
            ProcessFastPlacement(deltaTime);
        }

        if (_isFastTakeActive)
        {
            ProcessFastTake(deltaTime);
        }

        if (_heldFurniture != null)
        {
            KeepFurnitureAboveGround();
        }
    }

    #region Input Handlers
    public void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!IsHoldingSomething)
        {
            TryPickupObjectOrInteract();
        }
        else
        {
            TryPlaceOrUseHeldObject();
        }
    }

    public void OnInteractCanceled(InputAction.CallbackContext context)
    {
        if (_heldBox != null && _isFastPlacementActive)
        {
            _isFastPlacementActive = false;
        }
    }

    public void OnDropPerformed(InputAction.CallbackContext context)
    {
        if (_heldStock != null)
        {
            _heldStock.Release();
            _heldStock.Rb.AddForce(_config.camera.transform.forward * _config.throwForce, ForceMode.Impulse);
            _heldStock.transform.SetParent(null);
            _heldStock = null;
        }
        else if (_heldBox != null)
        {
            _heldBox.Release();
            _heldBox.Rb.AddForce(_config.camera.transform.forward * _config.throwForce, ForceMode.Impulse);
            _heldBox.transform.SetParent(null);
            _heldBox = null;
        }
    }

    public void OnOpenBoxPerformed(InputAction.CallbackContext context)
    {
        if (_heldBox != null)
        {
            _heldBox.OpenClose();
            return;
        }

        //if (TryRaycast(_config.shelfLayer, out RaycastHit hit))
        //{
        //    if (hit.collider.TryGetComponent(out StockBoxController box))
        //    {
        //        box.OpenClose();
        //    }
        //}
    }

    public void OnPickupFurniturePerformed(InputAction.CallbackContext context)
    {
        if (_heldFurniture != null)
        {
            PlaceFurniture();
            return;
        }

        IInteractable interactable;
        (interactable, _) = TryRaycastForInteractable();
        if (interactable == null) return;

        if (interactable.MyObject.TryGetComponent(out FurnitureController _))
        {
            PickupFurniture(hits[0]);
        }
    }

    public void OnTakeStockPerformed(InputAction.CallbackContext context)
    {
        // Only allow taking stock when holding a box
        if (_heldBox == null) return;

        IInteractable interactable;
        (interactable, _) = TryRaycastForInteractable();

        RaycastHit closestHit = default;
        float minDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.TryGetComponent(out ShelfSpaceController _)) continue;

            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;
                break;
            }
        }

        if (interactable == null) return;

        if (closestHit.collider.TryGetComponent(out ShelfSpaceController shelf))
        {
            TryTakeStockFromShelfIntoBox(shelf);
            _takeStockTimer = _config.waitToPlaceStock;
            _isFastTakeActive = true;
        }
    }

    public void OnTakeStockCanceled(InputAction.CallbackContext context)
    {
        if (_heldBox != null && _isFastTakeActive)
        {
            _isFastTakeActive = false;
        }
    }
    #endregion

    #region Pickup Logic
    private void TryPickupObjectOrInteract()
    {
        ComponentChecks();
        //if (TryInteractWithPriceLabel()) return;
        //if (TryPickupStock()) return;
        //if (TryPickupStockBox()) return;
        //if (TryTakeStockFromShelf()) return;
        //if (TryInteractWithCheckout()) return;
        //if (TryInteractWithStoreSign()) return;
    }

    private void ComponentChecks()
    {
        //IInteractable interactable;
        //Transform temp;
        //(interactable, temp) = TryRaycastForInteractable();

        RaycastHit[] tempHits = TryRaycastForInteractables();

        RaycastHit closestHit = default;
        float minDistance = float.MaxValue;

        if (tempHits == null) return;

        foreach (RaycastHit hit in tempHits)
        {
            if (!hit.collider.TryGetComponent(out StockObject _)) continue;
            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;

                closestHit.collider.GetComponent<IInteractable>().OnInteract(_config.stockHoldPoint);
                _heldStock = closestHit.collider.GetComponent<StockObject>();
                return;
            }
        }

        foreach (RaycastHit hit in tempHits)
        {
            if (!hit.collider.TryGetComponent(out ShelfSpaceController _)) continue;
            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;

                _heldStock = closestHit.collider.GetComponent<ShelfSpaceController>().GetStock();
                _heldStock.OnInteract(_config.stockHoldPoint);
                return;
            }
        }

        foreach (RaycastHit hit in tempHits)
        {
            if (!hit.collider.TryGetComponent(out StockBoxController _)) continue;
            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;

                closestHit.collider.GetComponent<IInteractable>().OnInteract(_config.boxHoldPoint);
                _heldBox = closestHit.collider.GetComponent<StockBoxController>();
                return;
            }
        }

        foreach (RaycastHit hit in tempHits)
        {
            if (!hit.collider.TryGetComponent(out Checkout _)) continue;
            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;

                closestHit.collider.GetComponent<IInteractable>().OnInteract();
                return;
            }
        }

        foreach (RaycastHit hit in tempHits)
        {
            if (!hit.collider.TryGetComponent(out StoreSign _)) continue;
            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;

                closestHit.collider.GetComponent<IInteractable>().OnInteract();
                return;
            }
        }
    }

    //private bool TryPickupStock()
    //{
    //    if (TryRaycast(_config.stockLayer, out RaycastHit hit))
    //    {
    //        _heldStock = hit.collider.GetComponent<StockObject>();
    //        if (_heldStock != null)
    //        {
    //            _heldStock.Pickup(_config.stockHoldPoint);
    //        }
    //        return _heldStock != null;
    //    }
    //    return false;
    //}

    //private bool TryPickupStockBox()
    //{
    //    if (TryRaycast(_config.stockBoxLayer, out RaycastHit hit))
    //    {
    //        _heldBox = hit.collider.GetComponent<StockBoxController>();
    //        if (_heldBox != null)
    //        {
    //            _heldBox.Pickup(_config.boxHoldPoint);

    //            if (!_heldBox.OpenBox)
    //            {
    //                _heldBox.OpenClose();
    //            }
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //private bool TryTakeStockFromShelf()
    //{
    //    if (TryRaycast(_config.shelfLayer, out RaycastHit hit))
    //    {
    //        if (hit.collider.TryGetComponent(out ShelfSpaceController shelf))
    //        {
    //            _heldStock = shelf.GetStock();
    //        }

    //        if (_heldStock != null)
    //        {
    //            _heldStock.Pickup(_config.stockHoldPoint);
    //            return true;
    //        }
    //    }
    //    return false;
    //}

    //private bool TryInteractWithPriceLabel()
    //{
    //    if (TryRaycast(_config.priceLabelLayer, out RaycastHit hit))
    //    {
    //        var shelf = hit.collider.GetComponentInParent<ShelfSpaceController>();
    //        if (shelf != null)
    //        {
    //            shelf.StartPriceUpdate();
    //        }
    //        return true;
    //    }
    //    return false;
    //}

    //private bool TryInteractWithCheckout()
    //{
    //    if (TryRaycast(_config.checkoutLayer, out RaycastHit hit))
    //    {
    //        if (hit.collider.TryGetComponent(out Checkout checkout))
    //        {
    //            checkout.CheckoutCustomer();
    //        }
    //        return true;
    //    }
    //    return false;
    //}

    private bool TryInteractWithStoreSign() // TODO: Allow Player to change the name of the Store
    {
        IInteractable interactable;
        Transform temp;
        (interactable, temp) = TryRaycastForInteractable();
        if (interactable == null || temp == null) return false;

        if (interactable.MyObject.TryGetComponent(out StoreSign storeSign))
        {
            return true;
        }
        return false;
    }
    #endregion

    #region Place/Use Logic
    private void TryPlaceOrUseHeldObject()
    {
        IInteractable interactable;
        (interactable, _) = TryRaycastForInteractable();
        if (interactable == null) return;

        if (_heldStock != null)
        {
            HandleHeldStock(interactable);
        }
        else if (_heldBox != null)
        {
            RaycastHit closestHit = default;
            float minDistance = float.MaxValue;
            foreach (RaycastHit hit in hits)
            {
                if (!hit.collider.TryGetComponent(out ShelfSpaceController _)) continue;

                if (hit.distance < minDistance)
                {
                    minDistance = hit.distance;
                    closestHit = hit;
                    break;
                }
            }

            HandleHeldBox(closestHit.collider.GetComponent<IInteractable>());
        }
        else if (_heldFurniture != null)
        {
            PlaceFurniture();
        }
    }

    private void HandleHeldStock(IInteractable interactable)
    {
        if (interactable.MyObject.TryGetComponent(out ShelfSpaceController shelf))
        {
            shelf.PlaceStock(_heldStock);

            if (_heldStock.IsPlaced) _heldStock = null;
        }
        else if (interactable.MyObject.TryGetComponent(out StockBoxController stockBox))
        {
            if (stockBox.CanTakeStockFromHand(_heldStock)) _heldStock = null;
        }
        else if (interactable.MyObject.TryGetComponent(out TrashCan _))
        {
            _heldStock.TrashObject();
            _heldStock = null;
        }
    }

    private void HandleHeldBox(IInteractable interactable)
    {
        // Add null check here
        if (interactable == null) return;

        if (_heldBox.StockInBox.Count > 0)
        {
            if (interactable.MyObject.TryGetComponent(out ShelfSpaceController shelf))
            {
                _heldBox.PlaceStockOnShelf(shelf);

                _placeStockTimer = _config.waitToPlaceStock;
                _isFastPlacementActive = true;
            }
            return;
        }

        if (interactable.MyObject.TryGetComponent(out TrashCan _))
        {
            _heldBox.Release();
            _heldBox.TrashObject();
            _heldBox = null;
        }
    }
    #endregion

    #region Take Stock Into Box
    private void TryTakeStockFromShelfIntoBox(ShelfSpaceController shelf)
    {
        if (_heldBox == null || shelf == null || shelf.StockInfo == null) return;

        // Check if the shelf has any stock
        if (shelf.ObjectsOnShelf == null || shelf.ObjectsOnShelf.Count == 0) return;

        // Check if box is empty OR if the stock types match
        bool canTakeStock = _heldBox.StockInBox.Count >= 0
                            && _heldBox.StockInBox.Count < _heldBox.MaxCapacity
                            && _heldBox.StockInfo != null
                            && _heldBox.StockInfo.name == shelf.StockInfo.name;

        if (!canTakeStock)
        {
            return;
        }

        // Take stock from shelf
        StockObject stockFromShelf = shelf.GetStock();

        if (stockFromShelf != null)
        {
            _heldBox.TakeStockFromShelf(stockFromShelf);
        }
    }
    #endregion

    #region Fast Placement
    private void ProcessFastPlacement(float deltaTime)
    {
        IInteractable interactable;
        Transform temp;
        (interactable, temp) = TryRaycastForInteractable();

        RaycastHit closestHit = default;
        float minDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.TryGetComponent(out ShelfSpaceController _)) continue;

            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;
                break;
            }
        }

        if (interactable == null || temp == null || _heldStock != null || _heldBox == null)
        {
            _isFastPlacementActive = false;
            return;
        }

        if (closestHit.collider.TryGetComponent(out ShelfSpaceController shelf))
        {
            _placeStockTimer -= deltaTime;

            if (_placeStockTimer <= 0f && _interactAction != null && _interactAction.IsPressed())
            {
                _heldBox.PlaceStockOnShelf(shelf);
                _placeStockTimer = _config.waitToPlaceStock;
            }
        }
        else
        {
            _isFastPlacementActive = false;
        }
    }
    #endregion

    #region Fast Take
    private void ProcessFastTake(float deltaTime)
    {
        IInteractable interactable;
        Transform temp;
        (interactable, temp) = TryRaycastForInteractable();

        RaycastHit closestHit = default;
        float minDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            if (!hit.collider.TryGetComponent(out ShelfSpaceController _)) continue;

            if (hit.distance < minDistance)
            {
                minDistance = hit.distance;
                closestHit = hit;
                break;
            }
        }

        if (interactable == null || temp == null || _heldStock != null || _heldBox == null)
        {
            _isFastTakeActive = false;
            return;
        }

        if (closestHit.collider.TryGetComponent(out ShelfSpaceController shelf))
        {
            _takeStockTimer -= deltaTime;

            if (_takeStockTimer <= 0f && _takeStockAction != null && _takeStockAction.IsPressed())
            {
                TryTakeStockFromShelfIntoBox(shelf);
                _takeStockTimer = _config.waitToPlaceStock;
            }
        }
        else
        {
            _isFastTakeActive = false;
        }
    }
    #endregion

    #region Furniture Methods
    private void PickupFurniture(RaycastHit hit)
    {
        _heldFurniture = hit.transform.GetComponent<FurnitureController>();
        if (_heldFurniture == null) return;

        hit.collider.enabled = false;
        _heldFurniture.transform.SetParent(_config.furnitureHoldPoint);
        _heldFurniture.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _heldFurniture.MakePlaceable();
    }

    private void PlaceFurniture()
    {
        if (_heldFurniture == null) return;

        _heldFurniture.PlaceObject();
        _heldFurniture = null;
    }

    private void KeepFurnitureAboveGround()
    {
        Vector3 holdPosition = _config.furnitureHoldPoint.position;
        Vector3 playerPosition = _config.furnitureHoldPoint.parent.position;

        _heldFurniture.transform.position = new Vector3(holdPosition.x, 0f, holdPosition.z);
        _heldFurniture.transform.LookAt(new Vector3(playerPosition.x, 0f, playerPosition.z));
    }
    #endregion

    #region Utility Methods
    private bool TryRaycast(LayerMask layerMask, out RaycastHit hit)
    {
        Ray ray = _config.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Physics.Raycast(ray, out hit, _config.interactionRange, layerMask);
    }

    private (IInteractable, Transform) TryRaycastForInteractable()
    {
        Ray ray = _config.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        int numOfHits = Physics.RaycastNonAlloc(ray, hits, _config.interactionRange, _config.interactableLayer);
        if (numOfHits == 0) return (null, null);
        Transform temp = hits[0].transform;
        IInteractable interactable = hits[0].transform.GetComponent<IInteractable>();
        return (interactable, temp);
    }

    private RaycastHit[] TryRaycastForInteractables()
    {
        Ray ray = _config.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] temp = new RaycastHit[Physics.RaycastNonAlloc(ray, hits, _config.interactionRange, _config.interactableLayer)];

        return temp;
    }
    #endregion
}
#endregion
#endregion

#region Claude Code v1
//using masonbell;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PlayerController : MonoBehaviour
//{
//    public static PlayerController Instance { get; private set; }

//    #region Events
//    public static event Action OnUIPanelClosedWithCancelAction;
//    #endregion

//    #region Serialized Fields
//    [Header("References")]
//    [SerializeField] private CharacterController _controller;
//    [SerializeField] private Camera _camera;

//    [Header("Movement Settings")]
//    [SerializeField] private float _moveSpeed = 5f;
//    [SerializeField] private float _lookSpeed = 2f;
//    [SerializeField] private float _jumpForce = 5f;
//    [SerializeField] private float _minLookAngle = -80f;
//    [SerializeField] private float _maxLookAngle = 80f;

//    [Header("Interaction Settings")]
//    [SerializeField] private float _interactionRange = 3f;
//    [SerializeField] private float _throwForce = 10f;
//    [SerializeField] private float _waitToPlaceStock = 0.2f;
//    [SerializeField] private Transform _stockHoldPoint;
//    [SerializeField] private Transform _boxHoldPoint;
//    [SerializeField] private Transform _furnitureHoldPoint;

//    [Header("Interaction Layers")]
//    [SerializeField] private LayerMask _stockLayer;
//    [SerializeField] private LayerMask _shelfLayer;
//    [SerializeField] private LayerMask _priceLabelLayer;
//    [SerializeField] private LayerMask _stockBoxLayer;
//    [SerializeField] private LayerMask _trashLayer;
//    [SerializeField] private LayerMask _furnitureLayer;
//    [SerializeField] private LayerMask _checkoutLayer;
//    #endregion

//    #region Private Fields
//    private InputSystem_Actions _gameInput;
//    private PlayerMovement _movement;
//    private PlayerInteraction _interaction;

//    private InputAction _moveAction;
//    private InputAction _lookAction;
//    private InputAction _jumpAction;
//    private InputAction _interactAction;
//    private InputAction _dropAction;
//    private InputAction _openBoxAction;
//    private InputAction _pickupFurnitureAction;
//    private InputAction _submitAction;
//    private InputAction _cancelAction;
//    #endregion

//    #region Properties
//    public bool IsHoldingSomething => _interaction.IsHoldingSomething;
//    #endregion

//    #region Unity Lifecycle
//    private void Awake()
//    {
//        InitializeSingleton();
//        InitializeInput();
//        InitializeComponents();
//    }

//    private void OnEnable()
//    {
//        SubscribeToInputEvents();
//        UIController.OnUIPanelClosed += DisableUIEnablePlayer;
//    }

//    private void Start()
//    {
//        Cursor.lockState = CursorLockMode.Locked;
//    }

//    private void Update()
//    {
//        float deltaTime = Time.deltaTime;
//        _movement.UpdateMovement(deltaTime);
//        _interaction.UpdateInteraction(deltaTime);
//    }

//    private void OnDisable()
//    {
//        UnsubscribeFromInputEvents();
//        UIController.OnUIPanelClosed -= DisableUIEnablePlayer;

//        _gameInput?.UI.Disable();
//        _gameInput?.Player.Disable();
//    }
//    #endregion

//    #region Initialization
//    private void InitializeSingleton()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }
//        Instance = this;
//    }

//    private void InitializeInput()
//    {
//        _gameInput = new InputSystem_Actions();
//        _gameInput.Enable();

//        _moveAction = _gameInput.Player.Move;
//        _lookAction = _gameInput.Player.Look;
//        _jumpAction = _gameInput.Player.Jump;
//        _interactAction = _gameInput.Player.Interact;
//        _dropAction = _gameInput.Player.DropHeldItem;
//        _openBoxAction = _gameInput.Player.OpenBox;
//        _pickupFurnitureAction = _gameInput.Player.PickupFurniture;
//        _submitAction = _gameInput.UI.Submit;
//        _cancelAction = _gameInput.UI.Cancel;
//    }

//    private void InitializeComponents()
//    {
//        var movementConfig = new PlayerMovement.Config
//        {
//            Controller = _controller,
//            CameraTransform = _camera.transform,
//            Transform = transform,
//            MoveSpeed = _moveSpeed,
//            LookSpeed = _lookSpeed,
//            JumpForce = _jumpForce,
//            MinLookAngle = _minLookAngle,
//            MaxLookAngle = _maxLookAngle
//        };
//        _movement = new PlayerMovement(movementConfig, _moveAction, _lookAction, _jumpAction);

//        var interactionConfig = new PlayerInteraction.Config
//        {
//            Camera = _camera,
//            InteractionRange = _interactionRange,
//            ThrowForce = _throwForce,
//            WaitToPlaceStock = _waitToPlaceStock,
//            StockHoldPoint = _stockHoldPoint,
//            BoxHoldPoint = _boxHoldPoint,
//            FurnitureHoldPoint = _furnitureHoldPoint,
//            StockLayer = _stockLayer,
//            ShelfLayer = _shelfLayer,
//            PriceLabelLayer = _priceLabelLayer,
//            StockBoxLayer = _stockBoxLayer,
//            TrashLayer = _trashLayer,
//            FurnitureLayer = _furnitureLayer,
//            CheckoutLayer = _checkoutLayer
//        };
//        _interaction = new PlayerInteraction(interactionConfig);
//        _interaction.SetInteractAction(_interactAction);
//    }
//    #endregion

//    #region Input Event Subscriptions
//    private void SubscribeToInputEvents()
//    {
//        _interactAction.performed += _interaction.OnInteractPerformed;
//        _interactAction.canceled += _interaction.OnInteractCanceled;
//        _dropAction.performed += _interaction.OnDropPerformed;
//        _openBoxAction.performed += _interaction.OnOpenBoxPerformed;
//        _pickupFurnitureAction.performed += _interaction.OnPickupFurniturePerformed;
//        _submitAction.performed += OnSubmitPerformed;
//        _cancelAction.performed += OnCancelPerformed;
//    }

//    private void UnsubscribeFromInputEvents()
//    {
//        _interactAction.performed -= _interaction.OnInteractPerformed;
//        _interactAction.canceled -= _interaction.OnInteractCanceled;
//        _dropAction.performed -= _interaction.OnDropPerformed;
//        _openBoxAction.performed -= _interaction.OnOpenBoxPerformed;
//        _pickupFurnitureAction.performed -= _interaction.OnPickupFurniturePerformed;
//        _submitAction.performed -= OnSubmitPerformed;
//        _cancelAction.performed -= OnCancelPerformed;
//    }
//    #endregion

//    #region Public Methods
//    public void DisableUIEnablePlayer()
//    {
//        _gameInput.UI.Disable();
//        _gameInput.Player.Enable();
//        Cursor.lockState = CursorLockMode.Locked;
//    }

//    public void DisablePlayerEnableUI()
//    {
//        _gameInput.Player.Disable();
//        _gameInput.UI.Enable();
//        Cursor.lockState = CursorLockMode.None;
//    }
//    #endregion

//    #region Input Callbacks
//    private void OnSubmitPerformed(InputAction.CallbackContext context)
//    {
//        if (UIController.Instance != null)
//        {
//            UIController.Instance.ApplyPriceUpdate();
//        }
//    }

//    private void OnCancelPerformed(InputAction.CallbackContext context)
//    {
//        OnUIPanelClosedWithCancelAction?.Invoke();
//        DisableUIEnablePlayer();
//    }
//    #endregion
//}

//#region Movement Component
//public class PlayerMovement
//{
//    public struct Config
//    {
//        public CharacterController Controller;
//        public Transform CameraTransform;
//        public Transform Transform;
//        public float MoveSpeed;
//        public float LookSpeed;
//        public float JumpForce;
//        public float MinLookAngle;
//        public float MaxLookAngle;
//    }

//    private readonly Config _config;
//    private readonly InputAction _moveAction;
//    private readonly InputAction _lookAction;
//    private readonly InputAction _jumpAction;

//    private float _ySpeed;
//    private float _horizontalRotation;
//    private float _verticalRotation;

//    public PlayerMovement(Config config, InputAction moveAction, InputAction lookAction, InputAction jumpAction)
//    {
//        _config = config;
//        _moveAction = moveAction;
//        _lookAction = lookAction;
//        _jumpAction = jumpAction;
//    }

//    public void UpdateMovement(float deltaTime)
//    {
//        HandleMovement(deltaTime);
//        HandleRotation(deltaTime);
//    }

//    private void HandleMovement(float deltaTime)
//    {
//        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
//        Vector3 moveDirection = CalculateMoveDirection(moveInput);

//        ApplyGravity(deltaTime);
//        HandleJump();

//        moveDirection.y = _ySpeed;
//        _config.Controller.Move(moveDirection * deltaTime);
//    }

//    private Vector3 CalculateMoveDirection(Vector2 input)
//    {
//        Vector3 forward = _config.Transform.forward * input.y;
//        Vector3 right = _config.Transform.right * input.x;
//        return (forward + right).normalized * _config.MoveSpeed;
//    }

//    private void ApplyGravity(float deltaTime)
//    {
//        if (_config.Controller.isGrounded)
//        {
//            _ySpeed = 0f;
//        }
//        _ySpeed += Physics.gravity.y * deltaTime;
//    }

//    private void HandleJump()
//    {
//        if (_jumpAction.IsPressed() && _config.Controller.isGrounded)
//        {
//            _ySpeed = _config.JumpForce;
//        }
//    }

//    private void HandleRotation(float deltaTime)
//    {
//        Vector2 lookInput = _lookAction.ReadValue<Vector2>();

//        _horizontalRotation += lookInput.x * deltaTime * _config.LookSpeed;
//        _config.Transform.rotation = Quaternion.Euler(0f, _horizontalRotation, 0f);

//        _verticalRotation -= lookInput.y * deltaTime * _config.LookSpeed;
//        _verticalRotation = Mathf.Clamp(_verticalRotation, _config.MinLookAngle, _config.MaxLookAngle);
//        _config.CameraTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
//    }
//}
//#endregion

//#region Interaction Component
//public class PlayerInteraction
//{
//    public struct Config
//    {
//        public Camera Camera;
//        public float InteractionRange;
//        public float ThrowForce;
//        public float WaitToPlaceStock;
//        public Transform StockHoldPoint;
//        public Transform BoxHoldPoint;
//        public Transform FurnitureHoldPoint;
//        public LayerMask StockLayer;
//        public LayerMask ShelfLayer;
//        public LayerMask PriceLabelLayer;
//        public LayerMask StockBoxLayer;
//        public LayerMask TrashLayer;
//        public LayerMask FurnitureLayer;
//        public LayerMask CheckoutLayer;
//    }

//    private readonly Config _config;

//    private StockObject _heldStock;
//    private StockBoxController _heldBox;
//    private FurnitureController _heldFurniture;

//    private float _placeStockTimer;
//    private bool _isFastPlacementActive;
//    private InputAction _interactAction;

//    public bool IsHoldingSomething => _heldStock != null || _heldBox != null || _heldFurniture != null;

//    public PlayerInteraction(Config config)
//    {
//        _config = config;
//    }

//    public void SetInteractAction(InputAction interactAction)
//    {
//        _interactAction = interactAction;
//    }

//    public void UpdateInteraction(float deltaTime)
//    {
//        if (_isFastPlacementActive)
//        {
//            ProcessFastPlacement(deltaTime);
//        }

//        if (_heldFurniture != null)
//        {
//            KeepFurnitureAboveGround();
//        }
//    }

//    #region Input Handlers
//    public void OnInteractPerformed(InputAction.CallbackContext context)
//    {
//        if (!IsHoldingSomething)
//        {
//            TryPickupObject();
//        }
//        else
//        {
//            TryPlaceOrUseHeldObject();
//        }
//    }

//    public void OnInteractCanceled(InputAction.CallbackContext context)
//    {
//        if (_heldBox != null && _isFastPlacementActive)
//        {
//            _isFastPlacementActive = false;
//        }
//    }

//    public void OnDropPerformed(InputAction.CallbackContext context)
//    {
//        if (_heldStock != null)
//        {
//            _heldStock.Release();
//            _heldStock.Rb.AddForce(_config.Camera.transform.forward * _config.ThrowForce, ForceMode.Impulse);
//            _heldStock.transform.SetParent(null);
//            _heldStock = null;
//        }
//        else if (_heldBox != null)
//        {
//            _heldBox.Release();
//            _heldBox.Rb.AddForce(_config.Camera.transform.forward * _config.ThrowForce, ForceMode.Impulse);
//            _heldBox.transform.SetParent(null);
//            _heldBox = null;
//        }
//    }

//    public void OnOpenBoxPerformed(InputAction.CallbackContext context)
//    {
//        if (_heldBox != null)
//        {
//            _heldBox.OpenClose();
//            return;
//        }

//        if (TryRaycast(_config.StockBoxLayer, out RaycastHit hit))
//        {
//            if (hit.collider.TryGetComponent(out StockBoxController box))
//            {
//                box.OpenClose();
//            }
//        }
//    }

//    public void OnPickupFurniturePerformed(InputAction.CallbackContext context)
//    {
//        if (_heldFurniture != null)
//        {
//            PlaceFurniture();
//            return;
//        }

//        if (TryRaycast(_config.FurnitureLayer, out RaycastHit hit))
//        {
//            PickupFurniture(hit);
//        }
//    }
//    #endregion

//    #region Pickup Logic
//    private void TryPickupObject()
//    {
//        if (TryInteractWithPriceLabel()) return;
//        if (TryPickupStock()) return;
//        if (TryPickupStockBox()) return;
//        if (TryTakeStockFromShelf()) return;
//        if (TryInteractWithCheckout()) return;
//    }

//    private bool TryPickupStock()
//    {
//        if (TryRaycast(_config.StockLayer, out RaycastHit hit))
//        {
//            _heldStock = hit.collider.GetComponent<StockObject>();
//            if (_heldStock != null)
//            {
//                _heldStock.Pickup(_config.StockHoldPoint);
//            }
//            return _heldStock != null;
//        }
//        return false;
//    }

//    private bool TryPickupStockBox()
//    {
//        if (TryRaycast(_config.StockBoxLayer, out RaycastHit hit))
//        {
//            _heldBox = hit.collider.GetComponent<StockBoxController>();
//            if (_heldBox != null)
//            {
//                _heldBox.Pickup(_config.BoxHoldPoint);

//                if (!_heldBox.OpenBox)
//                {
//                    _heldBox.OpenClose();
//                }
//                return true;
//            }
//        }
//        return false;
//    }

//    private bool TryTakeStockFromShelf()
//    {
//        if (TryRaycast(_config.ShelfLayer, out RaycastHit hit))
//        {
//            if (hit.collider.TryGetComponent(out ShelfSpaceController shelf))
//            {
//                _heldStock = shelf.GetStock();
//            }

//            if (_heldStock != null)
//            {
//                _heldStock.Pickup(_config.StockHoldPoint);
//                return true;
//            }
//        }
//        return false;
//    }

//    private bool TryInteractWithPriceLabel()
//    {
//        if (TryRaycast(_config.PriceLabelLayer, out RaycastHit hit))
//        {
//            var shelf = hit.collider.GetComponentInParent<ShelfSpaceController>();
//            if (shelf != null)
//            {
//                shelf.StartPriceUpdate();
//            }
//            return true;
//        }
//        return false;
//    }

//    private bool TryInteractWithCheckout()
//    {
//        if (TryRaycast(_config.CheckoutLayer, out RaycastHit hit))
//        {
//            if (hit.collider.TryGetComponent(out Checkout checkout))
//            {
//                checkout.CheckoutCustomer();
//            }
//            return true;
//        }
//        return false;
//    }
//    #endregion

//    #region Place/Use Logic
//    private void TryPlaceOrUseHeldObject()
//    {
//        if (_heldStock != null)
//        {
//            HandleHeldStock();
//        }
//        else if (_heldBox != null)
//        {
//            HandleHeldBox();
//        }
//        else if (_heldFurniture != null)
//        {
//            PlaceFurniture();
//        }
//    }

//    private void HandleHeldStock()
//    {
//        if (TryRaycast(_config.ShelfLayer, out RaycastHit hit))
//        {
//            if (hit.collider.TryGetComponent(out ShelfSpaceController shelf))
//            {
//                shelf.PlaceStock(_heldStock);
//            }

//            if (_heldStock.IsPlaced)
//            {
//                _heldStock = null;
//            }
//            return;
//        }

//        if (TryRaycast(_config.TrashLayer, out _))
//        {
//            _heldStock.TrashObject();
//            _heldStock = null;
//        }
//    }

//    private void HandleHeldBox()
//    {
//        if (_heldBox.StockInBox.Count > 0)
//        {
//            if (TryRaycast(_config.ShelfLayer, out RaycastHit hit))
//            {
//                var shelf = hit.collider.GetComponent<ShelfSpaceController>();
//                _heldBox.PlaceStockOnShelf(shelf);
//                _placeStockTimer = _config.WaitToPlaceStock;
//                _isFastPlacementActive = true;
//            }
//        }
//        else
//        {
//            if (TryRaycast(_config.TrashLayer, out _))
//            {
//                _heldBox.Release();
//                _heldBox.TrashObject();
//                _heldBox = null;
//            }
//        }
//    }
//    #endregion

//    #region Fast Placement
//    private void ProcessFastPlacement(float deltaTime)
//    {
//        if (_heldStock != null || _heldBox == null)
//        {
//            _isFastPlacementActive = false;
//            return;
//        }

//        if (TryRaycast(_config.ShelfLayer, out RaycastHit hit))
//        {
//            _placeStockTimer -= deltaTime;

//            if (_placeStockTimer <= 0f && _interactAction != null && _interactAction.IsPressed())
//            {
//                var shelf = hit.collider.GetComponent<ShelfSpaceController>();
//                _heldBox.PlaceStockOnShelf(shelf);
//                _placeStockTimer = _config.WaitToPlaceStock;
//            }
//        }
//        else
//        {
//            _isFastPlacementActive = false;
//        }
//    }
//    #endregion

//    #region Furniture Methods
//    private void PickupFurniture(RaycastHit hit)
//    {
//        _heldFurniture = hit.transform.GetComponent<FurnitureController>();
//        if (_heldFurniture == null) return;

//        hit.collider.enabled = false;
//        _heldFurniture.transform.SetParent(_config.FurnitureHoldPoint);
//        _heldFurniture.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//        _heldFurniture.MakePlaceable();
//    }

//    private void PlaceFurniture()
//    {
//        if (_heldFurniture == null) return;

//        _heldFurniture.PlaceObject();
//        _heldFurniture = null;
//    }

//    private void KeepFurnitureAboveGround()
//    {
//        Vector3 holdPosition = _config.FurnitureHoldPoint.position;
//        Vector3 playerPosition = _config.FurnitureHoldPoint.parent.position;

//        _heldFurniture.transform.position = new Vector3(holdPosition.x, 0f, holdPosition.z);
//        _heldFurniture.transform.LookAt(new Vector3(playerPosition.x, 0f, playerPosition.z));
//    }
//    #endregion

//    #region Utility Methods
//    private bool TryRaycast(LayerMask layerMask, out RaycastHit hit)
//    {
//        Ray ray = _config.Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
//        return Physics.Raycast(ray, out hit, _config.InteractionRange, layerMask);
//    }
//    #endregion
//}
//#endregion
#endregion

#region James' Code
//using masonbell;
//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class PlayerController : MonoBehaviour
//{
//    public static PlayerController Instance { get; private set; }

//    #region Event Fields
//    public static event Action OnUIPanelClosedWithCancelAction;
//    #endregion

//    #region Public Fields
//    #endregion

//    #region Serialized Private Fields
//    [Header("References")]
//    [SerializeField] private CharacterController controller;
//    [SerializeField] private Camera cameraTransform;

//    [Header("Interaction")]
//    [SerializeField] private Transform _holdPoint;

//    [SerializeField] private StockBoxController heldBox;
//    [SerializeField] private Transform boxHoldPoint;

//    [SerializeField] private FurnitureController _heldFurniture;
//    [SerializeField] private Transform _furnitureHoldPoint;

//    [SerializeField] private LayerMask whatIsStock;
//    [SerializeField] private LayerMask whatIsShelf;
//    [SerializeField] private LayerMask whatIsPriceLabel;
//    [SerializeField] private LayerMask whatIsStockBox;
//    [SerializeField] private LayerMask whatIsTrash;
//    [SerializeField] private LayerMask whatIsFurniture;
//    [SerializeField] private LayerMask whatIsCheckout;

//    [SerializeField] private float interactionRange;
//    [SerializeField] private float throwForce;

//    [Header("Movement")]
//    [SerializeField] private float moveSpeed;
//    [SerializeField] private float lookSpeed;
//    [SerializeField] private float jumpForce;
//    [SerializeField] private float _minLookAngle;
//    [SerializeField] private float _maxLookAngle;
//    #endregion

//    #region Private Fields
//    [Header("Input")]
//    private InputSystem_Actions _gameInput;
//    private InputAction _playerMapMoveAction;
//    private InputAction _playerMapLookAction;
//    private InputAction _playerMapJumpAction;
//    private InputAction _playerMapInteractAction;
//    private InputAction _playerMapDropAction;
//    private InputAction _playerMapOpenBoxAction;
//    private InputAction _playerMapPickupFurnitureAction;

//    private InputAction _uiMapSubmitAction;
//    private InputAction _uiMapCancelAction;

//    [Header("Interaction")]
//    private StockObject _heldObject;
//    private float _placeStockCounter;
//    private bool _canPlaceBoxObjectFast;

//    [Header("Movement")]
//    private float _ySpeed;
//    private float _horizontalRotation;
//    private float _verticalRotation;
//    #endregion

//    #region Public Properties

//    [field: SerializeField, Header("Properties")] public float WaitToPlaceStock { get; private set; }

//    public bool HasHeldObject => _heldObject != null;
//    #endregion

//    #region Unity Callbacks
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//            Destroy(gameObject);
//        else
//            Instance = this;

//            _gameInput = new();
//        _gameInput.Enable();
//        #region Player Map Actions
//        _playerMapMoveAction = _gameInput.Player.Move;
//        _playerMapLookAction = _gameInput.Player.Look;   
//        _playerMapJumpAction = _gameInput.Player.Jump;
//        _playerMapInteractAction = _gameInput.Player.Interact;
//        _playerMapDropAction = _gameInput.Player.DropHeldItem;
//        _playerMapOpenBoxAction = _gameInput.Player.OpenBox;
//        _playerMapPickupFurnitureAction = _gameInput.Player.PickupFurniture;
//        #endregion

//        #region UI Map Actions
//        _uiMapSubmitAction = _gameInput.UI.Submit;
//        _uiMapCancelAction = _gameInput.UI.Cancel;
//        #endregion
//    }

//    private void OnEnable()
//    {
//        _playerMapInteractAction.performed += CheckForInteraction;
//        _playerMapInteractAction.canceled += CheckForInteractionCancel;
//        _playerMapDropAction.performed += CheckForDrop;
//        _playerMapOpenBoxAction.performed += CheckForOpenCloseBox;
//        _playerMapPickupFurnitureAction.performed += CheckForFurniturePickup;

//        _uiMapSubmitAction.performed += UIApplyWithSubmit;
//        _uiMapCancelAction.performed += UICloseWithCancelAction;
//        UIController.OnUIPanelClosed += DisableUIEnablePlayer;
//    }

//    private void Start()
//    {
//        Cursor.lockState = CursorLockMode.Locked;
//    }

//    private void Update()
//    {
//        Move();
//        Rotate();

//        if (_canPlaceBoxObjectFast)
//        {
//            PlaceBoxObjectsFast();
//        }

//        if (_heldFurniture != null)
//            KeepHeldFurnitureAboveGround(); // TODO: Maybe come up with a better way to do this?
//    }

//    private void OnDisable()
//    {
//        _playerMapInteractAction.performed -= CheckForInteraction;
//        _playerMapInteractAction.canceled -= CheckForInteractionCancel;
//        _playerMapDropAction.performed -= CheckForDrop;
//        _playerMapOpenBoxAction.performed -= CheckForOpenCloseBox;

//        _uiMapSubmitAction.performed -= UIApplyWithSubmit;
//        _uiMapCancelAction.performed -= UICloseWithCancelAction;
//        UIController.OnUIPanelClosed -= DisableUIEnablePlayer;

//        _gameInput.UI.Disable();
//        _gameInput.Player.Disable();
//    }
//    #endregion

//    #region Public Methods
//    public void DisableUIEnablePlayer()
//    {
//        _gameInput.UI.Disable();
//        _gameInput.Player.Enable();
//        Cursor.lockState = CursorLockMode.Locked;
//    }

//    public void DisablePlayerEnableUI()
//    {
//        _gameInput.Player.Disable();
//        _gameInput.UI.Enable();
//        Cursor.lockState = CursorLockMode.None;
//    }
//    #endregion

//    #region Private Methods
//    private void Move()
//    {
//        Vector2 moveInput = _playerMapMoveAction.ReadValue<Vector2>();
//        Vector3 verticalMove = transform.forward * moveInput.y;
//        Vector3 horizontalMove = transform.right * moveInput.x;
//        Vector3 moveAmount = (horizontalMove + verticalMove).normalized;

//        moveAmount *= moveSpeed;
//        if (controller.isGrounded) 
//            _ySpeed = 0f;
//        _ySpeed += (Physics.gravity.y * Time.deltaTime);
//        Jump();
//        moveAmount.y = _ySpeed;
//        controller.Move(moveAmount * Time.deltaTime);
//    }

//    private void Rotate()
//    {
//        Vector2 lookInput = _playerMapLookAction.ReadValue<Vector2>();
//        _horizontalRotation += lookInput.x * Time.deltaTime * lookSpeed;
//        transform.rotation = Quaternion.Euler(0f, _horizontalRotation, 0f);
//        _verticalRotation -= lookInput.y * Time.deltaTime * lookSpeed;
//        _verticalRotation = Mathf.Clamp(_verticalRotation, _minLookAngle, _maxLookAngle);
//        cameraTransform.transform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
//    }

//    private void Jump()
//    {
//        if (_playerMapJumpAction.IsPressed() && controller.isGrounded)
//        {
//            _ySpeed = jumpForce;
//        }
//    }

//    #region Input Callbacks
//    private void CheckForInteraction(InputAction.CallbackContext context) // Interact Key Performed
//    {
//        Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));
//        RaycastHit hit;

//        if (_heldObject == null && heldBox == null && _heldFurniture == null)
//        {
//            if (Physics.Raycast(ray, out hit, interactionRange, whatIsStock))
//            {
//                _heldObject = hit.collider.GetComponent<StockObject>();
//                _heldObject.Pickup(_holdPoint);
//                return;
//            }

//            if (Physics.Raycast(ray, out hit, interactionRange, whatIsStockBox))
//            {
//                heldBox = hit.collider.GetComponent<StockBoxController>();
//                heldBox.Pickup(boxHoldPoint);

//                if (!heldBox.OpenBox) // TODO: You can remove this if you don't want to Open box immediately when picking up
//                    heldBox.OpenClose();
//                return;
//            }

//            if (Physics.Raycast(ray, out hit, interactionRange, whatIsPriceLabel))
//            {
//                hit.collider.GetComponentInParent<ShelfSpaceController>().StartPriceUpdate();
//                return;
//            }

//            if (Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
//            {
//                _heldObject = hit.collider.GetComponent<ShelfSpaceController>().GetStock();
//                if (_heldObject != null)
//                    _heldObject.Pickup(_holdPoint);
//                return;
//            }

//            if (Physics.Raycast(ray, out hit, interactionRange, whatIsCheckout))
//            {
//                hit.collider.GetComponent<Checkout>().CheckoutCustomer();
//                return;
//            }
//        }
//        else
//        {
//            if (_heldObject != null) // Holding Stock
//            {
//                if (Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
//                {
//                    hit.collider.GetComponent<ShelfSpaceController>().PlaceStock(_heldObject);

//                    if (_heldObject.IsPlaced)
//                        _heldObject = null;
//                    return;
//                }

//                if (Physics.Raycast(ray, out hit, interactionRange, whatIsTrash))
//                {
//                    _heldObject.TrashObject();
//                    _heldObject = null;
//                }
//            }

//            if (heldBox != null) // Holding Box
//            {
//                if (heldBox.stockInBox.Count > 0) // Interact with Shelf
//                {
//                    if (Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
//                    {
//                        heldBox.PlaceStockOnShelf(hit.collider.GetComponent<ShelfSpaceController>());
//                        _placeStockCounter = WaitToPlaceStock;
//                        _canPlaceBoxObjectFast = true;
//                    }
//                }
//                else // Detect Trash Can
//                {
//                    if (Physics.Raycast(ray, out _, interactionRange, whatIsTrash))
//                    {
//                        heldBox.Release();
//                        heldBox.TrashObject();
//                        heldBox = null;
//                    }
//                }
//            }

//            if (_heldFurniture != null) // Holding Furniture
//            {
//                SetFurnitureDownAndNull();
//            }
//        }
//    }

//    private void CheckForInteractionCancel(InputAction.CallbackContext context)
//    {
//        if (_heldObject == null && heldBox != null)
//        {
//            if (_canPlaceBoxObjectFast)
//                _canPlaceBoxObjectFast = false;
//        }
//    }

//    private void CheckForDrop(InputAction.CallbackContext context)
//    {
//        if (_heldObject != null)
//        {
//            _heldObject.Release();
//            _heldObject.Rb.AddForce(cameraTransform.transform.forward * throwForce, ForceMode.Impulse);
//            _heldObject.transform.SetParent(null);
//            _heldObject = null;
//            return;
//        }

//        if (heldBox != null)
//        {
//            heldBox.Release();
//            heldBox.Rb.AddForce(cameraTransform.transform.forward * throwForce, ForceMode.Impulse);
//            heldBox.transform.SetParent(null);
//            heldBox = null;
//            return;
//        }
//    }

//    private void CheckForOpenCloseBox(InputAction.CallbackContext obj)
//    {
//        if (heldBox == null)
//        {
//            Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));
//            RaycastHit hit;

//            if (Physics.Raycast(ray, out hit, interactionRange, whatIsStockBox))
//            {
//                hit.collider.GetComponent<StockBoxController>().OpenClose();
//                return;
//            }
//        }
//        else
//        {
//            heldBox.OpenClose();
//        }
//    }

//    private void CheckForFurniturePickup(InputAction.CallbackContext context)
//    {
//        Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));
//        RaycastHit hit;
//        if (Physics.Raycast(ray, out hit, interactionRange, whatIsFurniture))
//        {
//            _heldFurniture = hit.transform.GetComponent<FurnitureController>();
//            hit.collider.enabled = false;
//            _heldFurniture.transform.SetParent(_furnitureHoldPoint);
//            _heldFurniture.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
//            _heldFurniture.MakePlaceable();
//            return;
//        }

//        if (_heldFurniture != null) // Press Pickup Button while holding already to set down
//        {
//            SetFurnitureDownAndNull();
//        }
//    }

//    private void UIApplyWithSubmit(InputAction.CallbackContext context)
//    {
//        UIController.Instance.ApplyPriceUpdate();
//    }

//    private void UICloseWithCancelAction(InputAction.CallbackContext context)
//    {
//        OnUIPanelClosedWithCancelAction?.Invoke();
//        DisableUIEnablePlayer();
//    }
//    #endregion

//    #region Box Methods
//    private void PlaceBoxObjectsFast()
//    {
//        if (_heldObject == null && heldBox != null)
//        {
//            Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));

//            if (Physics.Raycast(ray, out RaycastHit hit, interactionRange, whatIsShelf))
//            {
//                _placeStockCounter -= Time.deltaTime;
//                if (_placeStockCounter <= 0 && _playerMapInteractAction.IsPressed())
//                {
//                    heldBox.PlaceStockOnShelf(hit.collider.GetComponent<ShelfSpaceController>());
//                    _placeStockCounter = WaitToPlaceStock;
//                }
//            }
//        }
//    }
//    #endregion

//    #region Furniture Methods
//    private void KeepHeldFurnitureAboveGround()
//    {
//        _heldFurniture.transform.position = new(_furnitureHoldPoint.position.x, 0f, _furnitureHoldPoint.position.z);
//        _heldFurniture.transform.LookAt(new Vector3(transform.position.x, 0f, transform.position.z));
//    }

//    private void SetFurnitureDownAndNull()
//    {
//        if (_heldFurniture == null) return;

//        _heldFurniture.PlaceObject();
//        _heldFurniture = null;
//    }
//    #endregion
//    #endregion
//}
#endregion