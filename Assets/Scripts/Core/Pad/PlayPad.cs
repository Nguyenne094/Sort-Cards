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
    public bool IsPaid
    {
        get
        {
            return _isPaid;
        }
        private set
        {
            if (value && IsLocked)
            {
                Debug.LogWarning("Cannot set IsPaid to true while the pad is locked.");
                return;
            }

            _isPaid = value;

            if (value)
            {
                SetPaidVisual();
            }
            else
            {
                SetNotPaidVisual();
            }
        }
    }

    private bool _isLocked = true;
    private bool _isPaid = false;

    private void Awake()
    {
        SetLockedVisual();
        SetNotPaidVisual();
    }

    public void Unlock()
    {
        Debug.Log("Unlocking pad: " + name);
        IsLocked = false;
    }

    public void Pay()
    {
        if (IsLocked)
        {
            Debug.LogWarning("Cannot pay for a locked pad.");
            return;
        }

        IsPaid = true;
    }

    private void SetLockedVisual()
    {
        lockedVisual.SetActive(true);
    }
    private void SetUnlockedVisual()
    {
        lockedVisual.SetActive(false);
    }

    private void SetPaidVisual()
    {
        padObject.GetComponent<Renderer>().material.color = Color.green;
    }

    private void SetNotPaidVisual()
    {
        padObject.GetComponent<Renderer>().material.color = Color.red;
    }
}