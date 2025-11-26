using UnityEngine.SceneManagement;
using UnityEngine;
using System;
using Discord;

public class Discord_Controller : MonoBehaviour
{
    public static Discord_Controller Instance;

    public Discord.Discord discord;
    string currentLevelName;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        discord = new Discord.Discord(1408933193935093873, (ulong)CreateFlags.NoRequireDiscord);
        currentLevelName = SceneManager.GetActiveScene().name;
        UpdateData();
    }

    void Update()
    {
        try { discord.RunCallbacks(); }
        catch { Destroy(gameObject); }
    }

    void OnApplicationQuit()
    {
        var am = discord.GetActivityManager();
        var act = new Activity();
        am.UpdateActivity(act, (r) => { });
        Destroy(gameObject);
    }

    void UpdateData()
    {
        try
        {
            var am = discord.GetActivityManager();
            var act = new Activity
            {
                Details = "Currently Testing",
                State = "On: " + currentLevelName,
                Type = ActivityType.Playing,
                Assets =
                {
                    LargeImage = "embedded_background",
                    LargeText = "testing the game",
                    SmallImage = "embedded_cover"
                },
                Timestamps =
                {
                    Start = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds
                }
            };
            am.UpdateActivity(act, (r) => { if (r != Result.Ok) Debug.LogWarning("Discord failed"); });
        }
        catch { Destroy(gameObject); }
    }
}
