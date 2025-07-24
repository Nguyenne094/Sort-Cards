using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ExchangePad : Pad
{
    [Header("Exchange Settings")]
    [SerializeField, Min(0)] private int cardsRequiredForExchange = 10;
    [SerializeField, Min(0)] private int cashPerCard = 1;

    [Header("References")]
    [SerializeField] private Slider cardsRequiredSlider;

    public int CardsRequiredForExchange => cardsRequiredForExchange;
    public int CashPerCard => cashPerCard;

    private void Start()
    {
        OnAllCardsExchanged += PlayCashExchangeAnimation;
        OnCardAdded += UpdateCardsRequiredSlider;
        OnCardRemoved += UpdateCardsRequiredSlider;
    }

    private void OnDestroy()
    {
        OnAllCardsExchanged -= PlayCashExchangeAnimation;
        OnCardAdded -= UpdateCardsRequiredSlider;
        OnCardRemoved -= UpdateCardsRequiredSlider;
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

    private void UpdateCardsRequiredSlider(Card card)
    {
        if (cardsRequiredSlider != null)
        {
            StopAllCoroutines();
            StartCoroutine(UpdateSliderValue(CardStack.Count / (float)cardsRequiredForExchange));
        }
    }

    private IEnumerator UpdateSliderValue(float targetValue)
    {
        float startValue = cardsRequiredSlider.value;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cardsRequiredSlider.value = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            yield return null;
        }

        cardsRequiredSlider.value = targetValue;
    }
}