using UnityEngine;

/// <summary>
/// Centralized game configuration and constants
/// </summary>
[CreateAssetMenu(fileName = "GameConfig", menuName = "Game/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Card Settings")]
    [SerializeField] private float cardMoveSpeed = 5f;
    [SerializeField] private float cardStackZOffset = 1f;
    
    [Header("Animation Settings")]
    [SerializeField] private float padShakeDuration = 0.5f;
    [SerializeField] private float padShakeMagnitude = 10f;
    [SerializeField] private float cardScaleDownDuration = 0.1f;
    
    [Header("Visual Settings")]
    [SerializeField] private Color lockedPadColor = Color.red;
    [SerializeField] private Color unlockedPadColor = Color.grey;
    
    [Header("Deal Settings")]
    [SerializeField] private int minCardsPerDeal = 1;
    [SerializeField] private int maxCardsPerDeal = 10;
    
    // Properties
    public float CardMoveSpeed => cardMoveSpeed;
    public float CardStackZOffset => cardStackZOffset;
    public float PadShakeDuration => padShakeDuration;
    public float PadShakeMagnitude => padShakeMagnitude;
    public float CardScaleDownDuration => cardScaleDownDuration;
    public Color LockedPadColor => lockedPadColor;
    public Color UnlockedPadColor => unlockedPadColor;
    public int MinCardsPerDeal => minCardsPerDeal;
    public int MaxCardsPerDeal => maxCardsPerDeal;
    
    // Singleton access
    private static GameConfig _instance;
    public static GameConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<GameConfig>("GameConfig");
            return _instance;
        }
    }
}