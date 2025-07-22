using UnityEngine;

public enum CardColor
{
    Red,
    Blue,
    Purple,
    Orange,
    Green,
    Yellow
}

public class Card : MonoBehaviour
{
    [SerializeField] private CardColor _color;
    public CardColor Color => _color;
}