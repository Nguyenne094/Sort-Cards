using System;
using NaughtyAttributes;
using UnityEngine;

public class ObjectGrid : MonoBehaviour
{
    [SerializeField] private GameObject _padPrefab;
    [SerializeField] private GridDataSO _gridData;

    public GridDataSO GridDataSO { get => _gridData; set => _gridData = value; }

    private GameObject[,] grid;
    private PadManager _padManager;

    public Action<PlayPad> OnPlayPadCreated;
    public Action OnGridCreationComplete;
    public GameObject[,] Grid => grid;
    public bool GridForPlayPad => _padPrefab != null && _padPrefab.GetComponentInChildren<PlayPad>() != null;

    private void Awake()
    {
        _padManager = PadManager.Instance;
    }

    private void Start() {
        if (!GridForPlayPad) CreateGrid();
    }

    [Button]
    public void CreateGrid()
    {
        ClearGrid();
        if(_gridData == null || _padPrefab == null)
        {
            Debug.LogWarning("GridData or PadPrefab is not set.");
            return;
        }

        if (_gridData.Amount <= 0 || _gridData.Columns <= 0 || _padPrefab == null)
        {
            return;
        }

        int rows = Mathf.CeilToInt((float)_gridData.Amount / _gridData.Columns);
        grid = new GameObject[rows, _gridData.Columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < _gridData.Columns; j++)
            {
                if (i * _gridData.Columns + j < _gridData.Amount)
                {
                    // Calculate position to center the grid
                    // The y-axis is inverted (row 0 is at the top)
                    float xPos = (j - (_gridData.Columns - 1) / 2.0f) * (_gridData.CellSize + _gridData.OffsetX);
                    float yPos = ((rows - 1) / 2.0f - i) * (_gridData.CellSize + _gridData.OffsetY);
                    Vector3 localPosition = new Vector3(xPos, yPos, 0);

                    var padObject = ObjectPool.Instance.GetObject(_padPrefab.GetComponentInChildren<PoolableObject>().PoolTag).gameObject;
                    if (padObject == null)
                    {
                        Debug.LogWarning($"No object available in the pool for '{_padPrefab.name}'.");
                        continue;
                    }
                    padObject.transform.SetParent(transform);

                    grid[i, j] = padObject;
                    if (padObject.GetComponentInChildren<Pad>() is Pad pad)
                    {
                        padObject.transform.localPosition = localPosition;
                        pad.Index = new Vector2(i, j);
                        if (GridForPlayPad) OnPlayPadCreated?.Invoke((PlayPad)pad);
                    }
                    else
                    {
                        Debug.LogWarning($"Pad at ({i}, {j}) does not have a CardStackPlacer component.");
                    }
                    padObject.name = $"Pad {i * _gridData.Columns + j + 1}";
                }
            }
        }
        
        if (GridForPlayPad) OnGridCreationComplete?.Invoke();
    }

    [Button]
    public void ClearGrid()
    {
        if (grid == null)
        {
            return;
        }

        foreach (GameObject pab in grid)
        {
            if (pab != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(pab);
                }
                else
                {
                    DestroyImmediate(pab);
                }
            }
        }

        grid = new GameObject[0, 0];

        if (GridForPlayPad)
        {
            _padManager.Pads.Clear();
            _padManager.UnlockedPads.Clear();
        }
    }
}
