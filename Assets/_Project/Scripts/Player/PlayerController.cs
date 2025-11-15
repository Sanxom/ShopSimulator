using PrimeTween;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    #region Events
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
    [SerializeField] private Transform _stockHoldPoint;
    [SerializeField] private Transform _boxHoldPoint;
    [SerializeField] private Transform _furnitureHoldPoint;

    [Header("Interaction Layers")]
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private LayerMask _furnitureLayer;

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
    #endregion

    #region Properties
    public bool IsHoldingSomething => _interaction.IsHoldingSomething;
    public GameObject HeldObject => _interaction.HeldObject;
    public InputSystem_Actions GameInput => _gameInput;

    public int MaxPossibleInteractableObjects { get => _maxPossibleInteractableObjects; private set => _maxPossibleInteractableObjects = value; }
    public PlayerInteraction Interaction { get => _interaction; private set => _interaction = value; }
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
        UIController.OnUIPanelOpened += DisablePlayerEnableUI;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        PrimeTweenConfig.warnZeroDuration = false;
        PrimeTweenConfig.warnEndValueEqualsCurrent = false;
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;
        _movement.UpdateMovement(deltaTime);
        _interaction.UpdateInteraction();
    }

    private void OnDisable()
    {
        UnsubscribeFromInputEvents();
        UIController.OnUIPanelClosed -= DisableUIEnablePlayer;
        UIController.OnUIPanelOpened -= DisablePlayerEnableUI;

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
            stockHoldPoint = _stockHoldPoint,
            boxHoldPoint = _boxHoldPoint,
            furnitureHoldPoint = _furnitureHoldPoint,
            interactableLayer = _interactableLayer,
            furnitureLayer = _furnitureLayer,
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
            _ySpeed = 0f;

        _ySpeed += Physics.gravity.y * deltaTime;
    }

    private void HandleJump()
    {
        if (_jumpAction.IsPressed() && _config.Controller.isGrounded)
        {
            _ySpeed = _config.JumpForce;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(8);
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
        public LayerMask furnitureLayer;
        public LayerMask interactableLayer;
    }

    private readonly Config _config;

    private StockObject _heldStock;
    private StockBoxController _heldBox;
    private FurnitureController _heldFurniture;
    private RaycastHit[] hits = new RaycastHit[PlayerController.Instance.MaxPossibleInteractableObjects];
    private RaycastHit _hit;

    private bool _hasHitInteractable;
    private bool _isFastPlacementActive;
    private bool _isFastTakeActive;
    private InputAction _interactAction;
    private InputAction _takeStockAction;

    public bool IsHoldingSomething => _heldStock != null || _heldBox != null || _heldFurniture != null;

    public GameObject HeldObject;

    public PlayerInteraction(Config config) => _config = config;

    public void SetInputActions(InputAction interactAction, InputAction takeStockAction)
    {
        _interactAction = interactAction;
        _takeStockAction = takeStockAction;
    }

    public void UpdateInteraction()
    {
        if (_isFastPlacementActive)
            ProcessFastPlacement();

        if (_isFastTakeActive)
            ProcessFastTake();

        if (_heldFurniture != null)
            KeepFurnitureAboveGround();
    }

    #region Input Handlers
    public void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (!IsHoldingSomething)
            TryPickupObjectOrInteract();
        else
            TryPlaceOrUseHeldObject();
    }

    public void OnInteractCanceled(InputAction.CallbackContext context)
    {
        if (_heldBox != null && _isFastPlacementActive)
            _isFastPlacementActive = false;
    }

    public void OnDropPerformed(InputAction.CallbackContext context)
    {
        if (_heldStock != null)
        {
            _heldStock.Release();
            _heldStock.Rb.AddForce(_config.camera.transform.forward * _config.throwForce, ForceMode.Impulse);
            _heldStock.transform.SetParent(null);
            RemoveHeldObjectReference();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(9);
        }
        else if (_heldBox != null)
        {
            _heldBox.Release();
            _heldBox.Rb.AddForce(_config.camera.transform.forward * _config.throwForce, ForceMode.Impulse);
            _heldBox.transform.SetParent(null);
            RemoveHeldObjectReference();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(0);
        }
    }

    public void OnOpenBoxPerformed(InputAction.CallbackContext context)
    {
        if (_heldBox != null)
        {
            _heldBox.OpenClose();
        }
        else
        {
            StockBoxController box;
            (_hasHitInteractable, box) = TryRaycastForBox();
            if (!_hasHitInteractable) return;
            box.OpenClose();

            //float minDistance = float.MaxValue;
            //int numOfHits = TryRaycastForInteractables();
            //if (numOfHits == 0) return;

            //for (int i = 0; i < numOfHits; i++)
            //{
            //    if (!hits[i].collider.TryGetComponent(out StockBoxController box)) continue;
            //    if (hits[i].distance < minDistance)
            //    {
            //        minDistance = hits[i].distance;
            //        _ = hits[i];

            //        box.OpenClose();
            //        return;
            //    }
            //}
        }
    }

    public void OnPickupFurniturePerformed(InputAction.CallbackContext context)
    {
        if (_heldFurniture != null)
        {
            PlaceFurniture();
            return;
        }
        else
        {
            (_hasHitInteractable, _hit) = TryRaycastForFurniture();
            if (!_hasHitInteractable) return;

            PickupFurniture(_hit);
            //RaycastHit closestHit;
            //float minDistance = float.MaxValue;
            //int numOfHits = TryRaycastForInteractables();
            //if (numOfHits == 0) return;

            //for (int i = 0; i < numOfHits; i++)
            //{
            //    if (!hits[i].collider.TryGetComponent(out FurnitureController _)) continue;
            //    if (hits[i].distance < minDistance)
            //    {
            //        minDistance = hits[i].distance;
            //        closestHit = hits[i];

            //        PickupFurniture(closestHit);
            //        return;
            //    }
            //}
        }
    }

    public void OnTakeStockPerformed(InputAction.CallbackContext context)
    {
        if (_heldBox == null) return;

        ShelfSpaceController shelf;
        (_hasHitInteractable, shelf) = TryRaycastForShelf();
        if (!_hasHitInteractable) return;

        TryTakeStockFromShelfIntoBox(shelf);
        _isFastTakeActive = true;
        //float minDistance = float.MaxValue;
        //int numOfHits = TryRaycastForInteractables();
        //if (numOfHits == 0) return;

        //for (int i = 0; i < numOfHits; i++)
        //{
        //    if (!hits[i].collider.TryGetComponent(out ShelfSpaceController shelf)) continue;
        //    if (hits[i].distance < minDistance)
        //    {
        //        minDistance = hits[i].distance;
        //        _ = hits[i];
        //        TryTakeStockFromShelfIntoBox(shelf);
        //        _isFastTakeActive = true;
        //        return;
        //    }
        //}
    }

    public void OnTakeStockCanceled(InputAction.CallbackContext context)
    {
        if (_heldBox != null && _isFastTakeActive)
        {
            _isFastTakeActive = false;

            if (_interactAction.IsPressed())
                _isFastPlacementActive = true;
        }
    }
    #endregion

    #region Pickup Logic
    private void TryPickupObjectOrInteract()
    {
        (_hasHitInteractable, _hit) = TryRaycastForInteractable();
        if (!_hasHitInteractable) return;

        IInteractable interactable = _hit.transform.GetComponent<IInteractable>();

        if (_hit.transform.TryGetComponent(out StockObject stockObject))
        {
            if (stockObject.IsOnCheckoutCounter)
                stockObject.MoveToCheckoutBag();
            else
            {
                _heldStock = stockObject;
                HeldObject = _heldStock.gameObject;
                stockObject.OnInteract(_config.stockHoldPoint);
            }
        }
        else if (_hit.transform.TryGetComponent(out StockBoxController box))
        {
            box.OnInteract(_config.boxHoldPoint);
            _heldBox = box;
            HeldObject = _heldBox.gameObject;
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(1);
        }
        else if (_hit.transform.TryGetComponent(out ShelfSpaceController shelf))
        {
            _heldStock = shelf.GetStock();
            if (_heldStock != null)
            {
                HeldObject = _heldStock.gameObject;
                _heldStock.OnInteract(_config.stockHoldPoint);
            }
        }
        else
            interactable.OnInteract();
    }
    #endregion

    #region Place/Use Logic
    private void TryPlaceOrUseHeldObject()
    {
        if (_heldFurniture != null)
            PlaceFurniture();
        else
        {
            (_hasHitInteractable, _hit) = TryRaycastForInteractable();
            if (!_hasHitInteractable) return;
            IInteractable interactable = _hit.collider.GetComponent<IInteractable>();

            if (_hit.collider.TryGetComponent(out TrashCan _) && CanBeTrashed(HeldObject))
            {
                HeldObject.GetComponent<ITrashable>().TrashObject();
                RemoveHeldObjectReference();

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(10);
            }

            if (_heldStock != null)
                HandleHeldStock(interactable);
            else if (_heldBox != null)
                HandleHeldBox(interactable);

            //RaycastHit closestHit = default;
            //float minDistance = float.MaxValue;
            //int numOfHits = TryRaycastForInteractables();
            //if (numOfHits == 0) return;

            //for (int i = 0; i < numOfHits; i++)
            //{
            //    if (!hits[i].collider.TryGetComponent(out TrashCan _)) continue;
            //    if (hits[i].distance < minDistance)
            //    {
            //        minDistance = hits[i].distance;
            //        closestHit = hits[i];
            //        if (CanBeTrashed(HeldObject) && HeldObject.TryGetComponent(out ITrashable trashable))
            //        {
            //            trashable.TrashObject();
            //            RemoveHeldObjectReference();

            //            if (AudioManager.Instance != null)
            //                AudioManager.Instance.PlaySFX(10);
            //            return;
            //        }
            //    }
            //}

            //for (int i = 0; i < numOfHits; i++)
            //{
            //    if (!hits[i].collider.TryGetComponent(out ShelfSpaceController _)) continue;
            //    if (hits[i].distance < minDistance)
            //    {
            //        minDistance = hits[i].distance;
            //        closestHit = hits[i];
            //        break;
            //    }
            //}

            //for (int i = 0; i < numOfHits; i++)
            //{
            //    if (!hits[i].collider.TryGetComponent(out StockBoxController _)) continue;
            //    if (hits[i].distance < minDistance)
            //    {
            //        minDistance = hits[i].distance;
            //        closestHit = hits[i];
            //        break;
            //    }
            //}

            //if (closestHit.collider == null) return;

            //if (closestHit.collider.TryGetComponent(out IInteractable interactable))
            //{
            //    if (_heldStock != null)
            //        HandleHeldStock(interactable);
            //    else if (_heldBox != null)
            //        HandleHeldBox(interactable);
            //}
        }
    }

    private void HandleHeldStock(IInteractable interactable)
    {
        if (interactable.MyObject.TryGetComponent(out ShelfSpaceController shelf))
        {
            shelf.PlaceStock(_heldStock);

            if (_heldStock.IsPlaced)
            {
                RemoveHeldObjectReference();
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(7);
            }
        }
        else if (interactable.MyObject.TryGetComponent(out StockBoxController stockBox))
        {
            if (stockBox.CanTakeStockFromHand(_heldStock))
            {
                RemoveHeldObjectReference();

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(7);
            }
        }
    }

    private void HandleHeldBox(IInteractable interactable)
    {
        if (interactable == null) return;

        if (_heldBox.IsTaking || _heldBox.IsPlacing) return;
        if (_heldBox.StockInBox.Count == 0) return;

        if (interactable.MyObject.TryGetComponent(out ShelfSpaceController shelf))
        {
            _heldBox.PlaceStockOnShelf(shelf);

            _isFastPlacementActive = true;
        }
        return;
    }
    #endregion

    #region Take Stock Into Box
    private void TryTakeStockFromShelfIntoBox(ShelfSpaceController shelf)
    {
        if (_heldBox == null || shelf == null || shelf.StockInfo == null) return;
        if (_heldBox.IsTaking || _heldBox.IsPlacing) return;
        if (shelf.ObjectsOnShelf == null || shelf.ObjectsOnShelf.Count == 0) return;

        bool canTakeStock = (_heldBox.StockInfo == null && _heldBox.StockInBox.Count == 0)
                            || (_heldBox.StockInBox.Count > 0
                            && _heldBox.StockInBox.Count < _heldBox.MaxCapacity
                            && _heldBox.StockInfo.Name == shelf.StockInfo.Name);

        if (!canTakeStock) return;

        StockObject stockFromShelf = shelf.GetStock();
        if (stockFromShelf != null)
        {
            _heldBox.TakeStockFromShelf(stockFromShelf);
        }
    }
    #endregion

    #region Fast Placement
    private void ProcessFastPlacement()
    {
        ShelfSpaceController shelf;
        (_hasHitInteractable, shelf) = TryRaycastForShelf();
        if (!_hasHitInteractable) return;

        if (shelf == null)
        {
            _isFastPlacementActive = false;
            return;
        }

        if (_heldStock != null || _heldBox == null)
        {
            _isFastPlacementActive = false;
            return;
        }

        if (_heldBox.IsTaking) return;

        if (_takeStockAction.WasPressedThisFrame() || _takeStockAction.IsPressed())
        {
            _isFastPlacementActive = false;
            _isFastTakeActive = true;
            return;
        }

        if (_interactAction.IsPressed())
        {
            _heldBox.PlaceStockOnShelf(shelf);
        }

        //float minDistance = float.MaxValue;
        //int numOfHits = TryRaycastForInteractables();
        //if (numOfHits == 0) return;
        //for (int i = 0; i < numOfHits; i++)
        //{
        //    if (!hits[i].collider.TryGetComponent(out ShelfSpaceController shelf)) continue;
        //    if (hits[i].distance < minDistance)
        //    {
        //        minDistance = hits[i].distance;
        //        _ = hits[i];

        //        if (shelf == null)
        //        {
        //            _isFastPlacementActive = false;
        //            return;
        //        }

        //        if (_heldStock != null || _heldBox == null)
        //        {
        //            _isFastPlacementActive = false;
        //            return;
        //        }

        //        if (_heldBox.IsTaking)
        //        {
        //            return;
        //        }

        //        if (_takeStockAction.IsPressed())
        //        {
        //            _isFastPlacementActive = false;
        //            _isFastTakeActive = true;
        //            return;
        //        }

        //        if (_interactAction.IsPressed())
        //        {
        //            _heldBox.PlaceStockOnShelf(shelf);
        //        }
        //        break;
        //    }
        //}
    }
    #endregion

    #region Fast Take
    private void ProcessFastTake()
    {
        ShelfSpaceController shelf;
        (_hasHitInteractable, shelf) = TryRaycastForShelf();
        if (!_hasHitInteractable) return;
        
        if (_heldStock != null || _heldBox == null)
        {
            _isFastTakeActive = false;
            return;
        }

        if (_heldBox.IsPlacing) return;

        if (_takeStockAction.WasPressedThisFrame() || _takeStockAction.IsPressed())
        {
            TryTakeStockFromShelfIntoBox(shelf);
        }

        //float minDistance = float.MaxValue;
        //int numOfHits = TryRaycastForInteractables();
        //if (numOfHits == 0) return;

        //for (int i = 0; i < numOfHits; i++)
        //{
        //    if (!hits[i].collider.TryGetComponent(out ShelfSpaceController shelf)) continue;
        //    if (hits[i].distance < minDistance)
        //    {
        //        minDistance = hits[i].distance;
        //        _ = hits[i];

        //        if (numOfHits == 0 || _heldStock != null || _heldBox == null)
        //        {
        //            _isFastTakeActive = false;
        //            return;
        //        }

        //        if (_heldBox.IsPlacing)
        //        {
        //            return;
        //        }

        //        if (_takeStockAction.IsPressed())
        //        {
        //            TryTakeStockFromShelfIntoBox(shelf);
        //        }
        //        break;
        //    }
        //}
    }
    #endregion

    #region Furniture Methods
    private void PickupFurniture(RaycastHit hit)
    {
        if (hit.transform.TryGetComponent(out FurnitureController furniture))
            _heldFurniture = furniture;

        if (_heldFurniture != null)
            HeldObject = _heldFurniture.gameObject;
        else
            return;

        hit.collider.enabled = false;
        _heldFurniture.transform.SetParent(_config.furnitureHoldPoint);
        _heldFurniture.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        _heldFurniture.MakePlaceable();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(4);
    }

    private void PlaceFurniture()
    {
        if (_heldFurniture == null) return;

        _heldFurniture.PlaceObject();
        RemoveHeldObjectReference();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(5);
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
    private int TryRaycastForInteractables()
    {
        Ray ray = _config.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        int numOfHits = Physics.RaycastNonAlloc(ray, hits, _config.interactionRange, _config.interactableLayer);

        return numOfHits;
    }

    private (bool, RaycastHit) TryRaycastForInteractable()
    {
        Ray ray = _config.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out _hit, _config.interactionRange, _config.interactableLayer))
        {
            _hasHitInteractable = true;
            return (_hasHitInteractable, _hit);
        }
        _hasHitInteractable = false;
        return (_hasHitInteractable, _hit);
    }

    private (bool, RaycastHit hit) TryRaycastForFurniture()
    {
        Ray ray = _config.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out _hit, _config.interactionRange, _config.furnitureLayer))
        {
            _hasHitInteractable = true;
            return (_hasHitInteractable, _hit);
        }

        _hasHitInteractable = false;
        return (_hasHitInteractable, _hit);
    }

    private (bool, ShelfSpaceController) TryRaycastForShelf()
    {
        Ray ray = _config.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out _hit, _config.interactionRange, _config.interactableLayer))
        {
            _hasHitInteractable = false;
            return (_hasHitInteractable, null);
        }

        if (_hit.collider.TryGetComponent(out ShelfSpaceController shelf))
        {
            _hasHitInteractable = true;
            return (_hasHitInteractable, shelf);
        }
        else
        {
            _hasHitInteractable = false;
            return (_hasHitInteractable, null);
        }
    }

    private (bool, StockBoxController) TryRaycastForBox()
    {
        Ray ray = _config.camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out _hit, _config.interactionRange, _config.interactableLayer))
        {
            _hasHitInteractable = false;
            return (_hasHitInteractable, null);
        }

        if (_hit.collider.TryGetComponent(out StockBoxController box))
        {
            _hasHitInteractable = true;
            return (_hasHitInteractable, box);
        }
        else
        {
            _hasHitInteractable = false;
            return (_hasHitInteractable, null);
        }
    }

    private bool CanBeTrashed(GameObject objectToTrash)
    {
        return objectToTrash.TryGetComponent(out StockBoxController box) && box.StockInBox.Count == 0 || objectToTrash.TryGetComponent(out StockObject _);
    }

    private void RemoveHeldObjectReference()
    {
        if (_heldBox != null)
            _heldBox = null;

        if (_heldStock != null)
            _heldStock = null;

        if (_heldFurniture != null)
            _heldFurniture = null;

        HeldObject = null;
    }
    #endregion
}
#endregion