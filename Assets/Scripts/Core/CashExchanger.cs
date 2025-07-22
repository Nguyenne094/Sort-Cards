using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Pad))]
public class CashExchanger : MonoBehaviour
{
    [SerializeField, Range(0, 100)] private int _maxCard;

    public Action<int> AllCardsExchanged;
    public int MaxCard => _maxCard;

    private Pad _pad;

    private void Awake()
    {
        _pad = GetComponent<Pad>();
        if (_pad == null)
        {
            Debug.LogError("CashExchanger requires a Pad component.");
        }
    }

    private void OnEnable()
    {
        AllCardsExchanged += PlayCashExchangeAnimation;
    }

    private void OnDisable()
    {
        AllCardsExchanged -= PlayCashExchangeAnimation;
    }

    private void PlayCashExchangeAnimation(int cardStackCount)
    {
        Debug.Log("Playing cash exchange animation.");
        StartCoroutine(CashExchangeAnimation(cardStackCount));
    }

    private IEnumerator CashExchangeAnimation(int cardStackCount)
    {
        // Implement the cash exchange animation logic here
        Debug.Log($"Exchanging {cardStackCount} cards for cash.");
        CashManager.Instance.AddCash(cardStackCount);
        yield return null;
    }
}