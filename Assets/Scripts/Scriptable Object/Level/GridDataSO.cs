using UnityEngine;

[CreateAssetMenu(fileName = "GridData", menuName = "GridDataSO", order = 1)]
public class GridDataSO : ScriptableObject
{
    [Min(0)] public int Amount;
    [Min(0)] public int Columns;
    [Min(0)] public float CellSize;
    [Min(0)] public float OffsetX;
    [Min(0)] public float OffsetY;
}