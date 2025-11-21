using PrimeTween;
using System;
using System.Collections;
using UnityEngine;

public class CreditCard : InteractableObject
{
    #region Event Fields
    public static event Action OnCameraTransitionToCardMachine;
    public static event Action OnCameraTransitionFromCardMachine;
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private Collider _collider;
    [SerializeField] private Customer _myCustomer;
    [SerializeField] private Vector3 _originalLocalPosition;
    [SerializeField] private Vector3 _originalLocalRotation;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    #endregion

    #region Public Methods
    public override string GetInteractionPrompt(PlayerInteraction player)
    {
        return base.GetInteractionPrompt(player);
    }

    public override void OnInteract(PlayerInteraction player)
    {
        // Invoke Camera Transition Event
        OnCameraTransitionToCardMachine?.Invoke();
        _collider.enabled = false;
        // Move Card to Credit Card Machine on Counter
        StartCoroutine(MoveCardToMachineCoroutine());
    }
    #endregion

    #region Private Methods
    private IEnumerator MoveCardToMachineCoroutine()
    {
        transform.SetParent(Checkout.Instance.CardMoveToPoint);
        yield return Tween.Rotation(transform, Quaternion.identity, 0f).ToYieldInstruction();
        yield return Tween.LocalPosition(transform, Vector3.zero, 0.1f).ToYieldInstruction();
        // Start Credit Card Payment function(s)
        // Start Player Card Machine Interaction
        // Freeze Player
        // Let them Interact with numbers on Card Machine or use Keyboard to enter only numbers (within Range of the payment amount to prevent excessive upCharging or downCharging)
        // Once Submit is pressed (Keyboard or Card Machine Button)
    }
    #endregion
}