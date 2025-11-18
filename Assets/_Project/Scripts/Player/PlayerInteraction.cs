using System;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
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
    private IInteractable _interactableObject;
    private IPlaceable _placeableObject;

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
        _interactableObject = FindNearestInteractable();
        if (_interactableObject == null)
        {
            UIController.Instance.HideInteractionPrompt();
            return;
        }
        _interactableObject.GetInteractionPrompt();

        // Poll for Furniture objects
        _placeableObject = FindNearestPlaceable();
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
            if (_interactableObject == null) return;
            _interactableObject.OnInteract(this);
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

        if (_interactableObject == null) return;

        if(_interactableObject.MyObject.TryGetComponent(out StockBoxController box)) // TODO: Don't reference box here maybe?
            box.OpenClose();
    }

    public void OnPickupFurniturePerformed(InputAction.CallbackContext context)
    {
        if (HeldBox != null || HeldStock != null) return;

        if (HeldFurniture != null)
        {
            HeldFurniture.PlaceObject();
            RemoveHeldObjectReference();
        }
        else
            _placeableObject.Pickup(this);
    }

    public void OnTakeStockPerformed(InputAction.CallbackContext context)
    {
        if (_interactableObject == null) return;
        if (HeldStock != null) return;
        _interactableObject.OnTake(this);
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
        if (_interactableObject == null)
        {
            IsFastPlacementActive = false;
            return;
        }
        
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
            _interactableObject.OnInteract(this);                              // fast placement if you start with taking then let go of the modifier key
    }
    #endregion

    #region Fast Take
    private void ProcessFastTake()
    {
        if (_interactableObject == null) return;

        if (HeldStock != null || HeldBox == null)
        {
            IsFastTakeActive = false;
            return;
        }

        if (HeldBox.IsPlacingStock) return;

        if (_takeStockAction.IsPressed())
            _interactableObject.OnTake(this);
    }
    #endregion

    #region Utility Methods
    public bool IsHoldingSomething => HeldObject != null;

    private IInteractable FindNearestInteractable()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = new RaycastHit[1];
        Physics.RaycastNonAlloc(ray, hits, _interactionRange, _interactableLayer);

        Collider col = hits[0].collider;
        if (col == null) return null;
        if (!col.TryGetComponent(out IInteractable interactable)) return null;
        if (!interactable.CanInteract()) return null;

        return interactable;
    }

    private IPlaceable FindNearestPlaceable()
    {
        Ray ray = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        RaycastHit[] hits = new RaycastHit[1];
        Physics.RaycastNonAlloc(ray, hits, _interactionRange, _furnitureLayer);
        Collider col = hits[0].collider;
        if (col == null) return null;
        if (!col.TryGetComponent(out IPlaceable placeable)) return null;

        return placeable;
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