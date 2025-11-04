using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class StoreController : MonoBehaviour
{
    public static StoreController Instance { get; private set; }

    #region Serialized Fields
    [Header("Store Settings")]
    [SerializeField] private float _currentMoney = 1000f;

    [Header("Spawn Points")]
    [SerializeField] private Transform _stockSpawnPoint;
    [SerializeField] private Transform _furnitureSpawnPoint;

    [Header("Shelving")]
    [SerializeField] private List<FurnitureController> _shelvingCases = new();
    #endregion

    #region Properties
    public List<FurnitureController> ShelvingCases => _shelvingCases;
    public Transform StockSpawnPoint => _stockSpawnPoint;
    public Transform FurnitureSpawnPoint => _furnitureSpawnPoint;
    public float CurrentMoney => _currentMoney;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        UpdateMoneyDisplay();
    }

    private void Update()
    {
        HandleDebugInput();
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
    #endregion

    #region Money Management
    public bool CheckMoneyAvailable(float amountToCheck)
    {
        return _currentMoney >= amountToCheck;
    }

    public void AddMoney(float amountToAdd)
    {
        _currentMoney += amountToAdd;
        UpdateMoneyDisplay();
    }

    public void SpendMoney(float amountToSpend)
    {
        _currentMoney -= amountToSpend;
        _currentMoney = Mathf.Max(0f, _currentMoney);
        UpdateMoneyDisplay();
    }

    private void UpdateMoneyDisplay()
    {
        if (UIController.Instance != null)
        {
            UIController.Instance.UpdateMoney(_currentMoney);
        }
    }
    #endregion

    #region Debug
    private void HandleDebugInput()
    {
        if (Keyboard.current == null)
        {
            return;
        }

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
}

//using System;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class StoreController : MonoBehaviour
//{
//    public static StoreController Instance { get; private set; }

//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public List<FurnitureController> shelvingCases = new();

//    public Transform stockSpawnPoint;
//    public Transform furnitureSpawnPoint;
//    public float currentMoney = 1000f;
//    #endregion

//    #region Serialized Private Fields
//    #endregion

//    #region Private Fields
//    #endregion

//    #region Public Properties
//    #endregion

//    #region Unity Callbacks
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//        }
//        else
//            Instance = this;
//    }

//    private void Start()
//    {
//        UIController.Instance.UpdateMoney(currentMoney);
//    }

//    private void Update()
//    {
//        if (Keyboard.current.iKey.wasPressedThisFrame)
//        {
//            AddMoney(100f);
//        }

//        if (Keyboard.current.oKey.wasPressedThisFrame)
//        {
//            if (CheckMoneyAvailable(250f))
//            {
//                SpendMoney(250f);
//            }
//        }
//    }
//    #endregion

//    #region Public Methods
//    public bool CheckMoneyAvailable(float amountToCheck)
//    {
//        bool hasEnough = false;

//        if (currentMoney >= amountToCheck)
//        {
//            hasEnough = true;
//        }

//        return hasEnough;
//    }

//    public void AddMoney(float amountToAdd)
//    {
//        currentMoney += amountToAdd;
//        UIController.Instance.UpdateMoney(currentMoney);
//    }

//    public void SpendMoney(float amountToSpend)
//    {
//        currentMoney -= amountToSpend;

//        if (currentMoney < 0)
//            currentMoney = 0;

//        UIController.Instance.UpdateMoney(currentMoney);
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}