using System.Collections.Generic;
using UnityEngine;

public class PadManager : Singleton<PadManager>
{
    [SerializeField] private List<Pad> _pads = new();
    [SerializeField] private List<Pad> _unlockedPads = new();
    public List<Pad> UnlockedPads { get => _unlockedPads; set => _unlockedPads = value; }
    public List<Pad> Pads { get => _pads; set => _pads = value; }
    public int TotalPads { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        TotalPads = _pads.Count;
    }
}