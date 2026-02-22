using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class Smeltables : ScriptableObject
{
    public List<Fuel> fuel;
    public List<Smeltable> smeltables;
}

[System.Serializable]
public class Smeltable
{
    public Item input;
    public Item output;
}

[System.Serializable]
public class Fuel
{
    public Item item;
    public int efficiency = 1;
}