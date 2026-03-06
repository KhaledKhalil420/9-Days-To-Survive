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
    public ItemData input;
    public float timeToSmelt;
    public ItemData output;
}

[System.Serializable]
public class Fuel
{
    public ItemData item;
    public int efficiency = 1;
}