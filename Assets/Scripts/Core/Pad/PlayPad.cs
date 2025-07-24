using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayPad : Pad
{
    [Header("PlayPad Settings")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Image lockedVisual;
    [SerializeField] private TMP_Text costText;

    public bool IsLocked
    {
        get => _isLocked;
        private set
        {
            _isLocked = value;
            if (value)
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
        canvas.worldCamera = Camera.main;
        SetLockedVisual();
        SetNotPaidVisual();
    }

    public void Unlock()
    {
        Debug.Log("Unlocking pad: " + name);
        IsLocked = false;
        SetNotPaidVisual();
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
        lockedVisual.gameObject.SetActive(true);
    }
    private void SetUnlockedVisual()
    {
        lockedVisual.gameObject.SetActive(false);
    }

    private void SetPaidVisual()
    {
        costText.gameObject.SetActive(false);
    }

    private void SetNotPaidVisual()
    {
        if (IsLocked)
        {
            costText.gameObject.SetActive(false);
            return;
        }
        costText.gameObject.SetActive(true);
        SetCostText(UnlockCost);
    }

    public void SetCostText(int cost)
    {
        costText.text = cost + "$";
    }
}