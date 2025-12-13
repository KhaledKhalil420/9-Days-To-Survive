using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class DayMusicSystem : MonoBehaviour
{
    [SerializeField] AudioSource source;
    [SerializeField] List<DayMusic> tracks;

    [SerializeField] float minSilence = 60f;
    [SerializeField] float maxSilence = 180f;

    List<DayMusic> pool = new();
    Tween timer;
    int currentDay;

    public void StartDay(int day)
    {
        currentDay = day;
        RebuildPool();
        ScheduleNext();
    }

    void ScheduleNext()
    {
        timer?.Kill();

        timer = DOVirtual.DelayedCall(
            Random.Range(minSilence, maxSilence),
            PlayNext
        );
    }

    void PlayNext()
    {
        if (pool.Count == 0)
            RebuildPool();

        var m = TakeRandom();
        source.clip = m.clip;
        source.Play();

        timer = DOVirtual.DelayedCall(
            m.clip.length,
            ScheduleNext
        );
    }

    void RebuildPool()
    {
        pool.Clear();
        foreach (var m in tracks)
            if (currentDay >= m.minDay && currentDay <= m.maxDay)
                pool.Add(m);
    }

    DayMusic TakeRandom()
    {
        int i = Random.Range(0, pool.Count);
        var m = pool[i];
        pool.RemoveAt(i);
        return m;
    }

    public void Stop()
    {
        timer?.Kill();
        source.Stop();
    }
}

[System.Serializable]
public class DayMusic
{
    public AudioClip clip;
    [Range(1, 9)] public int minDay;
    [Range(1, 9)] public int maxDay;
}