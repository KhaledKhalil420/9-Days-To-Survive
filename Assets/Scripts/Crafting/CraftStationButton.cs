using DG.Tweening;
using UnityEngine;

public enum CraftStationButtonType {Backward, Forward, Craft}
public class CraftStationButton : MonoBehaviour, IInteractable
{
    [SerializeField] private CraftingStation craftingStation;
    [SerializeField] private CraftStationButtonType buttonType;
    [SerializeField] private AudioSource source;
    private float initialY;

    void Start()
    {
        initialY = transform.localPosition.y;
    }

    public void Interact(GameObject sender)
    {
        switch (buttonType)
        {
            case CraftStationButtonType.Backward:
            craftingStation.DisplayBackwardRecipe();
            break;  

            case CraftStationButtonType.Forward:
            craftingStation.DisplayForwardRecipe();
            break;  

            case CraftStationButtonType.Craft:
            craftingStation.CraftRecipe(sender);
            break;  
        }

        source.Play();
        transform.DOLocalMoveY(initialY / 1.01f, 0.25f).OnComplete(() => transform.DOLocalMoveY(initialY, 0.25f));
    }
}
