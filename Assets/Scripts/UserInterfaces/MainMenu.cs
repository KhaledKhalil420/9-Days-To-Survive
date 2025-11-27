using DG.Tweening;
using Unity.Collections;
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
    }

    private void LoadGame()
    {
        parentGroup.interactable = false;
        DOVirtual.Float(source.volume, 0, 4, x => source.volume = x).OnComplete(() => SceneManager.LoadScene(1));
        DOVirtual.Float(group.alpha, 1, 2, x => group.alpha = x);
    }
}
