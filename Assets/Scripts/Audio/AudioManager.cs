using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using System;
using DG.Tweening;
using UnityEngine.SceneManagement;


[System.Serializable]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioData data;
    public bool startFadeIn;
    public AudioMixerGroup musicMixerGroup, soundEffectMixerGroup, ambienceMixerGroup, masterMixerGroup;

    public AudioMixerGroup DefaultAudioMixer;

    public bool destroyOnSceneLoad;

    private void Awake()
    {
        transform.parent = null;
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }
        
        DefaultAudioMixer.audioMixer.SetFloat("VolumeMaster", -80f);
        StartCoroutine(FadeIn());
        StartCoroutine(FadeInLowpass(1));
        RestoreSpeed(1);

        SceneManager.sceneLoaded += OnSceneLoad;

        SetupAudioManager();
    }

    public void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
    {
        StartCoroutine(FadeIn());
        StartCoroutine(FadeInLowpass(1));
        RestoreSpeed(1);
    }
    

    public void DestroyManager()
    {
        StartCoroutine(FadeOut(true));
    }

    void SetupAudioManager()
    {
        foreach (Sound soundToPlay in data.sounds)
        {
            // Create AudioSources for each sound
            soundToPlay.Source = gameObject.AddComponent<AudioSource>();

            // AudioSource Data
            soundToPlay.Source.pitch = soundToPlay.Pitch;
            soundToPlay.Source.clip = soundToPlay.Clip;
            soundToPlay.Source.loop = soundToPlay.Loop;
            soundToPlay.Source.volume = soundToPlay.Volume;
            soundToPlay.Source.outputAudioMixerGroup = soundToPlay.Mixer;

            if (soundToPlay.PlayOnAwake)
                soundToPlay.Source.Play();
        }
    }

    //-------------------Play sound
    /// <summary>
    /// Find and play sound, using it's name
    /// </summary>
    /// <param name="SoundName"></param>
    public void PlaySound(string SoundName)
    {
        foreach (Sound soundToPlay in data.sounds)
            if (soundToPlay.SoundName == SoundName)
            {
                soundToPlay.Source.Stop();
                soundToPlay.Source.Play();
                break;
            }
    }

    /// <summary>
    /// Find and play sound, using it's name. with random pitch
    /// </summary>
    /// <param name="SoundName"></param>
    /// <param name="MinPitch"></param>
    /// <param name="MaxPitch"></param>
    public void PlaySound(string SoundName, float MinPitch, float MaxPitch)
    {
        foreach (Sound soundToPlay in data.sounds)
            if (soundToPlay.SoundName == SoundName)
            {
                soundToPlay.Source.Stop();
                soundToPlay.Source.pitch = UnityEngine.Random.Range(MinPitch, MaxPitch);
                soundToPlay.Source.Play();
                break;
            }
    }

    /// <summary>
    /// Stop sound, using it's name
    /// </summary>
    /// <param name="SoundName"></param>
    public void StopSound(string SoundName)
    {
        foreach (Sound soundToPlay in data.sounds)
            if (soundToPlay.SoundName == SoundName)
            {
                soundToPlay.Source.Stop();
                return;
            }
    }

    /// <summary>
    /// Replace the sound audioClip with another, using the newclip and it's name
    /// </summary>
    /// <param name="SoundName"></param>
    /// <param name="NewClip"></param>
    public void ReplaceSoundClip(string SoundName, AudioClip NewClip)
    {
        foreach (Sound soundToPlay in data.sounds)
            if (soundToPlay.SoundName == SoundName)
            {
                soundToPlay.Source.clip = NewClip;
                return;
            }
    }

    //-------------------Effects
    
    // Slow down audio mixer
    public void SlowDown(float targetPitch = 0.5f, float duration = 1f)
    {
        DOTween.To(
            () => {
                DefaultAudioMixer.audioMixer.GetFloat("Pitch", out float v);
                return v;
            },
            x => DefaultAudioMixer.audioMixer.SetFloat("Pitch", x),
            targetPitch,
            duration
        ).SetUpdate(true);
    }
    
    // Restore normal speed
    public void RestoreSpeed(float duration = 2f)
    {
        DOTween.To(
            () => {
                DefaultAudioMixer.audioMixer.GetFloat("Pitch", out float v);
                return v;
            },
            x => DefaultAudioMixer.audioMixer.SetFloat("Pitch", x),
            1f,
            duration
        ).SetUpdate(true);
    }


    /// <summary>
    /// Fade all Audio In
    /// </summary>
    /// <returns></returns>
    public IEnumerator FadeIn()
    {
        DefaultAudioMixer.audioMixer.SetFloat("VolumeMaster", -80f);
        float currentTime = 0f;

        while (currentTime < 3)
        {
            currentTime += Time.deltaTime;
            float newVolume = Mathf.Lerp(-80f, 0f, currentTime / 3);
            DefaultAudioMixer.audioMixer.SetFloat("VolumeMaster", newVolume);
            yield return null;
        }

        DefaultAudioMixer.audioMixer.SetFloat("VolumeMaster", 0f);
    }

    /// <summary>
    /// Fade all audio out
    /// </summary>
    /// <param name="destroyOnSceneLoad"></param>
    /// <returns></returns>
    public IEnumerator FadeOut(bool destroyOnSceneLoad)
    {
        DefaultAudioMixer.audioMixer.SetFloat("VolumeMaster", 0);
        float currentTime = 0f;

        while (currentTime < 3)
        {
            currentTime += Time.unscaledDeltaTime;
            float newVolume = Mathf.Lerp(0, -90f, currentTime / 3);
            DefaultAudioMixer.audioMixer.SetFloat("VolumeMaster", newVolume);
            yield return null;
        }

        DefaultAudioMixer.audioMixer.SetFloat("VolumeMaster", -90f);

        if (destroyOnSceneLoad)
            Destroy(gameObject);
    }

    // Fade in Lowpass (removes lowpass effect)
    public IEnumerator FadeInLowpass(float duration = 3f, float targetCutoff = 22000f)
    {
        DefaultAudioMixer.audioMixer.GetFloat("LowPass", out float startCutoff);
        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float newCutoff = Mathf.Lerp(startCutoff, targetCutoff, time / duration);
            DefaultAudioMixer.audioMixer.SetFloat("LowPass", newCutoff);
            yield return null;
        }
        DefaultAudioMixer.audioMixer.SetFloat("LowPass", targetCutoff);
    }

    // Fade out Lowpass (applies lowpass effect)
    public IEnumerator FadeOutLowpass(float duration = 3f, float targetCutoff = 500f)
    {
        DefaultAudioMixer.audioMixer.GetFloat("LowPass", out float startCutoff);
        float time = 0f;
        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            float newCutoff = Mathf.Lerp(startCutoff, targetCutoff, time / duration);
            DefaultAudioMixer.audioMixer.SetFloat("LowPass", newCutoff);
            yield return null;
        }
        DefaultAudioMixer.audioMixer.SetFloat("LowPass", targetCutoff);
    }


    /// <summary>
    /// Transition AudioClip replacement. finding it using it's name, and replacing it with the clip
    /// </summary>
    /// <param name="audioName"></param>
    /// <param name="clip"></param>
    /// <returns></returns>
    public IEnumerator FadeOutReplace(string audioName, AudioClip clip)
    {
        AudioSource source = new();
        foreach (Sound soundToPlay in data.sounds)
        {
            if (soundToPlay.SoundName == audioName)
            {
                source = soundToPlay.Source;
                break;
            }
        }


        float currentTime = 0f;
        while (currentTime < 3)
        {
            currentTime += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(source.volume, 0, currentTime / 3);
            yield return null;
        }

        source.clip = clip;

        float currentTime2 = 3f;
        while (currentTime2 < 6)
        {
            currentTime += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(source.volume, 1, currentTime / 3);
            yield return null;
        }
    }
}

[System.Serializable]
public class Sound
{
    [Space(10)]
    //----SoundData
    public string SoundName;
    public bool Loop;
    public bool PlayOnAwake;

    [Space(10)]
    //----Sliders
    [Range(0, 1)] public float Volume;
    [Range(0, 3)] public float Pitch = 1;

    [Space(10)]
    //----AudioSource Data
    [HideInInspector]
    public AudioSource Source;

    public AudioMixerGroup Mixer;
    public AudioClip Clip;
}