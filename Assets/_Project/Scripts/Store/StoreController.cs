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
    private void Awake() => InitializeSingleton();

    private void Start()
    {
        UpdateMoneyDisplay();
        AudioManager.Instance.PlayBGM();
    }

    private void Update() => HandleDebugInput();
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
    public bool CheckMoneyAvailable(float amountToCheck) => _currentMoney >= amountToCheck;

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
            UIController.Instance.UpdateMoney(_currentMoney);
    }
    #endregion

    #region Debug
    private void HandleDebugInput()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.iKey.wasPressedThisFrame)
            AddMoney(100f);

        if (Keyboard.current.oKey.wasPressedThisFrame && CheckMoneyAvailable(250f))
            SpendMoney(250f);
    }
    #endregion
}