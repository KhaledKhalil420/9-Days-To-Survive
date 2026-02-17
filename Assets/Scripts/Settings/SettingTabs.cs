using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SettingsTab
{
    public GameObject associatedTab;
    public GameObject associatedButton;
}

public class SettingTabs : MonoBehaviour
{
    [SerializeField] private List<SettingsTab> settingsTab;

    private void Awake()
    {
        for (int i = 0; i < settingsTab.Count; i++)
        {
            int index = i;
            settingsTab[i].associatedButton.GetComponent<Button>().onClick.AddListener(() => OpenTab(index));
        }

        OpenTab(0);
    }

    private void OpenTab(int index)
    {
        foreach (var tab in settingsTab)
            tab.associatedTab.SetActive(false);

        settingsTab[index].associatedTab.SetActive(true);
    }
}