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
        //Reset time
        Time.timeScale = 1;
        Time.fixedDeltaTime = 0.01f;

        //Unlock mouse
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //Setup buttons
        startButton.onClick.AddListener(LoadGame);
        
        //Fade out
        group.alpha = 1;
        group.DOFade(0, 2);
    }

    private void LoadGame()
    {        
        parentGroup.interactable = false;

        //Fade out music
        source.DOFade(0, 4);

        //Fade in black screen
        group.DOFade(1, 2);

        //Start async scene load after short delay
        DOVirtual.DelayedCall(5, () => SceneManager.LoadScene("World")).OnComplete(() => DOTween.KillAll());
    }
}
