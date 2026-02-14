using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private CanvasGroup parentGroup;
    [SerializeField] private CanvasGroup group;
    [SerializeField] private AudioSource source;

    void Start()
    {
        startButton.onClick.AddListener(LoadGame);

        group.alpha = 1;
        DOVirtual.Float(group.alpha, 0, 2, x => group.alpha = x);
    }

    private void LoadGame()
    {
        DOTween.KillAll();
        
        parentGroup.interactable = false;

        //Fade out music
        DOVirtual.Float(source.volume, 0, 4, x => source.volume = x);

        //Fade in black screen
        DOVirtual.Float(group.alpha, 1, 2, x => group.alpha = x);

        //Start async scene load after short delay
        StartCoroutine(LoadSceneAsync("World", 2f));
    }

    private System.Collections.IEnumerator LoadSceneAsync(string sceneName, float delay)
    {
        yield return new WaitForSeconds(delay);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
                asyncLoad.allowSceneActivation = true;

            yield return null;
        }
    }
}
