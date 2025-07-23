using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField] private List<GameObject> _poolablePrefabs;
    private Dictionary<string, ObjectPool<PoolableObject>> _pools = new();

    protected override void Awake()
    {
        base.Awake();
        foreach (var prefab in _poolablePrefabs)
        {
            var poolable = prefab.GetComponent<PoolableObject>();
            if (poolable == null)
            {
                Debug.LogWarning($"Prefab {prefab.name} lacks PoolableObject.");
                continue;
            }
            _pools[poolable.PoolTag] = new ObjectPool<PoolableObject>(
                () => Instantiate(prefab).GetComponent<PoolableObject>(),
                o => o.gameObject.SetActive(true),
                o => o.gameObject.SetActive(false),
                o => Destroy(o.gameObject),
                true, 20, 100
            );
        }
    }

    public PoolableObject GetObject(string tag)
    {
        if (_pools.TryGetValue(tag, out var pool))
            return pool.Get();
        Debug.LogWarning($"No pool for tag {tag}");
        return null;
    }

    public void ReleaseObject(string tag, PoolableObject obj)
    {
        if (_pools.TryGetValue(tag, out var pool))
            pool.Release(obj);
        else
            Destroy(obj.gameObject);
    }

    public void ClearAllPools()
    {
        foreach (var pool in _pools.Values) pool.Clear();
    }
}
