using UnityEngine;

[RequireComponent(typeof(ObjectGrid))]
public class CardTable : MonoBehaviour
{   
    private ObjectGrid _objectGrid;

    private PadManager _padManager;

    private void Awake()
    {
        _objectGrid = GetComponent<ObjectGrid>();
        _padManager = PadManager.Instance;
    }

    private void OnEnable() {
        _objectGrid.OnPlayPadCreated += SyncPadManager;
    }

    private void OnDisable() {
        _objectGrid.OnPlayPadCreated -= SyncPadManager;
    }

    private void SyncPadManager(PlayPad pad)
    {
        _padManager.RegisterPad(pad);
    }
}
