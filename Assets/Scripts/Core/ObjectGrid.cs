using System;
using NaughtyAttributes;
using UnityEngine;

public class ObjectGrid : MonoBehaviour
{
    [SerializeField] private GameObject _padPrefab;
    [SerializeField, Min(0)] private int _amount;
    [SerializeField, Min(0)] private int _columns;
    [SerializeField, Min(0)] private float _cellSize;
    [SerializeField, Min(0)] private float _offsetX;
    [SerializeField, Min(0)] private float _offsetY;

    private GameObject[,] grid;
    private PadManager _padManager;

    public Action<Pad> OnPadCreated;

    private void Start()
    {
        _padManager = PadManager.Instance;
        CreateGrid();
    }

    [Button]
    private void CreateGrid()
    {
        ClearGrid();

        if (_amount <= 0 || _columns <= 0 || _padPrefab == null)
        {
            return;
        }

        int rows = Mathf.CeilToInt((float)_amount / _columns);
        grid = new GameObject[rows, _columns];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < _columns; j++)
            {
                if (i * _columns + j < _amount)
                {
                    // Calculate position to center the grid
                    // The y-axis is inverted (row 0 is at the top)
                    float xPos = (j - (_columns - 1) / 2.0f) * (_cellSize + _offsetX);
                    float yPos = ((rows - 1) / 2.0f - i) * (_cellSize + _offsetY);
                    Vector3 localPosition = new Vector3(xPos, yPos, 0);

                    var padObject = ObjectPool.Instance.GetObject(_padPrefab.GetComponentInChildren<PoolableObject>().PoolTag).gameObject;
                    padObject.transform.SetParent(transform);

                    grid[i, j] = padObject;
                    if (padObject.GetComponentInChildren<Pad>() is Pad pad)
                    {
                        padObject.transform.localPosition = localPosition;
                        pad.Index = new Vector2(i, j);
                        OnPadCreated?.Invoke(pad);
                    }
                    else
                    {
                        Debug.LogWarning($"Pad at ({i}, {j}) does not have a CardStackPlacer component.");
                    }
                    padObject.name = $"Pad {i * _columns + j + 1}";
                }
            }
        }
    }

    [Button]
    private void ClearGrid()
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

        grid = null;
        _padManager.Pads.Clear();
        _padManager.UnlockedPads.Clear();
    }
}
