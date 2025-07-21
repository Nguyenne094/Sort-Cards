using UnityEngine;

public class Singleton<T> : MonoBehaviour where T : Component
{
    public bool Persist = true;
    
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                SetupInstance();
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        RemoveDuplicate();
    }

    private static void SetupInstance()
    {
        _instance = FindFirstObjectByType<T>();

        if (_instance == null)
        {
            GameObject gameObj = new GameObject();
            gameObj.name = typeof(T).Name;
            _instance = gameObj.AddComponent<T>();

            if (_instance is Singleton<T> singleton && singleton.Persist)
            {
                DontDestroyOnLoad(gameObj);
            }
        }
    }

    private void RemoveDuplicate()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this as T;
            if (Persist)
            {
                DontDestroyOnLoad(gameObject);
            }
        }
    }
}