using DG.Tweening;
using UnityEngine;

public class HardwoodTree : Breakable_GiveOnDeath
{
    public void BreakBarier()
    {
        GetComponent<Renderer>().materials[2].DOFade(0, 0.25f);
        hitable = true;
    }
}
