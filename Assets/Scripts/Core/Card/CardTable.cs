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
        _objectGrid.OnPlayPadCreated += SyncPadManager;
    }

    private void OnDestroy() {
        _objectGrid.OnPlayPadCreated -= SyncPadManager;
    }

    private void SyncPadManager(PlayPad pad)
    {
        _padManager.RegisterPad(pad);
    }
}
