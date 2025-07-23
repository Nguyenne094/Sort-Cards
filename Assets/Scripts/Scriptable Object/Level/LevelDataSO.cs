using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "Game/LevelData")]
public class LevelDataSO : ScriptableObject
{
    [System.Serializable]
    public class Phase
    {
        [Header("Phase Info")]
        public int phaseIndex = 0;
        public int unlockCost = 0;
        
        [Header("Pad Configuration")]
        public List<Vector2> playPadIndexes = new List<Vector2>();

        [HideInInspector] public bool isActive = false;
        
        /// <summary>
        /// Get all pad indexes in this phase
        /// </summary>
        public List<Vector2> GetAllPadIndexes()
        {
            return playPadIndexes;
        }
    }

    [Header("Level Settings")]
    public int levelIndex = 0;
    public string levelName = "Level 1";
    
    [Header("Grid Configuration")]
    public GridDataSO gridData;
    
    [Header("Phases")]
    public Phase[] phases = new Phase[0];
    
    /// <summary>
    /// Get maximum color count for this level (based on total unlockable pads)
    /// </summary>
    public int GetMaxColorCount()
    {
        int totalNormalPads = 0;
        foreach (var phase in phases)
        {
            totalNormalPads += phase.playPadIndexes.Count;
        }
        return Mathf.Min(totalNormalPads, CardUtility.GetTotalColors());
    }
    
    /// <summary>
    /// Get phase by index
    /// </summary>
    public Phase GetPhase(int index)
    {
        if (index >= 0 && index < phases.Length)
            return phases[index];
        return null;
    }
}
