using UnityEngine;

public class Card : MonoBehaviour
{
    [SerializeField] private CardColor _color;
    public CardColor Color => _color;
}
