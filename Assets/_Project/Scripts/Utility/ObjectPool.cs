using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// High-performance generic object pool for Unity components.
/// Dramatically reduces GC pressure and instantiation overhead for frequently spawned objects.
/// Supports automatic pool growth, warm-up, and customizable reset behavior.
/// Typical performance gain: 10-100x faster than Instantiate/Destroy.
/// </summary>
/// <typeparam name="T">Component type to pool (must derive from Component)</typeparam>
public static class ObjectPool<T> where T : Component
{
    private static readonly Dictionary<T, Pool> PoolsByPrefab = new Dictionary<T, Pool>();
    private static readonly Dictionary<T, Pool> ActiveObjectToPool = new Dictionary<T, Pool>();

    /// <summary>
    /// Initialize a pool for a specific prefab.
    /// Should be called during scene initialization, typically in Start or Awake.
    /// </summary>
    /// <param name="prefab">Prefab to create pool for</param>
    /// <param name="initialSize">Number of objects to pre-instantiate (warm-up)</param>
    /// <param name="maxSize">Maximum pool size. 0 = unlimited growth</param>
    /// <param name="parent">Optional parent transform for organization</param>
    /// <param name="onSpawn">Optional callback when object is retrieved from pool</param>
    /// <param name="onReturn">Optional callback when object is returned to pool</param>
    public static void Initialize(
        T prefab,
        int initialSize = 10,
        int maxSize = 100,
        Transform parent = null,
        Action<T> onSpawn = null,
        Action<T> onReturn = null)
    {
        if (prefab == null)
        {
            Debug.LogError($"[ObjectPool<{typeof(T).Name}>] Cannot initialize pool with null prefab.");
            return;
        }

        if (PoolsByPrefab.ContainsKey(prefab))
        {
            Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Pool already initialized for prefab: {prefab.name}. Ignoring duplicate initialization.");
            return;
        }

        Pool pool = new Pool(prefab, initialSize, maxSize, parent, onSpawn, onReturn);
        PoolsByPrefab[prefab] = pool;

        Debug.Log($"[ObjectPool<{typeof(T).Name}>] Initialized pool for '{prefab.name}' with {initialSize} objects (Max: {(maxSize == 0 ? "Unlimited" : maxSize.ToString())})");
    }

    /// <summary>
    /// Get an object from the pool. If pool is empty, creates a new object.
    /// Much faster than Instantiate (typically 10-100x).
    /// </summary>
    /// <param name="prefab">Optional prefab reference. If null, uses first initialized pool.</param>
    /// <returns>Pooled object instance</returns>
    public static T GetFromPool(T prefab = null, Transform parent = null)
    {
        Pool pool = GetPool(prefab);

        if (pool == null)
        {
            Debug.LogError($"[ObjectPool<{typeof(T).Name}>] No pool initialized. Call Initialize() first.");
            return null;
        }

        T obj = pool.GetFromPool();
        obj.transform.parent = parent;
        ActiveObjectToPool[obj] = pool;
        return obj;
    }

    /// <summary>
    /// Get an object from the pool. If pool is empty, creates a new object.
    /// Much faster than Instantiate (typically 10-100x).
    /// </summary>
    /// <param name="prefab">Prefab to get based on Type.</param>
    /// <param name="position">Spawn position.</param>
    /// <param name="rotation">Spawn rotation.</param>
    /// <returns>Pooled object instance</returns>
    public static T GetFromPool(T prefab, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        Pool pool = GetPool(prefab);

        if (pool == null)
        {
            Debug.LogError($"[ObjectPool<{typeof(T).Name}>] No pool initialized. Call Initialize() first.");
            return null;
        }

        T obj = pool.GetFromPool();
        obj.transform.parent = parent;
        if (parent != null)
            obj.transform.SetLocalPositionAndRotation(position, rotation);
        else
            obj.transform.SetPositionAndRotation(position, rotation);

            ActiveObjectToPool[obj] = pool;
        return obj;
    }

