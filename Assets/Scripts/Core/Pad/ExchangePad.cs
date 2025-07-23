using System.Collections;
using UnityEngine;

public class ExchangePad : Pad
{
    [Header("Exchange Settings")]
    [SerializeField, Min(0)] private int cardsRequiredForExchange = 10;
    [SerializeField, Min(0)] private int cashPerCard = 1;

    public int CardsRequiredForExchange => cardsRequiredForExchange;
    public int CashPerCard => cashPerCard;

    private void Start()
    {
        OnAllCardsExchanged += PlayCashExchangeAnimation;
    }

    private void OnDestroy()
    {
        OnAllCardsExchanged -= PlayCashExchangeAnimation;
    }

    private void PlayCashExchangeAnimation(Pad pad, int exchangedCount)
    {
        if (pad != this) return;
        StartCoroutine(CashExchangeAnimation(exchangedCount));
    }

    private IEnumerator CashExchangeAnimation(int cardStackCount)
    {
        // Implement the cash exchange animation logic here
        Debug.Log($"Exchanging {cardStackCount} cards for cash.");
        CashManager.Instance.AddCash(cardStackCount * cashPerCard);
        yield return null;
    }
}
