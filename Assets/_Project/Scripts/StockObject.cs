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
    [SerializeField] private Collider col;
    [SerializeField] private float moveSpeed;
    #endregion

    #region Private Fields
    private bool _isPlaced;
    #endregion

    #region Public Properties
    public Rigidbody Rb { get; private set; }
    public string InteractionPrompt { get; set; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        Rb = GetComponent<Rigidbody>();
        InteractionPrompt = gameObject.name;
    }

    private void OnEnable()
    {
        _isPlaced = false;
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
    public void Pickup()
    {
        Rb.isKinematic = true;
        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        col.enabled = false;
        _isPlaced = false;
    }

    public void Place()
    {
        _isPlaced = true;
        Rb.isKinematic = true;
        col.enabled = true;
    }

    public void Release()
    {
        Rb.isKinematic = false;
        col.enabled = true;
    }
    #endregion

    #region Private Methods
    #endregion
}