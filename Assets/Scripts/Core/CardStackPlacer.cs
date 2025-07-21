using UnityEngine;

public class CardStackPlacer : MonoBehaviour
{
    [SerializeField] private Transform _cardStackHolder;

    public Vector2 LocalPosition { get; set; }
    public Vector2 Index { get; set; }
}