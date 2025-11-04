using masonbell;
using System.Collections.Generic;
using UnityEngine;

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
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        InitializeCustomerPools();
        _spawnTimer = _timeBetweenCustomers;
    }

    private void Update()
    {
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

//using masonbell;
//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class CustomerManager : MonoBehaviour
//{
//    public static CustomerManager Instance { get; private set; }

//    #region Event Fields
//    #endregion

//    #region Public Fields
//    public List<Customer> customersToSpawn = new();
//    public List<NavPoint> entryPointsLeft;
//    public List<NavPoint> entryPointsRight;
//    public float timeBetweenCustomers;
//    #endregion

//    #region Serialized Private Fields
//    [SerializeField] private Transform pooledObjectParent;
//    [SerializeField] private Transform customersSpawnParent;
//    [SerializeField] private int initialPooledObjectSize = 50;
//    #endregion

//    #region Private Fields
//    private float _spawnCounter;
//    #endregion

//    #region Public Properties
//    #endregion

//    #region Unity Callbacks
//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//            Destroy(gameObject);
//        else
//            Instance = this;
//    }

//    private void Start()
//    {
//        foreach (Customer customer in customersToSpawn) 
//        {
//            ObjectPool<Customer>.Initialize(customer, initialPooledObjectSize, 0, pooledObjectParent);
//        }
//    }

//    private void Update()
//    {
//        _spawnCounter -= Time.deltaTime;

//        if (_spawnCounter <= 0)
//        {
//            SpawnCustomer();
//        }
//    }
//    #endregion

//    #region Public Methods
//    public void SpawnCustomer()
//    {
//        ObjectPool<Customer>.GetFromPool(customersToSpawn[UnityEngine.Random.Range(0, customersToSpawn.Count)], customersSpawnParent);
//        _spawnCounter = timeBetweenCustomers * UnityEngine.Random.Range(0.75f, 1.25f);
//    }

//    public List<NavPoint> GetEntryPoints()
//    {
//        List<NavPoint> points = new();

//        switch (UnityEngine.Random.value)
//        {
//            case < 0.5f:
//                points.AddRange(entryPointsLeft);
//                break;
//            default:
//                points.AddRange(entryPointsRight);
//                break;
//        }

//        return points;
//    }

//    public List<NavPoint> GetExitPoints()
//    {
//        List<NavPoint> points = new();
//        List<NavPoint> temp = new();
//        switch (UnityEngine.Random.value)
//        {
//            case < 0.5f:
//                temp.AddRange(entryPointsLeft);
//                break;
//            default:
//                temp.AddRange(entryPointsRight);
//                break;
//        }

//        for (int i = temp.Count - 1; i >= 0; i--)
//        {
//            points.Add(temp[i]);
//        }

//        return points;
//    }
//    #endregion

//    #region Private Methods
//    #endregion
//}