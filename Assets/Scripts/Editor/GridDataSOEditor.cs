using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridDataSO))]
public class GridDataSOEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GridDataSO gridData = (GridDataSO)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Grid Preview", EditorStyles.boldLabel);

        if (gridData.Columns <= 0 || gridData.Amount <= 0)
        {
            EditorGUILayout.HelpBox("Set Columns and Amount to see a preview.", MessageType.Info);
            return;
        }

        DrawGridPreview(gridData);
    }

    private void DrawGridPreview(GridDataSO gridData)
    {
        int cols = gridData.Columns;
        int rows = Mathf.CeilToInt((float)gridData.Amount / cols);
        float boxSize = 25f;

        EditorGUILayout.BeginVertical(GUI.skin.box);
        for (int r = 0; r < rows; r++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int c = 0; c < cols; c++)
            {
                int index = r * cols + c;
                if (index < gridData.Amount)
                {
                    GUILayout.Box($"{r},{c}", GUILayout.Width(boxSize), GUILayout.Height(boxSize));
                }
                else
                {
                    GUILayout.Space(boxSize + 4);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}