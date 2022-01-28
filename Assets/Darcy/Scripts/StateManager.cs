using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    private List<Switchable> switchableObjects;

    private InputMaster controls;

    private void Awake()
    {
        // Instantiation
        controls = new InputMaster();
    }

    // Start is called before the first frame update
    void Start()
    {
        switchableObjects = new List<Switchable>();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Switchable"))
        {
            switchableObjects.Add(g.GetComponent<Switchable>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (controls.Player.Interact.triggered)
        {
            ChangeAllStates();
        }
    }

    void ChangeAllStates()
    {
        for (int i = 0; i < switchableObjects.Count; i++)
        {
            switchableObjects[i].ChangeState();
        }
    }

    #region Input System

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    #endregion
}
