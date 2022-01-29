using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public ScreenWipe screenWipe;

    public void NavigateToLevelSelect()
    {
        screenWipe.WipeToCover(0.5f);
        StartCoroutine(DelayedNavigate());
    }

    IEnumerator DelayedNavigate()
    {
        yield return new WaitForSecondsRealtime(0.8f);
        SceneManager.LoadScene(1);
        yield return null;
    }

    public void CloseGame()
    {
        Application.Quit();
    }
}
