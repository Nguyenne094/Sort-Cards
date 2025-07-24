using UnityEngine;

public class GameManager : MonoBehaviour {
    private void Start() {
        QualitySettings.vSyncCount = 0; // Disable VSync
        Application.targetFrameRate = 60; // Set target frame rate to 60 FPS
    }
}