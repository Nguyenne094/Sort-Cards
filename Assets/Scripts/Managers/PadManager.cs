using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PadManager : Singleton<PadManager>
{
    [SerializeField] private List<PlayPad> pads = new();
    [SerializeField] private List<PlayPad> unlockedPads = new();

    [SerializeField] private Transform cardTable;
    [SerializeField] private Transform cashExchangeTable;

    public Transform CardTable => cardTable;
    public Transform CashExchangeTable => cashExchangeTable;
    public List<PlayPad> Pads => pads;
    public List<PlayPad> UnlockedPads => unlockedPads;

    public Action<PlayPad> OnPadUnlocked;
    public Action<int> OnPhaseCompleted;
    public Action OnAllPadsUnlocked;

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

    public bool TryUnlockPadByCash(PlayPad pad)
    {
        if (pad == null || !pad.IsLocked || unlockedPads.Contains(pad))
            return false;
        if (!CashManager.Instance.TryPay(pad.UnlockCost))
            return false;
        UnlockPad(pad);
        return true;
    }

    public void UnlockPad(PlayPad pad)
    {
        if (pad == null || unlockedPads.Contains(pad)) return;
        unlockedPads.Add(pad);
        pad.Unlock();
        OnPadUnlocked?.Invoke(pad);
        CheckCurrentPhaseCompletion();
        if (unlockedPads.Count == pads.Count)
            OnAllPadsUnlocked?.Invoke();
    }

    public PlayPad GetPadByIndex(Vector2 index)
    {
        return pads.FirstOrDefault(p => p.Index == index);
    }

    private void CheckCurrentPhaseCompletion()
    {
        var levelManager = LevelManager.Instance;
        if (levelManager == null || levelManager.CurrentLevel == null) return;
        int currentPhaseIndex = levelManager.GetCurrentPhaseIndex();
        var currentPhase = levelManager.CurrentLevel.GetPhase(currentPhaseIndex);
        if (currentPhase == null) return;
        bool allUnlocked = currentPhase.GetAllPadIndexes().All(idx =>
            unlockedPads.Any(p => p.Index == idx));
        if (allUnlocked)
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
        var cashExchangeGrid = cashExchangeTable.GetComponent<ObjectGrid>();
        foreach (var pad in cashExchangeGrid.GetComponentsInChildren<Pad>())
        {
            pad.ClearStack();
        }
    }
}
