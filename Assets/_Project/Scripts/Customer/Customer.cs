using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Customer : MonoBehaviour
{
    #region Event Fields
    #endregion

    #region Public Fields
    public List<NavPoint> navPoints = new();
    public float moveSpeed;
    #endregion

    #region Serialized Private Fields
    #endregion

    #region Private Fields
    private const string IS_MOVING_ANIMATOR_NAME = "isMoving";

    private float _currentWaitTime = 0f;
    #endregion

    #region Public Properties
    [field: SerializeField] public Animator Anim { get; private set; }
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        
    }

    private void Start()
    {
        navPoints.Clear();
        navPoints.AddRange(CustomerManager.Instance.GetEntryPoints());
        if (navPoints.Count > 0)
        {
            transform.position = navPoints[0].point.position;
            _currentWaitTime = navPoints[0].waitTime;
        }

        navPoints.AddRange(CustomerManager.Instance.GetExitPoints());
    }

    private void Update()
    {
        if (navPoints.Count > 0)
        {
            MoveToPoint();
        }
    }
    #endregion

    #region Public Methods
    public void MoveToPoint()
    {
        bool isMoving = true;
        Vector3 targetPosition = new(navPoints[0].point.position.x, transform.position.y, navPoints[0].point.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        transform.LookAt(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.25f)
        {
            isMoving = false;

            _currentWaitTime -= Time.deltaTime;

            if (_currentWaitTime <= 0)
            {
                StartNextPoint();
            }
        }

        Anim.SetBool(IS_MOVING_ANIMATOR_NAME, isMoving);
    }

    public void StartNextPoint()
    {
        if (navPoints.Count > 0)
        {
            navPoints.RemoveAt(0);

            if (navPoints.Count > 0)
            {
                _currentWaitTime = navPoints[0].waitTime;
            }
        }
    }
    #endregion

    #region Private Methods
    #endregion
}

[Serializable]
public struct NavPoint
{
    public Transform point;
    public float waitTime;
}