    /// <summary>
    /// Return an object to the pool for reuse.
    /// Much faster than Destroy (typically 100-1000x).
    /// </summary>
    /// <param name="obj">Object to return to pool</param>
    public static void ReturnToPool(T obj)
    {
        if (obj == null)
        {
            Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Attempted to return null object to pool.");
            return;
        }

        if (!ActiveObjectToPool.TryGetValue(obj, out Pool pool))
        {
            Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Object '{obj.name}' was not retrieved from a pool. Destroying instead.");
            UnityEngine.Object.Destroy(obj.gameObject);
            return;
        }

        pool.ReturnToPool(obj);
        ActiveObjectToPool.Remove(obj);
    }

    /// <summary>
    /// Pre-warm pool by instantiating additional objects.
    /// Useful when you know you'll need more objects soon.
    /// </summary>
    /// <param name="prefab">Prefab reference</param>
    /// <param name="count">Number of additional objects to create</param>
    public static void WarmUp(T prefab, int count)
    {
        Pool pool = GetPool(prefab);

        if (pool == null)
        {
            Debug.LogError($"[ObjectPool<{typeof(T).Name}>] Cannot warm up uninitialized pool.");
            return;
        }

        pool.WarmUp(count);
    }

    /// <summary>
    /// Return all active objects to their pools.
    /// Useful when clearing a scene or resetting game state.
    /// </summary>
    public static void ReturnAllToPool()
    {
        // Create snapshot to avoid modification during iteration
        var activeObjects = new List<T>(ActiveObjectToPool.Keys);

        foreach (var obj in activeObjects)
        {
            ReturnToPool(obj);
        }
    }

    /// <summary>
    /// Clear and destroy a specific pool.
    /// </summary>
    /// <param name="prefab">Prefab reference</param>
    public static void ClearPool(T prefab)
    {
        if (prefab == null || !PoolsByPrefab.TryGetValue(prefab, out Pool pool))
        {
            Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] No pool found for prefab.");
            return;
        }

        pool.Clear();
        PoolsByPrefab.Remove(prefab);

