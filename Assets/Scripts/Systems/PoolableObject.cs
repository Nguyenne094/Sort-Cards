using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    [SerializeField] private string _poolTag;
    public string PoolTag { get => _poolTag; set => _poolTag = value; }
}
