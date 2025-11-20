using System;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private LayerMask _furnitureLayer;

    [Header("Variables")]
    [SerializeField] private float _interactionRange;
    [SerializeField] private float _throwForce;

    private RaycastHit _hit;
    private IInteractable _currentInteractableObject;
    private IPlaceable _currentPlaceableObject;

    private bool _hasHitInteractable;
    private InputAction _interactAction;
    private InputAction _takeStockAction;

    #region Properties
    [Header("Properties")]
    [field: SerializeField] public Transform StockHoldPoint { get; private set; }
    [field: SerializeField] public Transform BoxHoldPoint { get; private set; }
    [field: SerializeField] public Transform FurnitureHoldPoint { get; private set; }
    [field: SerializeField] public int MaxPossibleInteractableObjects { get; private set; }

    public GameObject HeldObject { get; set; }
    public StockObject HeldStock { get; set; }
    public StockBoxController HeldBox { get; set; }
    public FurnitureController HeldFurniture { get; set; }
    public bool IsFastPlacementActive { get; set; }
    public bool IsFastTakeActive { get; set; }
    #endregion

    #region Unity Callbacks
    private void Update()
    {
        // Poll for interactions
        _currentInteractableObject = FindNearestInteractable();
        if (_currentInteractableObject == null)
            UIController.Instance.HideInteractionPrompt();
        else
            _currentInteractableObject.GetInteractionPrompt(this);
        // Poll for Furniture objects
        _currentPlaceableObject = FindNearestPlaceable();
        if (HeldFurniture != null) return;

        if (IsFastPlacementActive)
            ProcessFastPlacement();

        if (IsFastTakeActive)
            ProcessFastTake();
    }
    #endregion

    public void SetInputActions(InputAction interactAction, InputAction takeStockAction)
    {
        _interactAction = interactAction;
        _takeStockAction = takeStockAction;
    }

    #region Input Handlers
    public void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (HeldFurniture != null)
        {
            HeldFurniture.OnInteract(this);
            RemoveHeldObjectReference();
        }
        else
        {
            if (_currentInteractableObject == null) return;
            _currentInteractableObject.OnInteract(this);
        }
    }

    public void OnInteractCanceled(InputAction.CallbackContext context)
    {
        if (HeldBox != null && IsFastPlacementActive)
        {
            IsFastPlacementActive = false;
        }
    }

    public void OnDropPerformed(InputAction.CallbackContext context)
    {
        if (HeldStock != null && !HeldStock.IsMoving)
        {
            HeldStock.Release();
            HeldStock.Rb.AddForce(_camera.transform.forward * _throwForce, ForceMode.Impulse);
            HeldStock.transform.SetParent(null);
            RemoveHeldObjectReference();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(9);
        }
        else if (HeldBox != null && !HeldBox.IsMoving)
        {
            HeldBox.Release();
            HeldBox.Rb.AddForce(_camera.transform.forward * _throwForce, ForceMode.Impulse);
            HeldBox.transform.SetParent(null);
            RemoveHeldObjectReference();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(0);
        }
    }

    public void OnOpenBoxPerformed(InputAction.CallbackContext context)
    {
        if (HeldBox != null)
            HeldBox.OpenClose();

        if (_currentInteractableObject == null) return;

        if(_currentInteractableObject.MyObject.TryGetComponent(out StockBoxController box)) // TODO: Don't reference box here maybe?
            box.OpenClose();
    }

    public void OnPickupFurniturePerformed(InputAction.CallbackContext context)
    {
        if (HeldBox != null || HeldStock != null) return;

        if (HeldFurniture != null)
        {
            HeldFurniture.PlaceObject();
            RemoveHeldObjectReference();
            return;
        }
        
        _currentPlaceableObject?.Pickup(this);
    }

    public void OnTakeStockPerformed(InputAction.CallbackContext context)
    {
        if (_currentInteractableObject == null) return;
        if (HeldStock != null) return;
        _currentInteractableObject.OnTake(this);
    }

    public void OnTakeStockCanceled(InputAction.CallbackContext context)
    {
        if (HeldBox != null && IsFastTakeActive)
        {
            if (Mouse.current.leftButton.isPressed) // TODO: Fix this somehow. It's hard-coded, but if we change Interact Action keybind, this won't work.
            {
                IsFastPlacementActive = true;
            }

            IsFastTakeActive = false;

            if (_interactAction.IsPressed())
            {
                IsFastPlacementActive = true;
            }
        }
    }
    #endregion

    #region Fast Placement
    private void ProcessFastPlacement()
    {
        if (_currentInteractableObject == null) return;

        if (HeldStock != null || HeldBox == null)
        {
            IsFastPlacementActive = false;
            return;
        }

        if (HeldBox.IsTaking) return;

        if (_takeStockAction.IsPressed())
        {
            IsFastPlacementActive = false;
            IsFastTakeActive = true;
            return;
        }

        if (_interactAction.IsPressed() || Mouse.current.leftButton.isPressed) // TODO: Fix this somehow. The Mouse is hard-coded because we need to do
            _currentInteractableObject?.OnInteract(this);                              // fast placement if you start with taking then let go of the modifier key
    }
    #endregion

    #region Fast Take
    private void ProcessFastTake()
    {
        if (_currentInteractableObject == null) return;

        if (HeldStock != null || HeldBox == null)
        {
            IsFastTakeActive = false;
            return;
        }

        if (HeldBox.IsPlacingStock) return;

        if (_takeStockAction.IsPressed())
            _currentInteractableObject.OnTake(this);
    }
    #endregion

    #region Utility Methods
    public bool IsHoldingSomething => HeldObject != null;

    private IInteractable FindNearestInteractable()
    {
        //Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Ray ray = new(_camera.transform.position, _camera.transform.forward);
        //RaycastHit[] hits = new RaycastHit[8];
        //Physics.RaycastNonAlloc(ray, hits, _interactionRange, _interactableLayer);
        if (!Physics.Raycast(ray, out _hit, _interactionRange, _interactableLayer))
        {
            _currentInteractableObject?.OnFocusLost();
            return null;
        }

        //Collider col = hits[0].collider;
        Collider col = _hit.collider;
        if (!col.TryGetComponent(out IInteractable interactable))
        {
            _currentInteractableObject?.OnFocusLost();
            return null;
        }
        if (!interactable.CanInteract())
        {
            _currentInteractableObject?.OnFocusLost();
            return null;
        }

        // Handle interactable change
        if (interactable != _currentInteractableObject)
        {
            // Exit previous
            _currentInteractableObject?.OnFocusLost();

            // Enter new
            interactable?.OnFocusGained();

            _currentInteractableObject = interactable;
        }
        return _currentInteractableObject;
    }

    private IPlaceable FindNearestPlaceable()
    {
        //Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Ray ray = new(_camera.transform.position, _camera.transform.forward);

        //RaycastHit[] hits = new RaycastHit[1];
        //Physics.RaycastNonAlloc(ray, hits, _interactionRange, _furnitureLayer);

        if (!Physics.Raycast(ray, out _hit, _interactionRange, _furnitureLayer))
        {
            return null;
        }
        //Collider col = hits[0].collider;
        Collider col = _hit.collider;
        if (!col.TryGetComponent(out IPlaceable placeable))
        {
            return null;
        }

        _currentPlaceableObject = placeable;

        return _currentPlaceableObject;
    }

    private (bool, RaycastHit) TryRaycastForInteractable()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out _hit, _interactionRange, _interactableLayer))
        {
            _hasHitInteractable = true;
            return (_hasHitInteractable, _hit);
        }
        _hasHitInteractable = false;
        return (_hasHitInteractable, _hit);
    }

    private (bool, RaycastHit hit) TryRaycastForFurniture()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out _hit, _interactionRange, _furnitureLayer))
        {
            _hasHitInteractable = true;
            return (_hasHitInteractable, _hit);
        }

        _hasHitInteractable = false;
        return (_hasHitInteractable, _hit);
    }

    private (bool, ShelfSpaceController) TryRaycastForShelf()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out _hit, _interactionRange, _interactableLayer))
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
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (!Physics.Raycast(ray, out _hit, _interactionRange, _interactableLayer))
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

    public bool CanBeTrashed(GameObject objectToTrash)
    {
        return objectToTrash.TryGetComponent(out StockBoxController box) && box.StockInBox.Count == 0 || objectToTrash.TryGetComponent(out StockObject _);
    }

    public void RemoveHeldObjectReference()
    {
        if (HeldBox != null)
            HeldBox = null;

        if (HeldStock != null)
            HeldStock = null;

        if (HeldFurniture != null)
            HeldFurniture = null;

        HeldObject = null;
    }
    #endregion
}