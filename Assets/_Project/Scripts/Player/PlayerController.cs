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
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private CharacterController controller;

    [Header("Movement")]
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpForce;
    #endregion

    #region Private Fields
    private float ySpeed;
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Update()
    {
        Move();
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    private void Move()
    {
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();

        Vector3 moveAmount = new(moveInput.x, 0f, moveInput.y);
        moveAmount *= moveSpeed;

        if (controller.isGrounded) 
            ySpeed = 0f;

        ySpeed += (Physics.gravity.y * Time.deltaTime);
        Jump();
        moveAmount.y = ySpeed;

        controller.Move(moveAmount * Time.deltaTime);
    }

    private void Jump()
    {
        if (jumpAction.action.IsPressed()  && controller.isGrounded)
            ySpeed = jumpForce;
    }
    #endregion
}