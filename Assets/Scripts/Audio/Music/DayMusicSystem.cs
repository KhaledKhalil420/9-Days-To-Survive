using UnityEngine;
using UnityEngine.Audio; 
using System.Collections.Generic;
using DG.Tweening;

public class DayMusicSystem : MonoBehaviour
{
    [SerializeField] private AudioSource daySource, nightSource;
    [SerializeField] private float transitionTime = 3;
    [SerializeField] private List<DayMusic> dayTracks;
    [SerializeField] private List<NightMusic> nightTracks;


    private void Start()
    {
        DayNightCycleManager.Instance.OnDayChange += ChangeMusic;
    }
    

    public void ChangeMusic(bool day)
    {
        DOVirtual.Float(daySource.volume, day == true ? 1 : 0, transitionTime, x => daySource.volume = x);
        DOVirtual.Float(nightSource.volume, day == false ? 1 : 0, transitionTime, x => nightSource.volume = x);
    }

    public void Stop()
    {
        daySource.Stop();
    }
}

[System.Serializable]
public class DayMusic
{
    public AudioResource clip;
}

[System.Serializable]
public class NightMusic
{
    public AudioResource clip;
}