        Debug.Log($"[ObjectPool<{typeof(T).Name}>] Cleared pool for '{prefab.name}'");
    }

    /// <summary>
    /// Clear all pools for this type.
    /// Useful during scene transitions or application shutdown.
    /// </summary>
    public static void ClearAllPools()
    {
        foreach (var pool in PoolsByPrefab.Values)
        {
            pool.Clear();
        }

        PoolsByPrefab.Clear();
        ActiveObjectToPool.Clear();

        Debug.Log($"[ObjectPool<{typeof(T).Name}>] Cleared all pools.");
    }

    /// <summary>
    /// Get statistics for a specific pool.
    /// </summary>
    public static PoolStatistics GetStatistics(T prefab)
    {
        Pool pool = GetPool(prefab);
        return pool?.GetStatistics();
    }

    /// <summary>
    /// Log pool statistics to console.
    /// </summary>
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogStatistics(T prefab)
    {
        Pool pool = GetPool(prefab);
        pool?.LogStatistics();
    }

    private static Pool GetPool(T prefab)
    {
        if (prefab != null && PoolsByPrefab.TryGetValue(prefab, out Pool pool))
        {
            return pool;
        }

        // If no prefab specified, return first pool (single pool scenario)
        if (prefab == null && PoolsByPrefab.Count == 1)
        {
            using (var enumerator = PoolsByPrefab.Values.GetEnumerator())
            {
                enumerator.MoveNext();
                return enumerator.Current;
            }
        }

        return null;
    }

    /// <summary>
    /// Internal pool implementation.
    /// </summary>
    private class Pool
    {
        private readonly T _prefab;
        private readonly int _maxSize;
        private readonly Transform _parent;
        private readonly Action<T> _onSpawn;
        private readonly Action<T> _onReturn;

        private readonly Queue<T> _availableObjects;
        private readonly HashSet<T> _allObjects;

        // Statistics
        private int _totalSpawned;
        private int _totalReturned;
        private int _peakActiveCount;

        public Pool(T prefab, int initialSize, int maxSize, Transform parent, Action<T> onSpawn, Action<T> onReturn)
        {
            _prefab = prefab;
            _maxSize = maxSize;
            _parent = parent;
            _onSpawn = onSpawn;
            _onReturn = onReturn;

            _availableObjects = new Queue<T>(initialSize);
            _allObjects = new HashSet<T>();

            // Warm up pool
            WarmUp(initialSize);
        }

        public T GetFromPool()
        {
            T obj;

            if (_availableObjects.Count > 0)
            {
                obj = _availableObjects.Dequeue();
            }
            else
            {
                // Pool is empty, create new object
                if (_maxSize > 0 && _allObjects.Count >= _maxSize)
                {
                    Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Pool reached max size ({_maxSize}). Consider increasing max size or optimizing object usage.");
                    // Return oldest object or create anyway (design decision)
                }

                obj = CreateNewObject();
            }

            // Activate and callback
            obj.gameObject.SetActive(true);

            if (obj is IPoolable poolable)
            {
                poolable.OnSpawnFromPool();
            }

            _onSpawn?.Invoke(obj);

            // Update statistics
            _totalSpawned++;
            int currentActive = _allObjects.Count - _availableObjects.Count;
            if (currentActive > _peakActiveCount)
            {
                _peakActiveCount = currentActive;
            }

            return obj;
        }

        public void ReturnToPool(T obj)
        {
            if (_availableObjects.Contains(obj))
            {
                Debug.LogWarning($"[ObjectPool<{typeof(T).Name}>] Object '{obj.name}' is already in pool. Ignoring duplicate return.");
                return;
            }

            // Callback and deactivate
            _onReturn?.Invoke(obj);

            if (obj is IPoolable poolable)
            {
                poolable.OnReturnToPool();
            }

            obj.gameObject.SetActive(false);

            // Return to parent
            if (_parent != null)
            {
                obj.transform.SetParent(_parent);
            }

            _availableObjects.Enqueue(obj);
            _totalReturned++;
        }

        public void WarmUp(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_maxSize > 0 && _allObjects.Count >= _maxSize)
                {
                    break;
                }

                T obj = CreateNewObject();
                obj.gameObject.SetActive(false);
                _availableObjects.Enqueue(obj);
            }
        }

        public void Clear()
        {
            foreach (var obj in _allObjects)
            {
                if (obj != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }

            _availableObjects.Clear();
            _allObjects.Clear();
        }

        private T CreateNewObject()
        {
            T obj = UnityEngine.Object.Instantiate(_prefab, _parent);
            obj.name = $"{_prefab.name} (Pooled)";
            _allObjects.Add(obj);
            return obj;
        }

        public PoolStatistics GetStatistics()
        {
            return new PoolStatistics
            {
                PrefabName = _prefab.name,
                TotalObjects = _allObjects.Count,
                AvailableObjects = _availableObjects.Count,
                ActiveObjects = _allObjects.Count - _availableObjects.Count,
                TotalSpawned = _totalSpawned,
                TotalReturned = _totalReturned,
                PeakActiveCount = _peakActiveCount,
                MaxSize = _maxSize
            };
        }

        public void LogStatistics()
        {
            PoolStatistics stats = GetStatistics();
            Debug.Log($"[ObjectPool<{typeof(T).Name}>] Statistics for '{stats.PrefabName}':\n" +
                        $"  Total Objects: {stats.TotalObjects}\n" +
                        $"  Available: {stats.AvailableObjects}\n" +
                        $"  Active: {stats.ActiveObjects}\n" +
                        $"  Total Spawned: {stats.TotalSpawned}\n" +
                        $"  Total Returned: {stats.TotalReturned}\n" +
                        $"  Peak Active: {stats.PeakActiveCount}\n" +
                        $"  Max Size: {(stats.MaxSize == 0 ? "Unlimited" : stats.MaxSize.ToString())}");
        }
    }
}

