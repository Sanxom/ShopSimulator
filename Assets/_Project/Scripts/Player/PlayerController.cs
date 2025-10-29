using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [Header("References")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference lookAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference interactAction;
    [SerializeField] private InputActionReference dropHeldItemAction;
    [SerializeField] private CharacterController controller;
    [SerializeField] private Camera cameraTransform;

    [Header("Interaction")]
    [SerializeField] private Transform _holdPoint;
    [SerializeField] private LayerMask whatIsStock;
    [SerializeField] private LayerMask whatIsShelf;
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
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        Move();
        Rotate();
        CheckForInteraction();
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    private void Move()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
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
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();
        _horizontalRotation += lookInput.x * Time.deltaTime * lookSpeed;
        transform.rotation = Quaternion.Euler(0f, _horizontalRotation, 0f);
        _verticalRotation -= lookInput.y * Time.deltaTime * lookSpeed;
        _verticalRotation = Mathf.Clamp(_verticalRotation, _minLookAngle, _maxLookAngle);
        cameraTransform.transform.localRotation = Quaternion.Euler(_verticalRotation, 0f, 0f);
    }

    private void Jump()
    {
        if (jumpAction.action.IsPressed()  && controller.isGrounded)
            _ySpeed = jumpForce;
    }

    private void CheckForInteraction()
    {
        Ray ray = cameraTransform.ViewportPointToRay(new(0.5f, 0.5f, 0f));
        RaycastHit hit;

        if (_heldObject == null)
        {
            if (interactAction.action.WasPressedThisFrame() 
                && Physics.Raycast(ray, out hit, interactionRange, whatIsStock))
            {
                _heldObject = hit.collider.GetComponent<StockObject>();
                _heldObject.transform.SetParent(_holdPoint);
                _heldObject.Pickup();
            }
            else if (interactAction.action.WasPressedThisFrame() 
                && Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
            {
                _heldObject = hit.collider.GetComponent<ShelfSpaceController>().GetStock();
                if (_heldObject != null)
                {
                    _heldObject.transform.SetParent(_holdPoint);
                    _heldObject.Pickup();
                }
            }
        }
        else
        {
            if (interactAction.action.WasPressedThisFrame()
                && Physics.Raycast(ray, out hit, interactionRange, whatIsShelf))
            {
                hit.collider.GetComponent<ShelfSpaceController>().PlaceStock(_heldObject);

                if (_heldObject.IsPlaced)
                    _heldObject = null;
            }

            if (dropHeldItemAction.action.WasPressedThisFrame())
            {
                _heldObject.Release();
                _heldObject.Rb.AddForce(cameraTransform.transform.forward * throwForce, ForceMode.Impulse);
                _heldObject.transform.SetParent(null);
                _heldObject = null;
            }
        }
    }
    #endregion
}