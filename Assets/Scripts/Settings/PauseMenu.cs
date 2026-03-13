using System;
using System.Collections;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance;
    public Action<bool> onPause;
    [SerializeField] private Transform parent;
    internal bool isPaused = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        parent.gameObject.SetActive(true);
        parent.gameObject.SetActive(false);
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        parent.gameObject.SetActive(false);
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            onPause?.Invoke(isPaused);
            
            isPaused = !isPaused;
            UiManager.ToggleUi(isPaused, true);
            Time.timeScale = isPaused ? 0 : 1;
            parent.gameObject.SetActive(isPaused);
        }
    }
}
