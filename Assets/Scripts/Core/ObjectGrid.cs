using NaughtyAttributes;
using UnityEngine;

public class ObjectGrid : MonoBehaviour
{
    [SerializeField] private GameObject cellPrefab;
    [SerializeField, Min(0)] private int _amount;
    [SerializeField, Min(0)] private int _columns;
    [SerializeField, Min(0)] private float _cellSize;
    [SerializeField, Min(0)] private float _offsetX;
    [SerializeField, Min(0)] private float _offsetY;

    private GameObject[,] grid;

    private void Start()
    {
        CreateGrid();
    }

    [Button]
    private void CreateGrid()
    {
        ClearGrid();

        if (_amount <= 0 || _columns <= 0 || cellPrefab == null)
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

                    GameObject cell = Instantiate(cellPrefab, Vector3.zero, Quaternion.identity, transform);
                    cell.transform.localPosition = localPosition;
                    grid[i, j] = cell;
                    if (cell.TryGetComponent(out CardStackPlacer stackPlacer))
                    {
                        stackPlacer.LocalPosition = localPosition;
                        stackPlacer.Index = new Vector2(i, j);
                    }
                    else
                    {

                        cell.AddComponent<CardStackPlacer>().LocalPosition = localPosition;
                        cell.GetComponent<CardStackPlacer>().Index = new Vector2(i, j);
                    }
                    cell.name = $"Cell {i * _columns + j + 1}";
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

        foreach (GameObject cell in grid)
        {
            if (cell != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(cell);
                }
                else
                {
                    DestroyImmediate(cell);
                }
            }
        }

        grid = null;
    }
}
