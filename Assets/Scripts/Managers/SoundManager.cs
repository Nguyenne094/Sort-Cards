using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using UnityEngine.UI;

public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource audioSourcePref;
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField, Tooltip("Audio Clips play in background")] private List<AudioClip> audioClips;
    [SerializeField] private AudioSource soundBackground;

    private ObjectPool<AudioSource> audioSourcePool;

    public bool IsMasterVolumeOn { get; private set; } = true;

    private void Start()
    {
        audioSourcePool =
            new ObjectPool<AudioSource>(CreateNewAudioSource, GetAS, ReleaseAS, DestroyAS, false, 10, 20);

        if (soundBackground)
        {
            soundBackground.clip = audioClips[Random.Range(0, audioClips.Count - 1)];
            soundBackground.Play();
        }

        audioMixer.SetFloat("music", 1);
        audioMixer.SetFloat("sfx", 1);
    }

    private void DestroyAS(AudioSource obj)
    {
        Destroy(obj.gameObject);
    }

    private void ReleaseAS(AudioSource obj)
    {
        obj.gameObject.SetActive(false);
    }

    private void GetAS(AudioSource obj)
    {
        obj.gameObject.SetActive(true);
    }

    private AudioSource CreateNewAudioSource()
    {
        AudioSource audioSource = Instantiate(audioSourcePref);
        audioSource.gameObject.SetActive(false);
        
        return audioSource;
    }

    public void StopBackgroundMusic()
    {
        soundBackground?.Stop();
    }

    public void ResumeBackgroundMusic()
    {
        soundBackground?.Play();
    }

    public void ToggleMasterVolume()
    {
        IsMasterVolumeOn = !IsMasterVolumeOn;
        audioMixer.SetFloat("master", IsMasterVolumeOn ? 0 : -80);
    }

    public void onMusicSliderValueChanged()
    {
        float value = musicSlider.value;
        audioMixer.SetFloat("music", Mathf.Log10(value) * 20);
    }
    
    public void onSFXSliderValueChanged()
    {
        float value = sfxSlider.value;
        audioMixer.SetFloat("sfx", Mathf.Log10(value)*20);
    }

    public void PlaySFX(AudioClip audioClip, float volume = 1)
    {
        AudioSource audioSource = audioSourcePool.Get();
        audioSource.outputAudioMixerGroup = audioMixer.FindMatchingGroups("Master")[0];
        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();
        StartCoroutine(ReleaseAudioSourceToPoolAfter(audioSource, audioSource.clip.length));
    }

    private IEnumerator ReleaseAudioSourceToPoolAfter(AudioSource audioSource, float clipLength)
    {
        yield return new WaitForSeconds(clipLength);
        audioSourcePool.Release(audioSource);
    }
}