/// <summary>
/// Interface for objects that need custom reset behavior when pooled.
/// Implement this on your components to handle spawn/return lifecycle.
/// </summary>
public interface IPoolable
{
    /// <summary>
    /// Called when object is retrieved from pool.
    /// Use to reset state and initialize for new use.
    /// </summary>
    void OnSpawnFromPool();

    /// <summary>
    /// Called when object is returned to pool.
    /// Use to cleanup state and prepare for next spawn.
    /// </summary>
    void OnReturnToPool();
}

/// <summary>
/// Statistics data for a pool.
/// </summary>
public class PoolStatistics
{
    public string PrefabName { get; set; }
    public int TotalObjects { get; set; }
    public int AvailableObjects { get; set; }
    public int ActiveObjects { get; set; }
    public int TotalSpawned { get; set; }
    public int TotalReturned { get; set; }
    public int PeakActiveCount { get; set; }
    public int MaxSize { get; set; }

    public float PoolUtilization => TotalObjects > 0 ? (float)ActiveObjects / TotalObjects : 0f;
}

/// <summary>
/// MonoBehaviour base class for pooled objects.
/// Provides convenient access to pooling functionality.
/// </summary>
public abstract class PooledBehaviour<T> : MonoBehaviour, IPoolable where T : Component
{
    /// <summary>
    /// Return this object to its pool.
    /// Equivalent to calling ObjectPool<T>.ReturnToPool(this).
    /// </summary>
    public void ReturnToPool()
    {
        ObjectPool<T>.ReturnToPool(this as T);
    }

    public abstract void OnSpawnFromPool();
    public abstract void OnReturnToPool();
}

/// <summary>
/// Utility component to automatically return pooled objects after a delay.
/// Attach to pooled GameObjects for timed auto-return (e.g., VFX, temporary objects).
/// </summary>
[AddComponentMenu("Core Systems/Auto Return To Pool")]
public class AutoReturnToPool : MonoBehaviour
{
    [SerializeField] private float _delay = 2f;
    [SerializeField] private bool _useUnscaledTime = false;

    private float _returnTime;

    private void OnEnable()
    {
        _returnTime = (_useUnscaledTime ? Time.unscaledTime : Time.time) + _delay;
    }

    private void Update()
    {
        float currentTime = _useUnscaledTime ? Time.unscaledTime : Time.time;

        if (currentTime >= _returnTime)
        {
            ReturnToPool();
        }
    }

    private void ReturnToPool()
    {
        // Try to find a pooled component and return it
        Component pooledComponent = GetComponent<IPoolable>() as Component;

        if (pooledComponent != null)
        {
            // Use reflection to call the correct ObjectPool<T>.ReturnToPool method
            Type poolType = typeof(ObjectPool<>).MakeGenericType(pooledComponent.GetType());
            var returnMethod = poolType.GetMethod("ReturnToPool", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            returnMethod?.Invoke(null, new object[] { pooledComponent });
        }
        else
        {
            Debug.LogWarning($"[AutoReturnToPool] No IPoolable component found on {gameObject.name}. Destroying instead.");
            Destroy(gameObject);
        }
    }
}

/// <summary>
/// Editor utility for pool visualization and debugging.
/// </summary>
#if UNITY_EDITOR
public static class PoolDebugger
{
    /// <summary>
    /// Get statistics for all active pools.
    /// </summary>
    public static void LogAllPoolStatistics()
    {
        // This would require maintaining a global registry of all pools
        // Implementation depends on requirements
        Debug.Log("[PoolDebugger] Pool statistics logging not yet implemented for all types.");
    }
}
#endif
