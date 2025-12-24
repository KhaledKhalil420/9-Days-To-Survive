using UnityEngine;

public interface IHoldInteractable 
{
    float HoldDuration { get; }
    public float holdProgress {get; set;}
    void OnHoldProgress(float progress); 
    void OnHoldComplete(GameObject sender);
}
