using System;
using System.Collections.Generic;
using UnityEngine;

public class StockObject : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    private const float moveSpeed = 10f;

    private bool _isPlaced;
    #endregion

    #region Public Properties
    [field: SerializeField] public StockInfo Info { get; private set; }

    public Rigidbody Rb { get; private set; }
    public Collider Col { get; private set; }
    public string InteractionPrompt { get; set; }
    public bool IsPlaced { get => _isPlaced; private set => _isPlaced = value; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        Col = GetComponent<Collider>();
        InteractionPrompt = gameObject.name;
    }

    private void OnEnable()
    {
        _isPlaced = false;
    }

    private void Start()
    {
        Info = StockInfoController.Instance.GetStockInfo(Info.name);
    }

    private void Update()
    {
        if (_isPlaced)
        {
            transform.SetLocalPositionAndRotation(Vector3.MoveTowards(transform.localPosition, Vector3.zero, moveSpeed * Time.deltaTime), 
                Quaternion.Slerp(transform.localRotation, Quaternion.identity, moveSpeed * Time.deltaTime));
        }
    }
    #endregion

    #region Public Methods
    public void MakePlaced()
    {
        _isPlaced = true;
        Rb.isKinematic = true;
        Col.enabled = false;
    }

    public void Pickup(Transform holdPoint)
    {
        Rb.isKinematic = true;
        transform.SetParent(holdPoint);
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        Col.enabled = false;
        _isPlaced = false;
    }

    public void Release()
    {
        Rb.isKinematic = false;
        Col.enabled = true;
    }

    public void PlaceInBox()
    {
        Rb.isKinematic = true;
        Col.enabled = false;
    }
    #endregion

    #region Private Methods
    #endregion
}