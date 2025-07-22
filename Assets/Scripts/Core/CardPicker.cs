using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class CardPicker : MonoBehaviour
{
    [SerializeField] private float _exchangeSpeed = 5f;
    private TouchController _touchController;
    private Pad _currentPad;
    private bool _isDealing = false;

    private void Awake()
    {
        _touchController = TouchController.Instance;
    }

    private void OnEnable()
    {
        TouchController.OnFingerDownAction += PickPad;
        TouchController.OnFingerDownAction += DoDealCoroutine;
    }

    private void OnDisable()
    {
        TouchController.OnFingerDownAction -= PickPad;
        TouchController.OnFingerDownAction -= DoDealCoroutine;
    }

    public void PickPad()
    {
        var pad = _touchController.GetObjectThroughRaycast<Pad>();
        if (pad == null) return;

        // Pick a new pad → select it
        if (_currentPad == null && pad.CardStack.Count > 0)
        {
            _currentPad = pad;
            _currentPad.IsHoldingStack = true;
        }
        // Pick the same stack again → unselect
        else if (_currentPad == pad)
        {
            _currentPad.IsHoldingStack = false;
            _currentPad = null;
        }
        // Pick a different stack → try to combine or cancel
        else if (_currentPad != null)
        {
            TryCombineOrMove(_currentPad, pad);
            _currentPad = null;
        }
    }

    private void TryCombineOrMove(Pad from, Pad to)
    {
        from.IsHoldingStack = false;
        to.IsHoldingStack = false;

        //Combine same color stacks
        if (from.GetTopCardColor() == to.GetTopCardColor())
        {
            Debug.Log("1");
            StartCoroutine(CombineStack(from, to));
        }
        //Move different color stacks
        else if (to.GetTopCardColor() != null && from.GetTopCardColor() != to.GetTopCardColor())
        {
            Debug.Log("2");
            StartCoroutine(StacksAreDifferentColor(from, to));
        }
        //Move empty stack
        else if (to.CardStack.Count == 0)
        {
            Debug.Log("3");
            StartCoroutine(MoveStackToNewPad(from, to));
        }
    }

    private IEnumerator CombineStack(Pad from, Pad to)
    {
        var (color, fromStack) = from.PopTopCardStack();
        if (color == null || fromStack.Count == 0) yield break;

        while (fromStack.Count > 0)
            yield return StartCoroutine(MoveCardTo(fromStack.Pop(), to));
        
        if (to.TryGetComponent<CashExchanger>(out var cashExchanger))
        {
            Debug.Log(to.CardStack.Count);
            if (to.CardStack.Count >= cashExchanger.MaxCard)
            {
                yield return StartCoroutine(ExchangeCash(to, cashExchanger));
            }
        }
    }

    private IEnumerator MoveStackToNewPad(Pad from, Pad to)
    {
        var (color, fromStack) = from.PopTopCardStack();
        if (fromStack.Count == 0) yield break;

        while (fromStack.Count > 0)
            yield return StartCoroutine(MoveCardTo(fromStack.Pop(), to));

        if (to.TryGetComponent<CashExchanger>(out var cashExchanger))
        {
            Debug.Log(to.CardStack.Count);
            if (to.CardStack.Count >= cashExchanger.MaxCard)
            {
                yield return StartCoroutine(ExchangeCash(to, cashExchanger));
            }
        }
    }

    private IEnumerator MoveCardTo(Card card, Pad targetPad)
    {
        Vector3 targetGlobalPosition = targetPad.GetPositionToAdd(card.transform);
        Vector3 targetLocalPosition = targetPad.GetPositionToAdd(card.transform, false);

        while (Vector3.Distance(card.transform.position, targetGlobalPosition) > 0.01f)
        {
            card.transform.position = Vector3.MoveTowards(card.transform.position, targetGlobalPosition, Time.deltaTime * _exchangeSpeed);
            yield return null;
        }

        card.transform.SetParent(targetPad.StackHolder);
        card.transform.localPosition = targetLocalPosition;
        targetPad.PushCardWithoutCreate(card);
    }

    private IEnumerator StacksAreDifferentColor(Pad from, Pad to)
    {
        from.IsHoldingStack = false;
        yield return StartCoroutine(ShakePad(to));
    }

    private void DoDealCoroutine()
    {
        StartCoroutine(Deal());
    }

    private IEnumerator Deal()
    {
        if (_isDealing) yield break;
        _isDealing = true;
        var dealBtn = _touchController.GetObjectThroughRaycast<DealBtn>();
        var unlockedPads = PadManager.Instance.UnlockedPads;
        if (dealBtn == null || unlockedPads.Count == 0) yield break;

        //TODO: Create a new stack of cards with random colors for each pad
        //Repeat for all unlocked pads
        for (int i = 0; i < unlockedPads.Count; i++)
        {
            var pad = unlockedPads[i];
            if (pad == null) continue;

            var randomAmount = UnityEngine.Random.Range(1, 10);
            for (int j = 0; j < randomAmount; j++)
            {
                var randomColor = (CardColor)UnityEngine.Random.Range(0, Enum.GetValues(typeof(CardColor)).Length);
                var card = ObjectPool.Instance.GetObject(randomColor.ToString() + "Card").GetComponent<Card>();
                card.transform.position = dealBtn.transform.position;

                yield return StartCoroutine(MoveCardTo(card, pad));
            }

            Debug.Log($"Dealt {randomAmount} cards.");
            yield return null;
        }

        _isDealing = false;
    }

    private IEnumerator ShakePad(Pad pad)
    {
        Quaternion originRotation = pad.transform.localRotation;
        float duration = 0.5f;
        float elapsed = 0f;
        float magnitude = 10f;

        float seed = Random.value * 100f;

        while (elapsed < duration)
        {
            float zOffset = (Mathf.PerlinNoise(seed, Time.time * 20f) - 0.5f) * 2f * magnitude;
            pad.transform.localRotation = originRotation * Quaternion.Euler(0, 0, zOffset);
            pad.StackHolder.localRotation = Quaternion.Euler(0, 0, zOffset);
            elapsed += Time.deltaTime;
            yield return null;
        }

        pad.transform.localRotation = originRotation;
        pad.StackHolder.localRotation = Quaternion.identity;
    }

    private IEnumerator ScaleDown(Card card)
    {
        Vector3 targetScale = Vector3.zero;
        float duration = 0.1f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            card.transform.localScale = Vector3.Lerp(card.transform.localScale, targetScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        var poolable = card.GetComponent<PoolableObject>();
        ObjectPool.Instance.ReleaseObject(poolable.PoolTag, poolable);
    }

    private IEnumerator ExchangeCash(Pad pad, CashExchanger cashExchanger)
    {
        
        int exchangedCount = 0;
        while (!pad.IsStackEmpty())
        {
            var card = pad.PopCardWithoutDestroy();
            if (card == null) break;
            exchangedCount++;
            yield return StartCoroutine(ScaleDown(card));
        }
        cashExchanger.AllCardsExchanged?.Invoke(exchangedCount);
    }
}
