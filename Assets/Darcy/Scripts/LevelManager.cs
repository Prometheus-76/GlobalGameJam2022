using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using TMPro;

public class LevelManager : MonoBehaviour
{
    public TextMeshProUGUI levelTitleText;
    public TextMeshProUGUI levelTimeText;
    public GameObject levelCompleteUI;
    public GameObject nextStagePrompt;
    public ScreenWipe screenWipe;

    private int currentLevelNumber;

    struct LevelData
    {
        public int number;
        public bool completed;
        public int minutes;
        public int seconds;
        public string name;
    }

    private LevelData[] levels;
    private DataConversion dataConversion;

    private float levelTimer;
    private bool runTimer;
    private int seconds;
    private int minutes;

    private bool menuShown;

    public GameObject exitProgressObject;
    public Image exitProgressImage;
    private float exitTimer;
    private bool exitingStage;

    private InputMaster controls;

    private void Awake()
    {
        controls = new InputMaster();
    }

    // Start is called before the first frame update
    void Start()
    {
        // The current level number (0 is the first one)
        runTimer = true;
        currentLevelNumber = SceneManager.GetActiveScene().buildIndex - 2;
        levels = new LevelData[SceneManager.sceneCountInBuildSettings - 1];
        dataConversion = new DataConversion();
        levelTimer = 0f;
        seconds = 0;
        minutes = 0;
        menuShown = false;
        exitingStage = false;

        LoadLevelData();

        // Set gameplay UI
        levelTitleText.text = "<b>Level " + (currentLevelNumber + 1) + "\n<size=60%></b>" + levels[currentLevelNumber].name;
    }

    // Update is called once per frame
    void Update()
    {
        if (controls.Player.Return.ReadValue<float>() != 0f && exitingStage == false)
        {
            exitTimer += Time.deltaTime;
            if (exitTimer >= 1f)
            {
                screenWipe.WipeToCover(0.5f);
                StartCoroutine(LoadSceneDelayed(1));
                exitingStage = true;
            }
        }
        else if (exitingStage == false)
        {
            exitTimer = 0f;
        }

        // Update the exiting progress UI
        exitProgressImage.fillAmount = Mathf.Clamp01(exitTimer / 1f);
        exitProgressObject.SetActive(exitTimer > 0f);

        // Update timer
        if (runTimer)
        {
            levelTimer += Time.deltaTime;
        }

        seconds = Mathf.FloorToInt(levelTimer % 60f);
        minutes = Mathf.FloorToInt((levelTimer - seconds) / 60f);

        if (menuShown)
        {
            if (controls.Player.Return.triggered)
            {
                SceneManager.LoadScene(1);
            }
            else if (controls.Player.Interact.triggered && currentLevelNumber < levels.Length - 1)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            }
        }
    }

    void LoadLevelData()
    {
        int currentLevel = 0;

        while (currentLevel < SceneManager.sceneCountInBuildSettings - 2)
        {
            if (File.Exists("LevelData/lv" + currentLevel + ".lsf"))
            {
                StreamReader reader = new StreamReader("LevelData/lv" + currentLevel + ".lsf");

                // Read the data
                levels[currentLevel].number = int.Parse(dataConversion.Decrypt(reader.ReadLine(), (currentLevel + 1)));
                levels[currentLevel].completed = (dataConversion.Decrypt(reader.ReadLine(), (currentLevel + 1) * 2) == "true" ? true : false);
                levels[currentLevel].minutes = int.Parse(dataConversion.Decrypt(reader.ReadLine(), (currentLevel + 1) * 3));
                levels[currentLevel].seconds = int.Parse(dataConversion.Decrypt(reader.ReadLine(), (currentLevel + 1) * 4));
                levels[currentLevel].name = dataConversion.Decrypt(reader.ReadLine(), (currentLevel + 1) * 5);

                reader.Close();
                reader.Dispose();
            }

            currentLevel += 1;
        }
    }

    public void EndLevel()
    {
        levels[currentLevelNumber].completed = true;
        runTimer = false;

        // Update loaded data if this time is a new fastest
        if (minutes <= levels[currentLevelNumber].minutes || levels[currentLevelNumber].minutes <= -1)
        {
            levels[currentLevelNumber].minutes = minutes;

            if (seconds <= levels[currentLevelNumber].seconds || levels[currentLevelNumber].seconds <= -1)
            {
                levels[currentLevelNumber].seconds = seconds;
            }
        }

        SaveLevelData();
        screenWipe.WipeToCover(0.5f);

        // Bring up level complete screen
        levelTimeText.text = "Time: " + minutes + "m " + seconds + "s";
        StartCoroutine(ShowLevelCompleteDelayed());
    }

    IEnumerator ShowLevelCompleteDelayed()
    {
        yield return new WaitForSecondsRealtime(0.6f);
        nextStagePrompt.SetActive(currentLevelNumber < levels.Length - 1);
        levelCompleteUI.SetActive(true);
        menuShown = true;
        yield return null;
    }

    public void ReloadLevel()
    {
        screenWipe.WipeToCover(0.5f);
        StartCoroutine(LoadSceneDelayed(SceneManager.GetActiveScene().buildIndex));
    }

    IEnumerator LoadSceneDelayed(int stageNumber)
    {
        yield return new WaitForSecondsRealtime(0.6f);
        SceneManager.LoadScene(stageNumber);
        yield return null;
    }

    void SaveLevelData()
    {
        int currentLevel = 0;

        while (currentLevel < levels.Length)
        {
            if (File.Exists("LevelData/lv" + currentLevel + ".lsf") && currentLevel == currentLevelNumber)
            {
                StreamWriter writer = new StreamWriter("LevelData/lv" + currentLevel + ".lsf");

                string data = currentLevel.ToString();
                data = dataConversion.Encrypt(data, (currentLevel + 1));
                writer.WriteLine(data);

                data = (levels[currentLevelNumber].completed ? "true" : "false");
                data = dataConversion.Encrypt(data, (currentLevel + 1) * 2);
                writer.WriteLine(data);

                data = levels[currentLevelNumber].minutes.ToString();
                data = dataConversion.Encrypt(data, (currentLevel + 1) * 3);
                writer.WriteLine(data);

                data = levels[currentLevelNumber].seconds.ToString();
                data = dataConversion.Encrypt(data, (currentLevel + 1) * 4);
                writer.WriteLine(data);

                data = levels[currentLevelNumber].name;
                data = dataConversion.Encrypt(data, (currentLevel + 1) * 5);
                writer.WriteLine(data);

                writer.Close();
                writer.Dispose();
            }

            currentLevel += 1;
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
