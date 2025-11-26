using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AdvancedAudioSource : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("Source Data")]
    public AudioClip[] clipsOnAwake;
    public enum SourceType {None, SoundEffect, Music, Master} public SourceType type;

    [Header("Source Effects")]
    public bool fadeOnAwake;
    public bool fadeLowPassOnAwake;


    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if(fadeOnAwake)
        DoFadeIn(5);

        if(fadeLowPassOnAwake)
        DoLowPassFade(5, 22000f);

        if(clipsOnAwake.Length > 0)
        audioSource.clip = clipsOnAwake[UnityEngine.Random.Range(0, clipsOnAwake.Length)];

        UpdateAudioSource();
    }

    #region AudioSource Properties

    public float Volume
    {
        get => audioSource.volume;
        set => audioSource.volume = value;
    }

    public bool Loop
    {
        get => audioSource.loop;
        set => audioSource.loop = value;
    }

    public bool IsPlaying => audioSource.isPlaying;

    #endregion

    #region AudioSource Methods

    public void Play() => audioSource.Play();
    public void Stop() => audioSource.Stop();
    public void Pause() => audioSource.Pause();
    public void UnPause() => audioSource.UnPause();
    public void PlayOneShot(AudioClip clip, float volume = 1.0f) => audioSource.PlayOneShot(clip, volume);

    #endregion

    #region Custom AudioSource Methods

    public void UpdateAudioSource()
    {
        if(audioSource.playOnAwake)
        audioSource.Play();

        switch (type)
        {
            case SourceType.SoundEffect:
            audioSource.outputAudioMixerGroup = AudioManager.instance.soundEffectMixerGroup;
            break;

            case SourceType.Music:
            audioSource.outputAudioMixerGroup = AudioManager.instance.musicMixerGroup;
            break;

            case SourceType.Master:
            audioSource.outputAudioMixerGroup = AudioManager.instance.masterMixerGroup;
            break;
        }
    }
    // Plays a random pitch with pre-set clip (for UnityEvent<float>)
    public void PlayWithPitch(float higherPitch)
    {
        if (audioSource.clip == null) return;
        audioSource.pitch = UnityEngine.Random.Range(1f, higherPitch);
        audioSource.Play();
    }

        #region Fading

    public void DoFadeIn(float duration)
    {
        Volume = 0;
        DOVirtual.Float(Volume, 1, duration, value => Volume = value);
    }

    //Volume Fading
    
    /// <summary>
    /// Transition volume on duration
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="targetValue"></param>
    public void DoVolumeFade(float duration, float targetValue)
    {
        DOVirtual.Float(Volume, targetValue, duration, value => Volume = value);
    }

    /// <summary>
    /// Transition volume on duration, Insert an action on completing transition
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="targetValue"></param>
    /// <param name="onCompletedAction"></param>
    public void DoVolumeFade(float duration, float targetValue, Action onCompletedAction)
    {
        DOVirtual.Float(Volume, targetValue, duration, value => Volume = value).OnComplete(() => onCompletedAction?.Invoke());;
    }


    //LowPass Fading

    /// <summary>
    /// Transition LowPass on duration
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="targetValue"></param>
    public void DoLowPassFade(float duration, float targetValue)
    {
        AudioLowPassFilter filter = GetComponent<AudioLowPassFilter>();
        DOVirtual.Float(filter.cutoffFrequency, targetValue, duration, value => {filter.cutoffFrequency = value;});
    }

    /// <summary>
    /// Transition LowPass on duration, Insert an action on completing transition
    /// </summary>
    /// <param name="duration"></param>
    /// <param name="targetValue"></param>
    /// <param name="onCompletedAction"></param>
    public void DoLowPassFade(float duration, float targetValue, Action onCompletedAction)
    {
        AudioLowPassFilter filter = GetComponent<AudioLowPassFilter>();
        DOVirtual.Float(filter.cutoffFrequency, targetValue, duration, value => {filter.cutoffFrequency = value;}).OnComplete(() => onCompletedAction?.Invoke());
    }

        #endregion


    #endregion
}
