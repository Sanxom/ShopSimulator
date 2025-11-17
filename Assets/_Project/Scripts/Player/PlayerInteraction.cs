using System;
using UnityEditor.ShaderKeywordFilter;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerInteraction : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera _camera;
    [SerializeField] private Transform _stockHoldPoint;
    [SerializeField] private Transform _boxHoldPoint;
    [SerializeField] private Transform _furnitureHoldPoint;
    [SerializeField] private LayerMask _interactableLayer;
    [SerializeField] private LayerMask _furnitureLayer;

    [Header("Variables")]
    [SerializeField] private float _interactionRange;
    [SerializeField] private float _throwForce;
    [SerializeField] private float _waitToPlaceStock;

    private RaycastHit _hit;
    private IInteractable _interactableObject;
    private IPlaceable _placeableObject;
    private string _interactionPrompt;

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
    private bool IsFastTakeActive { get; set; }
    #endregion

    #region Unity Callbacks
    private void Update()
    {
        // Poll for Furniture objects
        _placeableObject = FindNearestPlaceable();
        if (HeldFurniture != null) return;

        // Poll for interactions
        _interactableObject = FindNearestInteractable();
        if (_interactableObject == null) return;
        _interactionPrompt = _interactableObject.GetInteractionPrompt();

        if (IsFastPlacementActive)
            ProcessFastPlacement();

        if (IsFastTakeActive)
            ProcessFastTake();

        //if (HeldFurniture != null)
        //    KeepFurnitureAboveGround();
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
            HeldFurniture.PlaceObject();
            RemoveHeldObjectReference();
        }
        else
        {
            if (_interactableObject == null) return;

            _interactableObject.OnInteract(this);
        }
        //if (!IsHoldingSomething)
        //    TryPickupObjectOrInteract();
        //else
        //    TryPlaceOrUseHeldObject();
    }

    public void OnInteractCanceled(InputAction.CallbackContext context)
    {
        if (HeldBox != null && IsFastPlacementActive)
            IsFastPlacementActive = false;
    }

    public void OnDropPerformed(InputAction.CallbackContext context)
    {
        if (HeldStock != null)
        {
            HeldStock.Release();
            HeldStock.Rb.AddForce(_camera.transform.forward * _throwForce, ForceMode.Impulse);
            HeldStock.transform.SetParent(null);
            RemoveHeldObjectReference();

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(9);
        }
        else if (HeldBox != null)
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
        {
            HeldBox.OpenClose();
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
        if (HeldBox != null || HeldStock != null) return;

        if (HeldFurniture != null)
        {
            HeldFurniture.PlaceObject();
            //PlaceFurniture();
            RemoveHeldObjectReference();
            //return;
        }
        else
        {
            //(_hasHitInteractable, _hit) = TryRaycastForFurniture();
            //if (!_hasHitInteractable) return;

            _placeableObject.Pickup(this);
            //PickupFurniture(_hit);
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
        if (HeldBox == null) return;

        ShelfSpaceController shelf;
        (_hasHitInteractable, shelf) = TryRaycastForShelf();
        if (!_hasHitInteractable) return;

        TryTakeStockFromShelfIntoBox(shelf);
        IsFastTakeActive = true;
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
        if (HeldBox != null && IsFastTakeActive)
        {
            IsFastTakeActive = false;

            if (_interactAction.IsPressed())
                IsFastPlacementActive = true;
        }
    }
    #endregion

    #region Pickup Logic
    private void TryPickupObjectOrInteract()
    {
        (_hasHitInteractable, _hit) = TryRaycastForInteractable();
        if (!_hasHitInteractable) return;

        IInteractable interactable = _hit.transform.GetComponent<IInteractable>();

        //if (_hit.transform.TryGetComponent(out StockObject stockObject))
        //{
        //    if (stockObject.IsOnCheckoutCounter)
        //        stockObject.MoveToCheckoutBag();
        //    else
        //    {
        //        HeldStock = stockObject;
        //        HeldObject = HeldStock.gameObject;
        //        stockObject.OnInteract(StockHoldPoint);
        //    }
        //}
        //else if (_hit.transform.TryGetComponent(out StockBoxController box))
        //{
        //    box.OnInteract(BoxHoldPoint);
        //    HeldBox = box;
        //    HeldObject = HeldBox.gameObject;
        //    if (AudioManager.Instance != null)
        //        AudioManager.Instance.PlaySFX(1);
        //}
        //else if (_hit.transform.TryGetComponent(out ShelfSpaceController shelf))
        //{
        //    HeldStock = shelf.GetStock();
        //    if (HeldStock != null)
        //    {
        //        HeldObject = HeldStock.gameObject;
        //        HeldStock.OnInteract(StockHoldPoint);
        //    }
        //}
        //else
            //interactable.OnInteract();
    }
    #endregion

    #region Place/Use Logic
    private void TryPlaceOrUseHeldObject()
    {
        //if (HeldFurniture != null)
        //    PlaceFurniture();
        //else
        //{
        //    (_hasHitInteractable, _hit) = TryRaycastForInteractable();
        //    if (!_hasHitInteractable) return;
        //    IInteractable interactable = _hit.collider.GetComponent<IInteractable>();

        //    if (_hit.collider.TryGetComponent(out TrashCan _) && CanBeTrashed(HeldObject))
        //    {
        //        HeldObject.GetComponent<ITrashable>().TrashObject();
        //        RemoveHeldObjectReference();

        //        if (AudioManager.Instance != null)
        //            AudioManager.Instance.PlaySFX(10);
        //    }

        //    if (HeldStock != null)
        //        HandleHeldStock(interactable);
        //    else if (HeldBox != null)
        //        HandleHeldBox(interactable);

        //    //RaycastHit closestHit = default;
        //    //float minDistance = float.MaxValue;
        //    //int numOfHits = TryRaycastForInteractables();
        //    //if (numOfHits == 0) return;

        //    //for (int i = 0; i < numOfHits; i++)
        //    //{
        //    //    if (!hits[i].collider.TryGetComponent(out TrashCan _)) continue;
        //    //    if (hits[i].distance < minDistance)
        //    //    {
        //    //        minDistance = hits[i].distance;
        //    //        closestHit = hits[i];
        //    //        if (CanBeTrashed(HeldObject) && HeldObject.TryGetComponent(out ITrashable trashable))
        //    //        {
        //    //            trashable.TrashObject();
        //    //            RemoveHeldObjectReference();

        //    //            if (AudioManager.Instance != null)
        //    //                AudioManager.Instance.PlaySFX(10);
        //    //            return;
        //    //        }
        //    //    }
        //    //}

        //    //for (int i = 0; i < numOfHits; i++)
        //    //{
        //    //    if (!hits[i].collider.TryGetComponent(out ShelfSpaceController _)) continue;
        //    //    if (hits[i].distance < minDistance)
        //    //    {
        //    //        minDistance = hits[i].distance;
        //    //        closestHit = hits[i];
        //    //        break;
        //    //    }
        //    //}

        //    //for (int i = 0; i < numOfHits; i++)
        //    //{
        //    //    if (!hits[i].collider.TryGetComponent(out StockBoxController _)) continue;
        //    //    if (hits[i].distance < minDistance)
        //    //    {
        //    //        minDistance = hits[i].distance;
        //    //        closestHit = hits[i];
        //    //        break;
        //    //    }
        //    //}

        //    //if (closestHit.collider == null) return;

        //    //if (closestHit.collider.TryGetComponent(out IInteractable interactable))
        //    //{
        //    //    if (_heldStock != null)
        //    //        HandleHeldStock(interactable);
        //    //    else if (_heldBox != null)
        //    //        HandleHeldBox(interactable);
        //    //}
        //}
    }

    private void HandleHeldStock(IInteractable interactable)
    {
        if (interactable.MyObject.TryGetComponent(out ShelfSpaceController shelf))
        {
            shelf.PlaceStock(HeldStock);

            if (HeldStock.IsPlaced)
            {
                RemoveHeldObjectReference();
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySFX(7);
            }
        }
        else if (interactable.MyObject.TryGetComponent(out StockBoxController stockBox))
        {
            if (stockBox.CanTakeStockFromHand(HeldStock))
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

        if (HeldBox.IsTaking || HeldBox.IsPlacing) return;
        if (HeldBox.StockInBox.Count == 0) return;

        if (interactable.MyObject.TryGetComponent(out ShelfSpaceController shelf))
        {
            HeldBox.PlaceStockOnShelf(shelf);

            IsFastPlacementActive = true;
        }
    }
    #endregion

    #region Take Stock Into Box
    private void TryTakeStockFromShelfIntoBox(ShelfSpaceController shelf)
    {
        if (HeldBox == null || shelf == null || shelf.StockInfo == null) return;
        if (HeldBox.IsTaking || HeldBox.IsPlacing) return;
        if (shelf.ObjectsOnShelf == null || shelf.ObjectsOnShelf.Count == 0) return;

        bool canTakeStock = (HeldBox.StockInfo == null && HeldBox.StockInBox.Count == 0)
                            || (HeldBox.StockInBox.Count > 0
                            && HeldBox.StockInBox.Count < HeldBox.MaxCapacity
                            && HeldBox.StockInfo.Name == shelf.StockInfo.Name);

        if (!canTakeStock) return;

        StockObject stockFromShelf = shelf.GetStock();
        if (stockFromShelf != null)
        {
            HeldBox.TakeStockFromShelf(stockFromShelf);
        }
    }
    #endregion

    #region Fast Placement
    private void ProcessFastPlacement()
    {
        ShelfSpaceController shelf;
        (_hasHitInteractable, shelf) = TryRaycastForShelf(); // TODO: Remove this Raycast and use the one we're polling instead
        if (!_hasHitInteractable) return;

        if (shelf == null)
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

        if (_takeStockAction.WasPressedThisFrame() || _takeStockAction.IsPressed())
        {
            IsFastPlacementActive = false;
            IsFastTakeActive = true;
            return;
        }

        if (_interactAction.IsPressed())
        {
            HeldBox.PlaceStockOnShelf(shelf);
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
        (_hasHitInteractable, shelf) = TryRaycastForShelf(); // TODO: Remove this Raycast and use the one we're polling instead.
        if (!_hasHitInteractable) return;
        
        if (HeldStock != null || HeldBox == null)
        {
            IsFastTakeActive = false;
            return;
        }

        if (HeldBox.IsPlacing) return;

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
            HeldFurniture = furniture;

        if (HeldFurniture != null)
            HeldObject = HeldFurniture.gameObject;
        else
            return;

        hit.collider.enabled = false;
        HeldFurniture.transform.SetParent(FurnitureHoldPoint);
        HeldFurniture.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        HeldFurniture.MakePlaceable();

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlaySFX(4);
    }

    //private void PlaceFurniture()
    //{
    //    if (HeldFurniture == null) return;

    //    HeldFurniture.PlaceObject();
    //    RemoveHeldObjectReference();
    //}

    private void KeepFurnitureAboveGround()
    {
        Vector3 holdPosition = FurnitureHoldPoint.position;
        Vector3 playerPosition = FurnitureHoldPoint.parent.position;

        HeldFurniture.transform.position = new Vector3(holdPosition.x, 0f, holdPosition.z);
        HeldFurniture.transform.LookAt(new Vector3(playerPosition.x, 0f, playerPosition.z));
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