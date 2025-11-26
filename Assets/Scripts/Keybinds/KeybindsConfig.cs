using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


[CreateAssetMenu(fileName = "KeybindsConfig", menuName = "Input/Keybinds Config")]
public class KeybindsConfig : ScriptableObject
{
    
    public List<KeyEntry> keyEntries = new();
}
