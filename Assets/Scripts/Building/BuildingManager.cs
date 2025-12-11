using System.Collections.Generic;
using UnityEngine;

public class BuilderManager : MonoBehaviour
{
    public static BuilderManager instance;
    public List<Building> availableBuilds = new();

    void Awake()
    {
        instance = this;
    }

    public static void AddBuild(Building building)
    {
        if(instance != null)
            instance.availableBuilds.Add(building);
    }
}
