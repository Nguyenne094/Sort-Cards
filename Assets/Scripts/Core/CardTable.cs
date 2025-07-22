using UnityEngine;

[RequireComponent(typeof(ObjectGrid))]
public class CardTable : MonoBehaviour
{   
    [SerializeField] private int _initPad = 3;

    private ObjectGrid _objectGrid;
    private int _currentIndexPad = 0;

    private void Awake()
    {
        _objectGrid = GetComponent<ObjectGrid>();
    }

    private void OnEnable() {
        _objectGrid.OnPadCreated += SyncPadManager;
    }

    private void OnDisable() {
        _objectGrid.OnPadCreated -= SyncPadManager;
    }

    private void SyncPadManager(Pad pad)
    {
        PadManager.Instance.Pads.Add(pad);
        if (_currentIndexPad < _initPad)
        {
            PadManager.Instance.UnlockedPads.Add(pad);
            _currentIndexPad++;
        }
    }
}