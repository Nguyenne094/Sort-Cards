using UnityEngine;

public class PlayPad : Pad
{
    public bool IsLocked { get; private set; } = true;
    public int UnlockCost { get; set; }

    private void Awake()
    {
        SetLockedVisual();
    }

    public void Unlock()
    {
        IsLocked = false;
        SetUnlockedVisual();
    }

    public void Setup(int unlockCost)
    {
        UnlockCost = unlockCost;
    }

    private void SetLockedVisual()
    {
        if (padObject && padObject.TryGetComponent(out Renderer renderer))
            renderer.material.color = Color.gray;
    }
    private void SetUnlockedVisual()
    {
        if (padObject && padObject.TryGetComponent(out Renderer renderer))
            renderer.material.color = Color.white;
    }
}