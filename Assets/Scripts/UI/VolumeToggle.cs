using UnityEngine;
using UnityEngine.UI;

public class VolumeToggle : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private Sprite volumeOnIcon;
    [SerializeField] private Sprite volumeOffIcon;
    [SerializeField] private AudioClip toggleSound;
    private SoundManager soundManager;

    private void Awake()
    {
        soundManager = SoundManager.Instance;
        button.onClick.AddListener(ToggleVolume);
    }

    private void ToggleVolume()
    {
        Debug.Log("Toggle Volume Button Clicked");
        soundManager.ToggleMasterVolume();
        iconImage.sprite = soundManager.IsMasterVolumeOn ? volumeOnIcon : volumeOffIcon;

        if (toggleSound)
        {
            soundManager.PlaySFX(toggleSound);
        }
    }
}