using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicGravity : Switchable
{
    public GameObject onModel;
    public GameObject offModel;

    // Start is called before the first frame update
    void Start()
    {
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
            onModel.SetActive(true);
            offModel.SetActive(false);
        }
        else
        {
            // Off state
            onModel.SetActive(false);
            offModel.SetActive(true);
        }
    }
}
