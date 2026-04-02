using UnityEngine;

public class SnappingPoint : MonoBehaviour
{
    public MeshRenderer meshRenderer;
    public Building snappedTo;
    public bool isOccupied => snappedTo != null;
}