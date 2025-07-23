using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public abstract class Pad : MonoBehaviour
{
    [Header("Pad Settings")]
    [SerializeField] protected Transform cardStackHolder;
    [SerializeField] protected Transform padObject;

    // Events
    public Action<Card> OnCardAdded;
    public Action<Card> OnCardRemoved;
    public Action<Pad, int> OnAllCardsExchanged;


    public Vector2 Index { get; set; }
    public Stack<Card> CardStack => _cardStack;
    public Transform StackHolder => cardStackHolder;
    public Transform PadObject => padObject;
    public bool IsWorking { get; set; } = false;

    public bool IsHoldingStack
    {
        get => _isHoldingStack;
        set
        {
            if (_isHoldingStack == value) return;
            _isHoldingStack = value;
            if (value)
            {
                if (_cardStack.Count > 0)
                {
                    _holdingStack = PopTopCardStack().Item2;
                    foreach (var card in _holdingStack)
                    {
                        card.transform.localPosition = new Vector3(0, 0, card.transform.localPosition.z - 0.5f);
                        card.GetComponent<Outline>().enabled = true;
                    }
                }
            }
            else
            {
                if (_holdingStack.Count > 0)
                {
                    foreach (var card in _holdingStack)
                    {
                        card.transform.localPosition = new Vector3(0, 0, card.transform.localPosition.z + 0.5f);
                        card.GetComponent<Outline>().enabled = false;
                    }
                    PushStackCards(_holdingStack);
                }
            }
        }
    }


    protected Stack<Card> _cardStack = new();
    private Stack<Card> _holdingStack = new();
    protected bool _isHoldingStack;

    public (CardColor?, Stack<Card>) PopTopCardStack()
    {
        Stack<Card> topStack = new();
        if (_cardStack.Count == 0)
            return (null, topStack);

        Card topCard = _cardStack.Peek();
        CardColor targetColor = topCard.Color;
        while (_cardStack.Count > 0 && _cardStack.Peek().Color == targetColor)
        {
            topStack.Push(PopCardWithoutDestroy());
        }
        return (targetColor, topStack);
    }

    public CardColor? GetTopCardColor()
    {
        return _cardStack.Count > 0 ? _cardStack.Peek().Color : null;
    }

    public bool IsStackEmpty() => _cardStack.Count == 0;

    public Vector3 GetPositionToAdd(Transform card, bool worldPosition = true)
    {
        if (card == null) return Vector3.zero;
        if (_cardStack.Count == 0)
            return (worldPosition ? cardStackHolder.position : cardStackHolder.localPosition) - new Vector3(0, 0, padObject.transform.localScale.z / 2 + card.localScale.z / 2);

        return (worldPosition ? _cardStack.Peek().transform.position : _cardStack.Peek().transform.localPosition) - new Vector3(0, 0, card.localScale.z / 2 + _cardStack.Peek().transform.localScale.z / 2);
    }

    public void PushCardWithoutCreate(Card card)
    {
        if (card == null) return;
        OnCardAdded?.Invoke(card);
        _cardStack.Push(card);
    }

    public Card PopCardWithoutDestroy()
    {
        if (_cardStack.Count == 0) return null;
        Card card = _cardStack.Pop();
        OnCardRemoved?.Invoke(card);
        return card;
    }

    public void PushStackCards(Stack<Card> stack)
    {
        if (stack == null || stack.Count == 0) return;
        while (stack.Count > 0)
        {
            PushCardWithoutCreate(stack.Pop());
        }
    }

    #region Debug Methods

    [Button("Log Stack")]
    public void LogStack()
    {
        Debug.Log($"Stack Count: {_cardStack.Count}");
        Debug.Log($"Holding Stack Count: {_holdingStack.Count}");
    }

    [Button("Add Card")]
    public void AddCard()
    {
        var card = ObjectPool.Instance.GetObject("RedCard").GetComponent<Card>();
        if (card == null) return;
        OnCardAdded?.Invoke(card);
        _cardStack.Push(card);
    }

    [Button("Remove Card")]
    public void RemoveCard()
    {
        if (_cardStack.Count == 0) return;
        var card = _cardStack.Pop();
        OnCardRemoved?.Invoke(card);
        Destroy(card.gameObject);
    }

    [Button("Clear Stack")]
    public void ClearStack()
    {
        while (_cardStack.Count > 0)
        {
            var card = _cardStack.Pop();
            OnCardRemoved?.Invoke(card);
            Destroy(card.gameObject);
        }
    }
        
#endregion
}
