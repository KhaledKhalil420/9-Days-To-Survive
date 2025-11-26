using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class KeyEntry
{
    public string Action;
    public KeyCode Key;
}

public static class Keybinds
{
    private static Dictionary<string, KeyCode> keyMap = new();
    private static bool initialized = false;

    public static KeyCode Key(string action)
    {
        if (!initialized) Init();
        return keyMap.TryGetValue(action, out var key) ? key : KeyCode.None;
    }

    public static void Set(string action, KeyCode key)
    {
        keyMap[action] = key;
        // Optional: Save to file or update UI here
    }

    private static void Init()
    {
        var config = Resources.Load<KeybindsConfig>("KeybindsConfig"); // must be in Resources folder
        if (config == null)
        {
            Debug.LogError("KeybindsConfig asset not found in Resources folder!");
            return;
        }

        keyMap.Clear();
        foreach (var entry in config.keyEntries)
            keyMap[entry.Action] = entry.Key;

        initialized = true;
    }
}