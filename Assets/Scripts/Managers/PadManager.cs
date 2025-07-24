using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PadManager : Singleton<PadManager>
{
    [SerializeField] private List<PlayPad> pads = new();
    [SerializeField] private List<PlayPad> unlockedPads = new();
    [SerializeField] private List<PlayPad> paidPads = new();

    [SerializeField] private Transform cardTable;
    [SerializeField] private Transform cashExchangeTable;

    public Transform CardTable => cardTable;
    public Transform CashExchangeTable => cashExchangeTable;
    public List<PlayPad> Pads => pads;
    public List<PlayPad> UnlockedPads => unlockedPads;
    public List<PlayPad> PaidPads => paidPads;

    public Action<PlayPad> OnPadUnlocked;
    public Action<PlayPad> OnPadPaid;
    public Action<int> OnPhaseCompleted;
    public Action OnAllPadsPaid;

    public void RegisterPad(PlayPad pad)
    {
        if (pad != null && !pads.Contains(pad))
        {
            pads.Add(pad);
        }
    }
    public void UnregisterPad(PlayPad pad)
    {
        if (pad != null)
        {
            pads.Remove(pad);
            unlockedPads.Remove(pad);
        }
    }

    public bool TryPayPadByCash(PlayPad pad)
    {
        if (pad == null || pad.IsLocked || !unlockedPads.Contains(pad))
        {
            Debug.LogWarning($"Pad {pad?.name} is either null, locked, or not unlocked.");
            return false;
        }
        if (!CashManager.Instance.TryPay(pad.UnlockCost))
            return false;
        PayPad(pad);
        return true;
    }

    public void PayPad(PlayPad pad)
    {
        Debug.Log($"Paying for pad: {pad.name}");
        if (pad == null || !unlockedPads.Contains(pad) || paidPads.Contains(pad)) return;
        pad.Pay();
        paidPads.Add(pad);
        Debug.Log(paidPads.Contains(pad));
        OnPadPaid?.Invoke(pad);
        CheckCurrentPhaseCompletion();
        if (paidPads.Count == pads.Count)
            OnAllPadsPaid?.Invoke();
    }

    public void UnlockPad(PlayPad pad)
    {
        if (pad == null || unlockedPads.Contains(pad)) return;
        pad.Unlock();
        unlockedPads.Add(pad);
        OnPadUnlocked?.Invoke(pad);
        if (unlockedPads.Count == pads.Count)
            OnPadUnlocked?.Invoke(pad);
    }

    public PlayPad GetPadByIndex(Vector2 index)
    {
        return pads.FirstOrDefault(p => Equals(p.Index, index));
    }

    private void CheckCurrentPhaseCompletion()
    {
        var levelManager = LevelManager.Instance;
        if (levelManager == null || levelManager.CurrentLevel == null) return;
        int currentPhaseIndex = levelManager.GetCurrentPhaseIndex();
        var currentPhase = levelManager.CurrentLevel.GetPhase(currentPhaseIndex);
        if (currentPhase == null) return;
        currentPhase.GetAllPadIndexes().ForEach(idx => Debug.Log($"Pad Index: {idx}"));
        bool allPaid = currentPhase.GetAllPadIndexes().All(idx =>
            paidPads.Any(p => Equals(p.Index, idx)));

        if (allPaid)
            OnPhaseCompleted?.Invoke(currentPhaseIndex);
    }

    private void HandlePadExchange(Pad pad, int cardCount)
    {
        if (pad is ExchangePad exchangePad)
        {
            int cash = cardCount * exchangePad.CashPerCard;
            CashManager.Instance.AddCash(cash);
        }
    }

    public void ResetForNewLevel()
    {
        pads.Clear();
        unlockedPads.Clear();
        paidPads.Clear();
        var cashExchangeGrid = cashExchangeTable.GetComponent<ObjectGrid>();
        foreach (var pad in cashExchangeGrid.GetComponentsInChildren<Pad>())
        {
            pad.ClearStack();
        }
    }
}
