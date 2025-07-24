using UnityEngine;

public class PlayPad : Pad
{
    [SerializeField] private GameObject lockedVisual;

    public bool IsLocked
    {
        get => _isLocked;
        private set
        {
            _isLocked = value;
            if (_isLocked)
            {
                SetLockedVisual();
            }
            else
            {
                SetUnlockedVisual();
            }
        }
    }
    public int UnlockCost { get; set; }

    private bool _isLocked = true;

    private void Awake()
    {
        SetLockedVisual();
    }

    public void Unlock()
    {
        IsLocked = false;
    }

    public void Setup(int unlockCost)
    {
        UnlockCost = unlockCost;
    }

    private void SetLockedVisual()
    {
        lockedVisual.SetActive(true);
    }
    private void SetUnlockedVisual()
    {
        lockedVisual.SetActive(false);
    }
}