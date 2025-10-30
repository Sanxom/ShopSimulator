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
    [SerializeField] private LayerMask whatIsStock;
    [SerializeField] private LayerMask whatIsShelf;
    [SerializeField] private LayerMask whatIsPriceLabel;
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
    private InputAction _moveAction;
    private InputAction _lookAction;
    private InputAction _jumpAction;
    private InputAction _playerMapInteractAction;
    private InputAction _playerMapDropAction;
    private InputAction _uiMapSubmitAction;
    private InputAction _uiMapCancelAction;

    [Header("Interaction")]
    private StockObject _heldObject;

    [Header("Movement")]
    private float _ySpeed;
    private float _horizontalRotation;
    private float _verticalRotation;
    #endregion

    #region Public Properties
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
        _moveAction = _gameInput.Player.Move;
        _lookAction = _gameInput.Player.Look;   
        _jumpAction = _gameInput.Player.Jump;
        _playerMapInteractAction = _gameInput.Player.Interact;
        _playerMapDropAction = _gameInput.Player.DropHeldItem;
        #endregion

        #region UI Map Actions
        _uiMapSubmitAction = _gameInput.UI.Submit;
        _uiMapCancelAction = _gameInput.UI.Cancel;
        #endregion
    }

    private void OnEnable()
    {
        _playerMapInteractAction.performed += CheckForInteraction;
        _playerMapDropAction.performed += CheckForDrop;
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
    }

    private void OnDisable()
    {
        _playerMapInteractAction.performed -= CheckForInteraction;
        _playerMapDropAction.performed -= CheckForDrop;
        _uiMapSubmitAction.performed -= UIApplyWithSubmit;
        _uiMapCancelAction.performed -= UICloseWithCancelAction;
        UIController.OnUIPanelClosed -= DisableUIEnablePlayer;
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
        Vector2 moveInput = _moveAction.ReadValue<Vector2>();
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
        Vector2 lookInput = _lookAction.ReadValue<Vector2>();
        _horizontalRotation += lookInput.x * Time.deltaTime * lookSpeed;
        transform.rotation = Quaternion.Euler(0f, _horizontalRotation, 0f);
        _verticalRotation -= lookInput.y * Time.deltaTime * lookSpeed;
        _verticalRotation = Mathf.Clamp(_verticalRotation, _minLookAngle, _maxLookAngle);
        cameraTransform.transform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }

    private void Jump()
    {
        if (_jumpAction.IsPressed() && controller.isGrounded)
        {
            _ySpeed = jumpForce;
        }
    }

    private void CheckForInteraction(InputAction.CallbackContext context)
    {
        Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (_heldObject == null)
        {
            if (Physics.Raycast(ray, out hit, interactionRange, whatIsStock))
            {
                _heldObject = hit.collider.GetComponent<StockObject>();
                _heldObject.Pickup(_holdPoint);
            }
            else if (Physics.Raycast(ray, out hit, interactionRange, whatIsPriceLabel))
            {
                hit.collider.GetComponentInParent<ShelfSpaceController>().StartPriceUpdate();
            }
            else if (Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
            {
                _heldObject = hit.collider.GetComponent<ShelfSpaceController>().GetStock();
                if (_heldObject != null)
                    _heldObject.Pickup(_holdPoint);
            }
        }
        else
        {
            if (Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
            {
                hit.collider.GetComponent<ShelfSpaceController>().PlaceStock(_heldObject);

                if (_heldObject.IsPlaced)
                    _heldObject = null;
            }
        }
    }
    
    private void CheckForDrop(InputAction.CallbackContext context)
    {
        if (_heldObject == null) return;

        _heldObject.Release();
        _heldObject.Rb.AddForce(cameraTransform.transform.forward * throwForce, ForceMode.Impulse);
        _heldObject.transform.SetParent(null);
        _heldObject = null;
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
}