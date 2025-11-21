using System;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraSwitcher : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    [SerializeField] private CinemachineCamera _playerCamera;
    [SerializeField] private CinemachineCamera _cardMachineCamera;
    [SerializeField] private CinemachineCamera _cashRegisterCamera;

    [SerializeField] private List<CinemachineCamera> _cinemachineCamerasList;
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        CreditCard.OnCameraTransitionToCardMachine += SwitchToCardMachineCamera;
        CreditCard.OnCameraTransitionFromCardMachine += SwitchToPlayerCamera;

        SwitchToCamera(_playerCamera);
    }

    private void OnDestroy()
    {
        CreditCard.OnCameraTransitionToCardMachine -= SwitchToCardMachineCamera;
        CreditCard.OnCameraTransitionFromCardMachine -= SwitchToPlayerCamera;
    }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    private void SwitchToCamera(CinemachineCamera targetCamera)
    {
        foreach (CinemachineCamera cinemachineCamera in _cinemachineCamerasList)
        {
            cinemachineCamera.gameObject.SetActive(cinemachineCamera == targetCamera);
        }
    }

    private void SwitchToCardMachineCamera()
    {
        SwitchToCamera(_cardMachineCamera);
        PlayerController.Instance.DisablePlayerEnableUI();
    }

    private void SwitchToPlayerCamera()
    {
        SwitchToCamera(_playerCamera);
        PlayerController.Instance.DisableUIEnablePlayer();
    }
    #endregion
}