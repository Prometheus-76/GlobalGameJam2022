using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    public bool isDayTime = true;
    private List<Switchable> switchableObjects;
    private List<GameObject> dayObjects;
    private List<GameObject> nightObjects;

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

        nightObjects = new List<GameObject>();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Night"))
        {
            nightObjects.Add(g);
        }

        dayObjects = new List<GameObject>();
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Day"))
        {
            dayObjects.Add(g);
        }

        // Set daytime and nighttime objects on/off at the start of the level
        foreach (GameObject g in dayObjects)
        {
            g.SetActive(isDayTime);
        }

        foreach (GameObject g in nightObjects)
        {
            g.SetActive(!isDayTime);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (controls.Player.Switch.triggered)
        {
            ChangeAllStates();
        }
    }

    void ChangeAllStates()
    {
        isDayTime = !isDayTime;
        for (int i = 0; i < switchableObjects.Count; i++)
        {
            switchableObjects[i].ChangeState();
        }

        foreach (GameObject g in dayObjects)
        {
            g.SetActive(isDayTime);
        }

        foreach (GameObject g in nightObjects)
        {
            g.SetActive(!isDayTime);
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
