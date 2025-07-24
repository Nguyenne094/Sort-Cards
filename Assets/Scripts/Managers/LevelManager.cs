using System;
using System.Collections;
using System.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages level progression and phase transitions
/// </summary>
public class LevelManager : Singleton<LevelManager>
{
    [Header("Level Data")]
    [SerializeField] private LevelDataSO[] levels;

    [Header("UI Elements")]
    [SerializeField] private RectTransform completedPanel;
    [SerializeField] private RectTransform allLevelsCompletedPanel;
    [SerializeField] private Button nextLevelButton;
    [SerializeField] private Button exitButton;

    private ObjectGrid cardTable;
    private ObjectGrid cashExchangeTable;

    // Events
    public Action<LevelDataSO> OnLevelLoaded;
    public Action<int> OnPhaseActivated; // phase index
    public Action OnLevelCompleted;
    public Action OnAllLevelsCompleted;

    // Properties
    public LevelDataSO CurrentLevel { get; private set; }
    public int CurrentLevelIndex { get; private set; } = -1;

    #region Unity Lifecycle

    protected override void Awake()
    {
        base.Awake(); 
        cardTable = PadManager.Instance.CardTable.GetComponent<ObjectGrid>();
        cashExchangeTable = PadManager.Instance.CashExchangeTable.GetComponent<ObjectGrid>();

        SubscribeToEvents();
        LoadLevel(0);
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    #endregion

    #region Event Handling

    private void SubscribeToEvents()
    {
        if (PadManager.Instance != null)
        {
            PadManager.Instance.OnPhaseCompleted += HandlePhaseCompleted;
            PadManager.Instance.OnAllPadsPaid += HandleAllPadsPaid;
        }

        if (cardTable != null)
        {
            cardTable.OnGridCreationComplete += HandleGridCreationComplete;
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.AddListener(LoadNextLevel);
        }
        
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(() => Application.Quit());
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (PadManager.Instance != null)
        {
            PadManager.Instance.OnPhaseCompleted -= HandlePhaseCompleted;
            PadManager.Instance.OnAllPadsPaid -= HandleAllPadsPaid;
        }

        if (cardTable != null)
        {
            cardTable.OnGridCreationComplete -= HandleGridCreationComplete;
        }

        if (nextLevelButton != null)
        {
            nextLevelButton.onClick.RemoveListener(LoadNextLevel);
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveListener(() => Application.Quit());
        }
    }

    private void HandlePhaseCompleted(int phaseIndex)
    {
        Debug.Log($"Phase {phaseIndex} completed!");
        ActivateNextPhase();
    }

    private void HandleAllPadsPaid()
    {
        Debug.Log("All pads paid! Level completed!");
        OnLevelCompleted?.Invoke();
        completedPanel.gameObject.SetActive(true);
    }

    private void HandleGridCreationComplete()
    {
        // This is called after all pads are created and registered
        Debug.Log("Grid creation complete. Setting up phase 0.");
        ActivatePhase(0);
        SetCostForPads();
        UnlockAllPadsOfPhase(0);
        PayAllPadsOfPhase(0);
    }

    #endregion

    #region Phase Management

    /// <summary>
    /// Get current active phase index
    /// </summary>
    public int GetCurrentPhaseIndex()
    {
        if (CurrentLevel?.phases == null)
            return -1;

        for (int i = 0; i < CurrentLevel.phases.Length; i++)
        {
            if (CurrentLevel.phases[i].isActive)
                return i;
        }
        return -1;
    }

    /// <summary>
    /// Activate specific phase
    /// </summary>
    public void ActivatePhase(int phaseIndex)
    {
        if (CurrentLevel?.phases == null || phaseIndex < 0 || phaseIndex >= CurrentLevel.phases.Length)
        {
            Debug.LogWarning($"Invalid phase index: {phaseIndex} to unlock pads");
            return;
        }

        // Deactivate all phases
        foreach (var phase in CurrentLevel.phases)
        {
            phase.isActive = false;
        }

        // Activate target phase
        var targetPhase = CurrentLevel.phases[phaseIndex];
        targetPhase.isActive = true;

        UnlockAllPadsOfPhase(phaseIndex);

        OnPhaseActivated?.Invoke(phaseIndex);
        Debug.Log($"Activated Phase {phaseIndex}");
    }

    /// <summary>
    /// Activate next phase
    /// </summary>
    public void ActivateNextPhase()
    {
        int currentPhase = GetCurrentPhaseIndex();
        if (currentPhase >= 0 && currentPhase + 1 < CurrentLevel.phases.Length)
        {
            ActivatePhase(currentPhase + 1);
        }
        else
        {
            Debug.Log("No more phases to activate");
        }
    }

    #endregion

    #region Level Management

    /// <summary>
    /// Load specific level
    /// </summary>
    public void LoadLevel(int levelIndex)
    {
        if (levelIndex < 0 || levelIndex >= levels.Length)
        {
            Debug.LogError($"Invalid level index: {levelIndex}");
            return;
        }

        CleanupCurrentLevel();

        CurrentLevelIndex = levelIndex;
        CurrentLevel = levels[levelIndex];

        SetupLevel();
        OnLevelLoaded?.Invoke(CurrentLevel);

        Debug.Log($"Loaded Level {levelIndex}: {CurrentLevel.levelName}");
    }

    /// <summary>
    /// Load next level
    /// </summary>
    public void LoadNextLevel()
    {
        int nextIndex = CurrentLevelIndex + 1;
        if (nextIndex < levels.Length)
        {
            LoadLevel(nextIndex);
            completedPanel.gameObject.SetActive(false);
        }
        else
        {
            Debug.Log("All levels completed!");
            OnAllLevelsCompleted?.Invoke();
            completedPanel.gameObject.SetActive(false);
            allLevelsCompletedPanel.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Restart current level
    /// </summary>
    public void RestartCurrentLevel()
    {
        if (CurrentLevelIndex >= 0)
        {
            LoadLevel(CurrentLevelIndex);
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Setup level components
    /// </summary>
    private void SetupLevel()
    {
        if (CurrentLevel?.gridData == null)
        {
            Debug.LogError("Level grid data is missing!");
            return;
        }

        // Setup grids - this will trigger the OnGridCreationComplete event when done
        cardTable.GridDataSO = CurrentLevel.gridData;
        cardTable.CreateGrid();
    }

    private void SetCostForPads()
    {
        if (CurrentLevel?.phases == null || CurrentLevel.phases.Length == 0)
        {
            Debug.LogWarning("No phases available to set pad costs.");
            return;
        }

        for (int i = 0; i < CurrentLevel.phases.Length; i++)
        {
            var phase = CurrentLevel.phases[i];
            for (int j = 0; j < phase.playPadIndexes.Count; j++)
            {
                var padIndex = phase.playPadIndexes[j];
                if (PadManager.Instance.GetPadByIndex(padIndex) is PlayPad pad)
                {
                    pad.UnlockCost = phase.unlockCost;
                }
            }
        }
    }

    private void UnlockAllPadsOfPhase(int phaseIndex)
    {
        if (CurrentLevel?.phases == null || phaseIndex < 0 || phaseIndex >= CurrentLevel.phases.Length)
        {
            Debug.LogWarning($"Invalid phase index: {phaseIndex} to unlock pads");
            return;
        }

        var phase = CurrentLevel.phases[phaseIndex];
        foreach (var padIndex in phase.playPadIndexes)
        {
            var pad = PadManager.Instance.GetPadByIndex(padIndex);
            if (pad is PlayPad playPad && playPad.IsLocked)
            {
                PadManager.Instance.UnlockPad(playPad);
            }
        }
    }

    private void PayAllPadsOfPhase(int phaseIndex)
    {
        if (CurrentLevel?.phases == null || phaseIndex < 0 || phaseIndex >= CurrentLevel.phases.Length)
        {
            Debug.LogWarning($"Invalid phase index: {phaseIndex} to pay pads");
            return;
        }

        var phase = CurrentLevel.phases[phaseIndex];
        foreach (var padIndex in phase.playPadIndexes)
        {
            var pad = PadManager.Instance.GetPadByIndex(padIndex);
            if (pad is PlayPad playPad && !playPad.IsPaid)
            {
                PadManager.Instance.PayPad(playPad);
            }
        }
    }

    /// <summary>
    /// Configure pads for given phase
    /// </summary>
    // private void ConfigurePadsForPhase(LevelDataSO.Phase phase)
    // {
    //     var padManager = PadManager.Instance;
    //     if (padManager == null || phase == null) return;

    //     // Setup pads in phase
    //     foreach (var padIndex in phase.playPadIndexes)
    //     {
    //         var pad = padManager.GetPadByIndex(padIndex);
    //         if (pad != null)
    //         {
    //             pad.Setup(phase.unlockCost);
    //         }
    //     }
    // }

    private void CleanupCurrentLevel()
    {
        if (CurrentLevel == null) return;

        // Reset managers
        PadManager.Instance.ResetForNewLevel();

        // Clear grids
        cardTable.ClearGrid();
    }

    #endregion

    #region Debug Methods

    [Button("Restart Level")]
    public void RestartLevel()
    {
        Debug.Log("Restarting current level...");
        RestartCurrentLevel();
    }

    #endregion
}
