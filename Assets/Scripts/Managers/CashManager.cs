using System;
using TMPro;
using UnityEngine;

public class CashManager : Singleton<CashManager>
{
    [SerializeField] private int _totalCash = 0;
    [SerializeField] private TMP_Text cashText;
    public int TotalCash => _totalCash;
    private const string TotalCashKey = "TotalCash";

    public Action<int?> OnCashChanged;

    protected override void Awake()
    {
        base.Awake();

        OnCashChanged += UpdateCashText;

        LoadTotalCash();
    }

    private void OnDestroy()
    {
        OnCashChanged -= UpdateCashText;
    }

    private void UpdateCashText(int? _)
    {
        if (cashText != null)
        {
            cashText.text = _totalCash.ToString();
        }
    }

    private void LoadTotalCash()
    {
        _totalCash = PlayerPrefs.GetInt(TotalCashKey, 0);
        UpdateCashText(null);
    }

    public void AddCash(int amount)
    {
        _totalCash += amount;
        PlayerPrefs.SetInt(TotalCashKey, _totalCash);
        PlayerPrefs.Save();
        UpdateCashText(null);
    }

    public void SubtractCash(int amount)
    {
        _totalCash = Mathf.Max(0, _totalCash - amount);
        PlayerPrefs.SetInt(TotalCashKey, _totalCash);
        PlayerPrefs.Save();
        UpdateCashText(null);
    }

    public void SetCash(int amount)
    {
        _totalCash = amount;
        PlayerPrefs.SetInt(TotalCashKey, _totalCash);
        PlayerPrefs.Save();
        UpdateCashText(null);
    }

    public bool TryPay(int amount)
    {
        if (_totalCash >= amount)
        {
            SubtractCash(amount);
            return true;
        }
        return false;
    }
}
