using System;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;

public class Pad : MonoBehaviour
{
    [Header("Pad Settings")]
    [SerializeField] private Transform _cardStackHolder;
    [SerializeField] private Transform _pad;
    [SerializeField, Min(0)] private float _zOffsetWhenHoldingStack = 1f;

    [Header("Ingame Settings")]
    [SerializeField, Min(0)] private int _cost;

    public Action<Card> OnCardPushed;
    public Action<Card> OnCardPopped;

    public Vector2 Index { get; set; }
    public bool IsPadUnlocked { get; set; }
    public bool IsHoldingStack
    {
        get
        {
            return _isHoldingStack;
        }
        set
        {
            if (_isHoldingStack == value) return;
            _isHoldingStack = value;
            if (value)
            {
                _topColorStack = PopTopCardStack().Item2;
                foreach (var card in _topColorStack)
                {
                    card.transform.localPosition = new Vector3(0, 0, card.transform.localPosition.z - _zOffsetWhenHoldingStack);
                    var outline = card.GetComponent<Outline>();
                    if (outline != null) outline.enabled = true;
                }
            }
            else
            {
                foreach (var card in _topColorStack)
                {
                    card.transform.localPosition = new Vector3(0, 0, card.transform.localPosition.z + _zOffsetWhenHoldingStack);
                    var outline = card.GetComponent<Outline>();
                    if (outline != null) outline.enabled = false;
                }
                PushStackWithoutCreate(_topColorStack);
            }
        }
    }
    public float PadScaleZ => _pad.localScale.z;
    public Transform StackHolder => _cardStackHolder;
    public Stack<Card> CardStack => _cardStack;

    private Stack<Card> _cardStack = new();
    private Stack<Card> _topColorStack = new();
    private bool _isHoldingStack;

#region Top Color Stack Operations

    public (CardColor?, Stack<Card>) PopTopCardStack()
    {
        Stack<Card> topCardStack = new();
        if (_cardStack.Count == 0)
            return (null, topCardStack);

        Card topCard = PopCardWithoutDestroy();
        CardColor targetColor = topCard.Color;
        topCardStack.Push(topCard);

        while (_cardStack.Count > 0 && _cardStack.Peek().Color == targetColor)
        {
            topCardStack.Push(PopCardWithoutDestroy());
        }

        return (targetColor, topCardStack);
    }
    
    public void PushStackWithoutCreate(Stack<Card> stack)
    {
        if (stack == null || stack.Count == 0)
        {
            Debug.LogWarning("Cannot push an empty or null stack onto the pad.");
            return;
        }

        while (stack.Count > 0)
        {
            Card card = stack.Pop();
            PushCardWithoutCreate(card);
        }
    }

    public CardColor? GetTopCardColor()
    {
        if (_cardStack.Count == 0)
            return null;

        Card topCard = _cardStack.Peek();
        return topCard.Color;
    }
#endregion

#region Card Stack Operations
        public Card PopCardWithoutDestroy()
        {
        if (_cardStack.Count > 0)
        {
            Card card = _cardStack.Pop();
            OnCardPopped?.Invoke(card);
            return card;
        }
        else
        {
            Debug.LogWarning("No cards to pop from the stack.");
        }
            return null;
        }
    
        public void PushCardWithoutCreate(Card card)
        {
            if (card == null)
            {
                Debug.LogWarning("Cannot push a null card onto the stack.");
                return;
            }
    
            _cardStack.Push(card);
            OnCardPushed?.Invoke(card);
        }
    
        [Button("Generate Card Stack")]
        public void GenerateCardStack()
        {
            var randomAmount = UnityEngine.Random.Range(1, 10);
    
            for (int i = 1; i <= randomAmount; i++)
            {
                var obj = ObjectPool.Instance.GetObject("RedCard");
                if (obj == null)
                {
                    Debug.LogWarning("No object available in the pool for 'RedCard'.");
                    return;
                }
                var card = obj.GetComponent<Card>();
                if (card == null)
                {
                    Debug.LogWarning("Card component not found on the object.");
                    continue;
                }
    
                card.transform.SetParent(_cardStackHolder);
                card.transform.localPosition = new Vector3(0, 0, -_pad.localScale.z / 2 - (obj.transform.localScale.z / 2 * (i * 2 - 1)));
                PushCardWithoutCreate(card);
            }
        }
    
        [Button("Clear Card Stack")]
        public void RemoveAllCards()
        {
            while (_cardStack.Count > 0)
            {
                Card card = PopCardWithoutDestroy();
                Destroy(card.gameObject);
            }
        }
#endregion

#region Utility Methods
        public bool IsStackEmpty() => _cardStack.Count == 0;
        public void MoveStackFrom(Stack<Card> from)
        {
            if (from == null || from.Count == 0) return ;

            Stack<Card> temp = new Stack<Card>(from.Reverse());
            from.Clear();

            while (temp.Count > 0)
                PushCardWithoutCreate(temp.Pop());

            return;
        }

    [Button("Log Stack")]
    private void LogStack()
    {
        Debug.Log($"Pad {name} Stack Count: {_cardStack.Count}");
    }

        public bool IsAllCardsSameColor()
    {
        if (_cardStack.Count == 0)
            return false;

        CardColor? firstCardColor = _cardStack.Peek().Color;
        return _cardStack.All(card => card.Color == firstCardColor);
    }

        public Vector3 GetPositionToAdd(Transform cardToAdd, bool isWorldPosition = true)
        {
            Vector3 targetPosition;
            if (_cardStack.Count > 0)
            {
                var card = _cardStack.Peek();
                targetPosition = isWorldPosition ? card.transform.position : card.transform.localPosition - new Vector3(0, 0, card.transform.localScale.z);
            }
            else
            {
                targetPosition = new Vector3(0, 0, -_pad.localScale.z / 2 - cardToAdd.transform.localScale.z / 2);
            }
    
            return targetPosition;
        }
#endregion
}