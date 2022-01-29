using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicGravity : Switchable
{
    public Color onColour;
    public Color offColour;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        UpdateState();
    }

    // Swaps between active and inactive states
    public override void ChangeState()
    {
        activated = !activated;
        UpdateState();
    }

    public override void UpdateState()
    {
        if (activated)
        {
            // On state
            spriteRenderer.color = onColour;
            spriteRenderer.sortingLayerName = "Default";
        }
        else
        {
            // Off state
            spriteRenderer.color = offColour;
            spriteRenderer.sortingLayerName = "Background";
        }
    }
}
