using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StoreController : MonoBehaviour
{
    public static StoreController Instance { get; private set; }

    #region Event Fields
    #endregion

    #region Public Fields
    public List<FurnitureController> shelvingCases = new();

    public Transform stockSpawnPoint;
    public Transform furnitureSpawnPoint;
    public float currentMoney = 1000f;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
            Instance = this;
    }

    private void Start()
    {
        UIController.Instance.UpdateMoney(currentMoney);
    }

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            AddMoney(100f);
        }

        if (Keyboard.current.oKey.wasPressedThisFrame)
        {
            if (CheckMoneyAvailable(250f))
            {
                SpendMoney(250f);
            }
        }
    }
    #endregion

    #region Public Methods
    public bool CheckMoneyAvailable(float amountToCheck)
    {
        bool hasEnough = false;

        if (currentMoney >= amountToCheck)
        {
            hasEnough = true;
        }

        return hasEnough;
    }

    public void AddMoney(float amountToAdd)
    {
        currentMoney += amountToAdd;
        UIController.Instance.UpdateMoney(currentMoney);
    }

    public void SpendMoney(float amountToSpend)
    {
        currentMoney -= amountToSpend;

        if (currentMoney < 0)
            currentMoney = 0;

        UIController.Instance.UpdateMoney(currentMoney);
    }
    #endregion

    #region Private Methods
    #endregion
}