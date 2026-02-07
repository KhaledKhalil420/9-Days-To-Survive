using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Transform parent;
    private bool isPaused = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            isPaused = !isPaused;
            UiManager.ToggleUi(isPaused);
            Time.timeScale = isPaused ? 1 : 0;
            parent.gameObject.SetActive(isPaused);
        }
    }
}
