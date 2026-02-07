using UnityEngine;
using UnityEngine.UI;

public class ScrollingBackground : MonoBehaviour
{
    public RawImage RawImage;
    public Vector2 position;

    void Update()
    {
        RawImage.uvRect = new Rect(RawImage.uvRect.position + new Vector2(position.x, position.y) * Time.unscaledDeltaTime, RawImage.uvRect.size);
    }
}
