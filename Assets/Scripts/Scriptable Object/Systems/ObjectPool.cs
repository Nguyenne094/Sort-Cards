using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : Singleton<ObjectPool>
{
    [SerializeField] private List<GameObject> poolableObjects;
    Dictionary<string, ObjectPool<PoolableObject>> pools = new();

    protected override void Awake()
    {
        base.Awake();
        foreach (var obj in poolableObjects)
        {
            var poolable = obj.GetComponent<PoolableObject>();
            if (poolable != null)
            {
                pools[poolable.PoolTag] = new ObjectPool<PoolableObject>(
                    createFunc: () => {
                        var instance = Instantiate(obj);
                        instance.gameObject.SetActive(false);
                        return instance.GetComponent<PoolableObject>();
                    },
                    actionOnGet: o => o.gameObject.SetActive(true),
                    actionOnRelease: o => o.gameObject.SetActive(false),
                    actionOnDestroy: o => Destroy(o.gameObject),
                    collectionCheck: true,
                    defaultCapacity: 20,
                    maxSize: 100
                );
            }
            else
            {
                Debug.LogWarning($"Object {obj.name} does not have a PoolableObject component.");
            }
        }
    }

    public PoolableObject GetObject(string poolTag)
    {
        if (pools.TryGetValue(poolTag, out var pool))
        {
            return pool.Get();
        }
        else
        {
            Debug.LogWarning($"No pool found for tag: {poolTag}");
            return null;
        }
    }

    public void ReleaseObject(string poolTag, PoolableObject obj)
    {
        if (pools.TryGetValue(poolTag, out var pool))
        {
            pool.Release(obj);
        }
        else
        {
            Debug.LogWarning($"No pool found for tag: {poolTag}");
        }
    }
}