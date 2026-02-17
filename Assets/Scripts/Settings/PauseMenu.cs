using System.Collections;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializeField] private Transform parent;
    private bool isPaused = false;

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
            isPaused = !isPaused;
            UiManager.ToggleUi(isPaused, true);
            Time.timeScale = isPaused ? 0 : 1;
            parent.gameObject.SetActive(isPaused);
        }
    }
}
