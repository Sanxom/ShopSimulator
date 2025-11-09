using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    #region Serialized Fields
    [Header("Customer Settings")]
    [SerializeField] private List<Customer> _customersToSpawn = new();
    [SerializeField] private float _timeBetweenCustomers = 5f;

    [Header("Navigation Points")]
    [SerializeField] private List<NavPoint> _entryPointsLeft;
    [SerializeField] private List<NavPoint> _entryPointsRight;

    [Header("Pooling")]
    [SerializeField] private Transform _pooledObjectParent;
    [SerializeField] private Transform _customersSpawnParent;
    [SerializeField] private int _initialPooledObjectSize = 50;
    #endregion

    #region Private Fields
    private float _spawnTimer;
    private bool _isInitialized;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        if (_isInitialized) return;

        InitializeCustomerPools();
        _spawnTimer = _timeBetweenCustomers;
        _isInitialized = true;
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "MainMenu") return;

        UpdateSpawnTimer(Time.deltaTime);
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
        DontDestroyOnLoad(gameObject);
    }

    private void InitializeCustomerPools()
    {
        foreach (Customer customer in _customersToSpawn)
        {
            if (customer != null)
            {
                ObjectPool<Customer>.Initialize(customer, _initialPooledObjectSize, 0, _pooledObjectParent);
            }
        }
    }
    #endregion

    #region Spawn Logic
    private void UpdateSpawnTimer(float deltaTime)
    {
        _spawnTimer -= deltaTime;

        if (_spawnTimer <= 0f)
        {
            SpawnCustomer();
            ResetSpawnTimer();
        }
    }

    public void SpawnCustomer()
    {
        if (_customersToSpawn.Count == 0)
        {
            return;
        }

        Customer randomCustomer = _customersToSpawn[Random.Range(0, _customersToSpawn.Count)];
        ObjectPool<Customer>.GetFromPool(randomCustomer, _customersSpawnParent);
    }

    private void ResetSpawnTimer()
    {
        _spawnTimer = _timeBetweenCustomers * Random.Range(0.75f, 1.25f);
    }
    #endregion

    #region Navigation Points
    public List<NavPoint> GetEntryPoints()
    {
        return Random.value < 0.5f
            ? new List<NavPoint>(_entryPointsLeft)
            : new List<NavPoint>(_entryPointsRight);
    }

    public List<NavPoint> GetExitPoints()
    {
        List<NavPoint> entryPoints = Random.value < 0.5f
            ? _entryPointsLeft
            : _entryPointsRight;

        List<NavPoint> exitPoints = new List<NavPoint>();

        for (int i = entryPoints.Count - 1; i >= 0; i--)
        {
            exitPoints.Add(entryPoints[i]);
        }

        return exitPoints;
    }
    #endregion
}