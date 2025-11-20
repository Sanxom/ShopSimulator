using PrimeTween;
using Unity.Cinemachine;
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
    [SerializeField] private CinemachineCamera _cam;
    [SerializeField] private Camera _cardMachineCamera;
    [SerializeField] private Camera _cashRegisterCamera;
    [SerializeField] private PlayerInteraction _playerInteraction;

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _lookSpeed = 2f;
    [SerializeField] private float _jumpForce = 5f;
    [SerializeField] private float _minLookAngle = -80f;
    [SerializeField] private float _maxLookAngle = 80f;
    #endregion

    #region Private Fields
    private InputSystem_Actions _gameInput;
    private PlayerMovement _movement;
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
    public InputSystem_Actions GameInput => _gameInput;

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
        _playerInteraction.SetInputActions(_interactAction, _takeStockAction);
    }

    private void InitializeComponents()
    {
        var movementConfig = new PlayerMovement.Config
        {
            Controller = _controller,
            CameraTransform = _camera.transform,
            CamTransform = _cam.transform,
            Transform = transform,
            MoveSpeed = _moveSpeed,
            LookSpeed = _lookSpeed,
            JumpForce = _jumpForce,
            MinLookAngle = _minLookAngle,
            MaxLookAngle = _maxLookAngle
        };
        _movement = new PlayerMovement(movementConfig, _moveAction, _lookAction, _jumpAction);
    }
    #endregion

    #region Input Event Subscriptions
    private void SubscribeToInputEvents()
    {
        _interactAction.performed += _playerInteraction.OnInteractPerformed;
        //_interactAction.canceled += _playerInteraction.OnInteractCanceled;
        _dropAction.performed += _playerInteraction.OnDropPerformed;
        _openBoxAction.performed += _playerInteraction.OnOpenBoxPerformed;
        _pickupFurnitureAction.performed += _playerInteraction.OnPickupFurniturePerformed;
        _takeStockAction.performed += _playerInteraction.OnTakeStockPerformed;
        _takeStockAction.canceled += _playerInteraction.OnTakeStockCanceled;
    }

    private void UnsubscribeFromInputEvents()
    {
        _interactAction.performed -= _playerInteraction.OnInteractPerformed;
        //_interactAction.canceled -= _playerInteraction.OnInteractCanceled;
        _dropAction.performed -= _playerInteraction.OnDropPerformed;
        _openBoxAction.performed -= _playerInteraction.OnOpenBoxPerformed;
        _pickupFurnitureAction.performed -= _playerInteraction.OnPickupFurniturePerformed;
        _takeStockAction.performed -= _playerInteraction.OnTakeStockPerformed;
        _takeStockAction.canceled -= _playerInteraction.OnTakeStockCanceled;
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

    public void DisablePlayerAndSwapToCardCamera()
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
        public Transform CamTransform;
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
        //_config.Transform.rotation = Quaternion.Euler(0f, _config.CamTransform.rotation.y, 0f);

        //_verticalRotation -= lookInput.y * deltaTime * _config.LookSpeed;
        //_verticalRotation = Mathf.Clamp(_verticalRotation, _config.MinLookAngle, _config.MaxLookAngle);
        //_config.CamTransform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }
}

#endregion