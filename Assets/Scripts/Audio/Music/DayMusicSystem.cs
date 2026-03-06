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

        DayCycleMusic entry = GetEntryForDay(DayNightCycleManager.Instance.DayCount);
        RebuildPlaylist(entry.dayTracks, ref dayPlaylist);
        RebuildPlaylist(entry.nightTracks, ref nightPlaylist);

        // Music crossfade
        DOVirtual.Float(daySource.volume,   day ? 1f : 0f, transitionTime, x => daySource.volume = x);
        DOVirtual.Float(nightSource.volume, day ? 0f : 1f, transitionTime, x => nightSource.volume = x);

        // Ambience crossfade
        DayCyclePreset preset = day
            ? DayNightCycleManager.Instance.dayPreset
            : DayNightCycleManager.Instance.nightPreset;

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
        scheduleCoroutine = StartCoroutine(ScheduleNextTrack());
    }

    private IEnumerator ScheduleNextTrack()
    {
        while (true)
        {
            float delay = Random.Range(randomPlayDelay.x, randomPlayDelay.y);
            yield return new WaitForSeconds(delay);

            if (isDay) PlayNext(daySource, dayPlaylist, ref lastDayTrack);
            else        PlayNext(nightSource, nightPlaylist, ref lastNightTrack);
        }
    }

    private void PlayNext(AudioSource source, List<AudioClip> playlist, ref AudioClip last)
    {
        if (playlist.Count == 0) return;

        if (playlist.Count == 1 && playlist[0] == last)
        {
            DayCycleMusic entry = GetEntryForDay(DayNightCycleManager.Instance.DayCount);
            RebuildPlaylist(isDay ? entry.dayTracks : entry.nightTracks, ref playlist);
        }

        AudioClip next = playlist[0];
        playlist.RemoveAt(0);
        last = next;

        source.clip = next;
        source.Play();
    }

    private void RebuildPlaylist(List<AudioClip> source, ref List<AudioClip> playlist)
    {
        playlist = new List<AudioClip>(source);
        Shuffle(playlist);

        AudioClip last = playlist == dayPlaylist ? lastDayTrack : lastNightTrack;
        if (playlist.Count > 1 && playlist[0] == last)
        {
            playlist.RemoveAt(0);
            playlist.Insert(Random.Range(1, playlist.Count), last);
        }
    }

    private void Shuffle(List<AudioClip> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
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

#if UNITY_EDITOR
    [ContextMenu("Preview Day Track")]
    private void PreviewDay()
    {
        DayCycleMusic entry = GetEntryForDay(DayNightCycleManager.Instance?.DayCount ?? 0);
        if (entry?.dayTracks?.Count > 0)
        {
            daySource.clip = entry.dayTracks[Random.Range(0, entry.dayTracks.Count)];
            daySource.Play();
            Debug.Log($"[DayMusicSystem] Previewing day track: {daySource.clip.name}");
        }
    }

    [ContextMenu("Preview Night Track")]
    private void PreviewNight()
    {
        DayCycleMusic entry = GetEntryForDay(DayNightCycleManager.Instance?.DayCount ?? 0);
        if (entry?.nightTracks?.Count > 0)
        {
            nightSource.clip = entry.nightTracks[Random.Range(0, entry.nightTracks.Count)];
            nightSource.Play();
            Debug.Log($"[DayMusicSystem] Previewing night track: {nightSource.clip.name}");
        }
    }

    [ContextMenu("Preview Day Ambience")]
    private void PreviewDayAmbience()
    {
        AudioClip clip = DayNightCycleManager.Instance?.dayPreset?.ambience;
        if (clip != null)
        {
            ambienceSource.clip = clip;
            ambienceSource.loop = true;
            ambienceSource.Play();
            Debug.Log($"[DayMusicSystem] Previewing day ambience: {clip.name}");
        }
    }

    [ContextMenu("Preview Night Ambience")]
    private void PreviewNightAmbience()
    {
        AudioClip clip = DayNightCycleManager.Instance?.nightPreset?.ambience;
        if (clip != null)
        {
            ambienceSource.clip = clip;
            ambienceSource.loop = true;
            ambienceSource.Play();
            Debug.Log($"[DayMusicSystem] Previewing night ambience: {clip.name}");
        }
    }

    [ContextMenu("Force Day Transition")]
    private void ForceDay() => OnDayChange(true);

    [ContextMenu("Force Night Transition")]
    private void ForceNight() => OnDayChange(false);

    [ContextMenu("Skip Current Track")]
    private void SkipTrack()
    {
        if (scheduleCoroutine != null) StopCoroutine(scheduleCoroutine);
        scheduleCoroutine = StartCoroutine(ScheduleNextTrack());
        if (isDay) PlayNext(daySource, dayPlaylist, ref lastDayTrack);
        else        PlayNext(nightSource, nightPlaylist, ref lastNightTrack);
    }
#endif
}