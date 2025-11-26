using UnityEngine;

public enum CraftStationButtonType {Backward, Forward, Craft}
public class CraftStationButton : MonoBehaviour, IInteractable
{
    [SerializeField] private CraftingStation craftingStation;
    [SerializeField] private CraftStationButtonType buttonType;

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
    }
}
