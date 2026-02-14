using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AutoStaticReset : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetAllStatics();
    }

    void ResetAllStatics()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsClass || !type.IsAbstract || !type.IsSealed) // only static classes
                    continue;

                foreach (var field in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (field.IsLiteral) continue; // skip const
                    if (field.FieldType.IsValueType)
                        field.SetValue(null, Activator.CreateInstance(field.FieldType));
                    else
                        field.SetValue(null, null);
                }
            }
        }
    }
}
