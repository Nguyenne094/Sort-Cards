using System;
using System.Collections;
using System.Collections.Generic;
using CandyCoded.HapticFeedback;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(TouchController))]
public class CardPicker : MonoBehaviour
{
    [Header("Card Picker Settings")]
    [SerializeField] private float _exchangeSpeed = 5f;
    [SerializeField] private AudioSource _flipCardAudioSource;
    [SerializeField] private AudioSource _cardExchangeAudioSource;
    [SerializeField] private AudioClip _wrongPadAudioClip;
    [SerializeField] private AudioClip _cantPayAudioClip;
    
    [SerializeField, Range(0f, 1f)] private float _volume = 0.5f;

    private TouchController _touchController;
    private Pad _currentPad;
    private bool _isDealing = false;
    private bool _isPicking = false;

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
        var pad = _touchController.GetObjectWithType<Pad>();
        if (pad == null || pad.IsWorking || _isDealing) return;

        Debug.Log("Selected pad: " + pad.name);
        PlayPad playpad = null;

        if (pad is PlayPad)
        {
            playpad = (PlayPad)pad;
            if (playpad.IsLocked) return;
        }


        // Pad is not paid -> try to pay
        if (playpad != null && !playpad.IsPaid)
        {
            if (PadManager.Instance.TryPayPadByCash(playpad))
            {
                return;
            }
            else
            {
                if (_currentPad != null && _currentPad != playpad)
                {
                    _currentPad.IsHoldingStack = false;
                    _currentPad = null;
                }
                SoundManager.Instance.PlaySFX(_cantPayAudioClip);
                StartCoroutine(ShakePad(playpad));
            }
            return;
        }

