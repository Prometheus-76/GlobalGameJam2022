using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DynamicPlatform : Switchable
{
    public Color onColour;
    public Color offColour;

    public LayerMask playerLayer;

    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;

    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();

        UpdateState();
    }

    private void FixedUpdate()
    {
        // If the player is inside the box and it is attempting to activate
        if (activated && boxCollider.enabled == false)
        {
            AttemptActivate();
        }
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
            AttemptActivate();
        }
        else
        {
            // Off state
            spriteRenderer.color = offColour;
            spriteRenderer.sortingLayerName = "Background";
            boxCollider.enabled = false;
        }
    }

    void AttemptActivate()
    {
        if (Physics2D.OverlapBox(transform.position, (boxCollider.size * transform.localScale) - (Vector2.one * 0.1f), transform.rotation.z, playerLayer) == false)
        {
            // On state
            spriteRenderer.color = onColour;
            spriteRenderer.sortingLayerName = "Default";
            boxCollider.enabled = true;
        }
    }    
}
