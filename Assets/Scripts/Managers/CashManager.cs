using UnityEngine;

public class CashManager : Singleton<CashManager>
{
    [SerializeField] private int _totalCash = 0;
    public int TotalCash => _totalCash;
    private const string TotalCashKey = "TotalCash";

    protected override void Awake()
    {
        base.Awake();
        LoadTotalCash();
    }

    private void LoadTotalCash()
    {
        _totalCash = PlayerPrefs.GetInt(TotalCashKey, 0);
    }

    public void AddCash(int amount)
    {
        _totalCash += amount;
        PlayerPrefs.SetInt(TotalCashKey, _totalCash);
        PlayerPrefs.Save();
    }

    public void SubtractCash(int amount)
    {
        _totalCash = Mathf.Max(0, _totalCash - amount);
        PlayerPrefs.SetInt(TotalCashKey, _totalCash);
        PlayerPrefs.Save();
    }

    public void SetCash(int amount)
    {
        _totalCash = amount;
        PlayerPrefs.SetInt(TotalCashKey, _totalCash);
        PlayerPrefs.Save();
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