        // When pad is unlocked, handle it
        HandleUnlockedPad(pad);
    }

    private void HandleUnlockedPad(Pad pad)
    {
        // Pick a new pad → select it
        if (_currentPad == null && pad.CardStack.Count > 0) 
        {
            Debug.Log("Selecting new pad.");
            _currentPad = pad;
            _currentPad.IsHoldingStack = true;
        }
        // Pick the same stack again → unselect
        else if (_currentPad == pad)
        {
            Debug.Log("Unselecting current pad.");
            _currentPad.IsHoldingStack = false;
            _currentPad = null;
        }
        // Pick a different stack → try to combine or cancel
        else if (_currentPad != null)
        {
            Debug.Log("Trying to combine or move stacks.");
            StartCoroutine(TryCombineOrMove(_currentPad, pad));
        }
    }

    private IEnumerator TryCombineOrMove(Pad from, Pad to)
    {
        if (from.IsWorking || to.IsWorking)
        {
            Debug.Log("One of the pads is already working, skipping operation.");
            yield break;
        }

        from.IsWorking = true;
        to.IsWorking = true;
        from.IsHoldingStack = false;
        to.IsHoldingStack = false;

        //Combine same color stacks
        if (from.GetTopCardColor() == to.GetTopCardColor())
        {
            Debug.Log("1");
            yield return StartCoroutine(CombineStack(from, to));
        }
        //Move different color stacks
        else if (to.GetTopCardColor() != null && from.GetTopCardColor() != to.GetTopCardColor())
        {
            Debug.Log("2");
            yield return StartCoroutine(StacksAreDifferentColor(from, to));
        }
        //Move empty stack
        else if (to.CardStack.Count == 0)
        {
            Debug.Log("3");
            yield return StartCoroutine(MoveStackToNewPad(from, to));
        }

        yield return StartCoroutine(WorkingDone(from, to));
    }

    private IEnumerator CombineStack(Pad from, Pad to)
    {
        var (color, fromStack) = from.PopTopCardStack();
        if (color == null || fromStack.Count == 0) yield break;

        while (fromStack.Count > 0)
            yield return StartCoroutine(MoveCardTo(fromStack.Pop(), to));

        if (to is ExchangePad exchangePad)
        {
            Debug.Log(exchangePad.CardStack.Count);
            if (exchangePad.CardStack.Count >= exchangePad.CardsRequiredForExchange)
            {
                yield return StartCoroutine(ExchangeCash(exchangePad));
            }
        }
    }

    private IEnumerator MoveStackToNewPad(Pad from, Pad to)
    {
        var (color, fromStack) = from.PopTopCardStack();
        if (fromStack.Count == 0) yield break;

        while (fromStack.Count > 0)
            yield return StartCoroutine(MoveCardTo(fromStack.Pop(), to));

        if (to is ExchangePad exchangePad)
        {
            Debug.Log(exchangePad.CardStack.Count);
            if (exchangePad.CardStack.Count >= exchangePad.CardsRequiredForExchange)
            {
                yield return StartCoroutine(ExchangeCash(exchangePad));
            }
        }
    }

    private IEnumerator MoveCardTo(Card card, Pad targetPad)
    {
        if (card == null || targetPad == null) yield break;

        if (_flipCardAudioSource?.isPlaying == false)
        {
            _flipCardAudioSource.volume = _volume;
            _flipCardAudioSource.Play();
        }

        Vector3 startPos = card.transform.position;
        Vector3 endPos = targetPad.GetPositionToAdd(card.transform);
        Vector3 targetLocalPosition = targetPad.GetPositionToAdd(card.transform, false);
        Vector3 startRotation = card.transform.localRotation.eulerAngles;
        Vector3 endRotation = targetPad.PadObject.transform.localRotation.eulerAngles;

        float duration = .3f;
        float elapsed = 0f;
        float amplitude = 3f;

        bool moveUp = endPos.y > startPos.y;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 linearPos = Vector3.Lerp(startPos, endPos, t);
            Vector3 linearRotation = Vector3.Lerp(startRotation, endRotation, t);
            float curveRotationX = Mathf.Lerp(startRotation.x, moveUp ? endRotation.x + 180 : endRotation.x - 180, t);

            float sinOffset = Mathf.Sin(t * Mathf.PI) * amplitude; // PI equal to half a circle

            Vector3 curvedPos = linearPos - new Vector3(0, 0, sinOffset);
            Vector3 curvedRotation = linearRotation - new Vector3(curveRotationX, 0, 0);

            card.transform.position = curvedPos;
            card.transform.localRotation = Quaternion.Euler(curvedRotation);

            elapsed += Time.deltaTime;
            yield return null;
        }

        card.transform.position = endPos;
        card.transform.SetParent(targetPad.StackHolder);
        card.transform.localPosition = targetLocalPosition;
        card.transform.localRotation = Quaternion.identity;
        targetPad.PushCardWithoutCreate(card);

        if (_flipCardAudioSource?.isPlaying == true)
        {
            _flipCardAudioSource.volume = _volume;
            _flipCardAudioSource.Stop();
        }
    }

    private IEnumerator StacksAreDifferentColor(Pad from, Pad to)
    {
        from.IsHoldingStack = false;
        SoundManager.Instance.PlaySFX(_wrongPadAudioClip, _volume);
        HapticFeedback.MediumFeedback();
        yield return StartCoroutine(ShakePad(to));
    }

    private void DoDealCoroutine()
    {
        var dealBtn = _touchController.GetObjectWithType<DealBtn>();
        var paidPads = PadManager.Instance.PaidPads;
        if (dealBtn == null || paidPads.Count == 0 || _currentPad?.IsWorking == true) return;
        StartCoroutine(Deal(dealBtn, paidPads));
    }

    private IEnumerator Deal(DealBtn dealBtn, List<PlayPad> paidPads)
    {
        if (_isDealing) yield break;

        _isDealing = true;

        if (_flipCardAudioSource?.isPlaying == false)
        {
            _flipCardAudioSource.volume = _volume;
            _flipCardAudioSource.Play();
        }

        // If Player is picking cards, stop it
        if (_currentPad != null)
        {
            _currentPad.IsHoldingStack = false;
            _currentPad.IsWorking = false;
            _currentPad = null;
        }

        int colorLimit = Mathf.Min(paidPads.Count, CardUtility.GetTotalColors());

        for (int i = 0; i < paidPads.Count; i++)
        {
            var pad = paidPads[i];
            if (pad == null) continue;

            var randomAmount = Random.Range(1, 10);
            for (int j = 0; j < randomAmount; j++)
            {
                var randomColor = CardUtility.GetRandomColor(colorLimit - 1);
                var card = ObjectPool.Instance.GetObject(CardUtility.GetCardPoolTag(randomColor)).GetComponent<Card>();
                card.transform.position = dealBtn.transform.position;
                yield return StartCoroutine(MoveCardTo(card, pad));
            }
            yield return null;
        }

        if (_flipCardAudioSource?.isPlaying == true)
        {
            _flipCardAudioSource.volume = _volume;
            _flipCardAudioSource.Stop();
        }

        _isDealing = false;
    }

    private IEnumerator ShakePad(Pad pad)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        float magnitude = 10f;

        float seed = Random.value * 100f;

        var padTransform = pad.transform;

        while (elapsed < duration)
        {
            float zOffset = (Mathf.PerlinNoise(seed, Time.time * 20f) - 0.5f) * 2f * magnitude;
            padTransform.localRotation = Quaternion.identity * Quaternion.Euler(0, 0, zOffset);
            elapsed += Time.deltaTime;
            yield return null;
        }

        padTransform.localRotation = Quaternion.identity;
    }

    private IEnumerator ScaleDown(Card card)
    {
        var originScale = card.transform.localScale;
        var targetScale = Vector3.zero;
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
        card.transform.localScale = originScale; // Reset scale for next use
    }

    private IEnumerator ExchangeCash(ExchangePad pad)
    {

        int exchangedCount = 0;
        while (!pad.IsStackEmpty())
        {
            var card = pad.PopCardWithoutDestroy();
            if (card == null) break;
            if (_cardExchangeAudioSource?.isPlaying == false)
            {
                _cardExchangeAudioSource.volume = _volume;
                _cardExchangeAudioSource.Play();
            }

            exchangedCount++;
            yield return StartCoroutine(ScaleDown(card));

            if (_cardExchangeAudioSource?.isPlaying == true)
            {
                _cardExchangeAudioSource.volume = _volume;
                _cardExchangeAudioSource.Stop();
            }
        }
        pad.OnAllCardsExchanged?.Invoke(pad, exchangedCount);
    }

    private IEnumerator WorkingDone(Pad from, Pad to)
    {
        yield return null;
        from.IsWorking = false;
        to.IsWorking = false;
        _currentPad = null;
    }
}
