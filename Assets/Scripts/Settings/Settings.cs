using System;
using System.IO;
using UnityEngine;

// ── Unified data container ────────────────────────────────────────────────────
// All three settings classes read and write through this one object.
[System.Serializable]
public class AllSettingsData
{
    public GraphicsSettingsData graphics = new GraphicsSettingsData();
    public SoundSettingsData    sound    = new SoundSettingsData();
    public GameplaySettingsData gameplay = new GameplaySettingsData();
}

// ── Static manager ────────────────────────────────────────────────────────────
// Thread-safe enough for Unity's main-thread use. No MonoBehaviour needed.
public static class SettingsFileManager
{
    private static AllSettingsData _data;
    private static string          _cachedPath;

    // Lazily resolved once per session.
    public static string SavePath
    {
        get
        {
            if (_cachedPath != null) return _cachedPath;

            // In a build  : Application.dataPath = "<GameFolder>/<Name>_Data"  → ".." = game root
            // In the editor: Application.dataPath = "<Project>/Assets"          → ".." = project root
            // Either way we land somewhere sensible.
            string root        = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string settingsDir = Path.Combine(root, "settings");

            Directory.CreateDirectory(settingsDir);          // no-op if already exists
            _cachedPath = Path.Combine(settingsDir, "AllSettings.json");
            return _cachedPath;
        }
    }

    // Returns the in-memory data, loading from disk on first access.
    public static AllSettingsData Data => _data ?? Load();

    // ── Load ─────────────────────────────────────────────────────────────────

    public static AllSettingsData Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                _data = JsonUtility.FromJson<AllSettingsData>(File.ReadAllText(SavePath));

                // Nested objects can come back null if the file predates a new section.
                _data.graphics ??= new GraphicsSettingsData();
                _data.sound    ??= new SoundSettingsData();
                _data.gameplay ??= new GameplaySettingsData();

                return _data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SettingsFileManager] Corrupt file, resetting defaults: {e.Message}");
            }
        }

        // No file or corrupt — write clean defaults.
        _data = new AllSettingsData();
        Save();
        return _data;
    }

    // ── Save ─────────────────────────────────────────────────────────────────

    public static void Save()
    {
        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(_data ?? new AllSettingsData(), true));
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SettingsFileManager] Could not save settings: {e.Message}");
        }
    }

    // Convenience: push a modified section back then save.
    public static void SaveGraphics(GraphicsSettingsData d) { Data.graphics = d; Save(); }
    public static void SaveSound(SoundSettingsData d)       { Data.sound    = d; Save(); }
    public static void SaveGameplay(GameplaySettingsData d) { Data.gameplay = d; Save(); }
}