using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScreenWipe : MonoBehaviour
{
    public float openingDuration;
    public Image transitionImage;
    public Canvas transitionCanvas;

    private float duration;
    private float timer;
    private bool toScene;

    // Start is called before the first frame update
    void Start()
    {
        WipeToScene(openingDuration);
    }

    // Update is called once per frame
    void Update()
    {
        // Increment timer
        timer -= Time.deltaTime;
        transitionImage.fillAmount = (toScene == false ? 1f - CosEasing01(Mathf.Clamp((timer / duration), 0f, 1f)) : CosEasing01(Mathf.Clamp((timer / duration), 0f, 1f)));
        transitionCanvas.enabled = (timer > 0f) || toScene == false;
    }

    void WipeToScene(float transitionDuration)
    {
        toScene = true;
        duration = transitionDuration;
        timer = duration;
    }

    public void WipeToCover(float transitionDuration)
    {
        toScene = false;
        duration = transitionDuration;
        timer = duration;
    }

    float CosEasing01(float input)
    {
        float output = Mathf.Cos((Mathf.PI * Mathf.Clamp01(input)) / 2f) * Mathf.Cos((Mathf.PI * Mathf.Clamp01(input)) / 2f);
        return Mathf.Clamp01(1f - output);
    }
}
