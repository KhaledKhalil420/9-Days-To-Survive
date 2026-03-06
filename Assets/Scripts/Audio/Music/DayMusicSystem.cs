using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

[System.Serializable]
public class DayCycleMusic
{
    public int dayToPlayIn = 0;
    public List<AudioClip> dayTracks;
    public List<AudioClip> nightTracks;
}

public class DayMusicSystem : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private AudioSource daySource;
    [SerializeField] private AudioSource nightSource;
    [SerializeField] private float transitionTime = 3f;
    [SerializeField] private Vector2 randomPlayDelay = new Vector2(5f, 20f);
    [SerializeField] private List<DayCycleMusic> cycles;

    [Header("Ambience")]
    [SerializeField] private AudioSource ambienceSource;

    private List<AudioClip> dayPlaylist = new();
    private List<AudioClip> nightPlaylist = new();
    private AudioClip lastDayTrack;
    private AudioClip lastNightTrack;
    private bool isDay = true;
    private Coroutine scheduleCoroutine;

    private void Start()
    {
        DayNightCycleManager.Instance.OnDayChange += OnDayChange;
    }

    private void OnDestroy()
    {
        if (DayNightCycleManager.Instance != null)
            DayNightCycleManager.Instance.OnDayChange -= OnDayChange;
    }

    private void OnDayChange(bool day)
    {
        isDay = day;

        daySource.Stop();
        nightSource.Stop();

        DayCycleMusic entry = GetEntryForDay(DayNightCycleManager.Instance.DayCount);
        dayPlaylist = Shuffled(entry.dayTracks, lastDayTrack);
        nightPlaylist = Shuffled(entry.nightTracks, lastNightTrack);

        DOVirtual.Float(daySource.volume,   day ? 1f : 0f, transitionTime, x => daySource.volume = x);
        DOVirtual.Float(nightSource.volume, day ? 0f : 1f, transitionTime, x => nightSource.volume = x);

        DayCyclePreset preset = day ? DayNightCycleManager.Instance.dayPreset : DayNightCycleManager.Instance.nightPreset;
        if (preset?.ambience != null && ambienceSource.clip != preset.ambience)
        {
            DOVirtual.Float(ambienceSource.volume, 0f, transitionTime * 0.5f, x => ambienceSource.volume = x)
                .OnComplete(() =>
                {
                    ambienceSource.clip = preset.ambience;
                    ambienceSource.loop = true;
                    ambienceSource.Play();
                    DOVirtual.Float(0f, 1f, transitionTime * 0.5f, x => ambienceSource.volume = x);
                });
        }

        if (scheduleCoroutine != null) StopCoroutine(scheduleCoroutine);
        scheduleCoroutine = StartCoroutine(ScheduleNextTrack(immediate: true));
    }

    private IEnumerator ScheduleNextTrack(bool immediate = false)
    {
        if (!immediate)
            yield return new WaitForSeconds(Random.Range(randomPlayDelay.x, randomPlayDelay.y));

        while (true)
        {
            AudioSource current;

            // Only start a new clip if that source isn't already playing.
            if (isDay)
            {
                current = daySource;
                if (!daySource.isPlaying)
                    PlayNext(daySource, ref dayPlaylist, ref lastDayTrack);
            }
            else
            {
                current = nightSource;
                if (!nightSource.isPlaying)
                    PlayNext(nightSource, ref nightPlaylist, ref lastNightTrack);
            }

            // Wait until that current source finishes, whether we just started it or it was already playing.
            yield return new WaitUntil(() => !current.isPlaying);

            // then random delay before next play
            yield return new WaitForSeconds(Random.Range(randomPlayDelay.x, randomPlayDelay.y));
        }
    }

    private void PlayNext(AudioSource source, ref List<AudioClip> playlist, ref AudioClip last)
    {
        // Safety: don't start a new clip if the source is still playing.
        if (source.isPlaying) return;

        if (playlist == null || playlist.Count == 0)
        {
            DayCycleMusic entry = GetEntryForDay(DayNightCycleManager.Instance.DayCount);
            if (entry != null)
                playlist = Shuffled(isDay ? entry.dayTracks : entry.nightTracks, last);
        }

        if (playlist == null || playlist.Count == 0) return;

        AudioClip next = playlist[0];
        playlist.RemoveAt(0);

        // If somehow the next equals the currently playing clip, and it's playing, skip it.
        if (source.clip == next && source.isPlaying) return;

        last = next;
        source.clip = next;
        source.Play();
    }

    private List<AudioClip> Shuffled(List<AudioClip> source, AudioClip avoid)
    {
        if (source == null) return new List<AudioClip>();

        List<AudioClip> list = new(source);
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        if (list.Count > 1 && list[0] == avoid)
        {
            list.RemoveAt(0);
            list.Insert(Random.Range(1, list.Count), avoid);
        }
        return list;
    }

    private DayCycleMusic GetEntryForDay(int day)
    {
        DayCycleMusic match = cycles.Find(c => c.dayToPlayIn == day);
        if (match != null) return match;
        if (cycles.Count > 0) return cycles[Random.Range(0, cycles.Count)];
        return null;
    }

    public void Stop()
    {
        if (scheduleCoroutine != null) StopCoroutine(scheduleCoroutine);
        daySource.Stop();
        nightSource.Stop();
        ambienceSource.Stop();
    }
}