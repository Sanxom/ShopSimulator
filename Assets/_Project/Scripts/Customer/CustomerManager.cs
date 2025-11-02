using masonbell;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    #region Event Fields
    #endregion

    #region Public Fields
    public List<Customer> customersToSpawn = new();
    public List<NavPoint> entryPointsLeft;
    public List<NavPoint> entryPointsRight;
    public float timeBetweenCustomers;
    #endregion

    #region Serialized Private Fields
    [SerializeField] private Transform pooledObjectParent;
    [SerializeField] private int initialPooledObjectSize = 50;
    #endregion

    #region Private Fields
    private float _spawnCounter;
    #endregion

    #region Public Properties
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    private void Start()
    {
        foreach (Customer customer in customersToSpawn) 
        {
            ObjectPool<Customer>.Initialize(customer, initialPooledObjectSize, 0, pooledObjectParent);
        }
    }

    private void Update()
    {
        _spawnCounter -= Time.deltaTime;

        if (_spawnCounter <= 0)
        {
            SpawnCustomer();
        }
    }
    #endregion

    #region Public Methods
    public void SpawnCustomer()
    {
        ObjectPool<Customer>.GetFromPool(customersToSpawn[UnityEngine.Random.Range(0, customersToSpawn.Count)]);
        _spawnCounter = timeBetweenCustomers * UnityEngine.Random.Range(0.75f, 1.25f);
    }

    public List<NavPoint> GetEntryPoints()
    {
        List<NavPoint> points = new();

        switch (UnityEngine.Random.value)
        {
            case < 0.5f:
                points.AddRange(entryPointsLeft);
                break;
            default:
                points.AddRange(entryPointsRight);
                break;
        }

        return points;
    }

    public List<NavPoint> GetExitPoints()
    {
        List<NavPoint> points = new();
        List<NavPoint> temp = new();
        switch (UnityEngine.Random.value)
        {
            case < 0.5f:
                temp.AddRange(entryPointsLeft);
                break;
            default:
                temp.AddRange(entryPointsRight);
                break;
        }

        for (int i = temp.Count - 1; i >= 0; i--)
        {
            points.Add(temp[i]);
        }

        return points;
    }
    #endregion

    #region Private Methods
    #endregion
}