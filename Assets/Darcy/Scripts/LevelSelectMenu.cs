using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelSelectMenu : MonoBehaviour
{
    struct LevelData
    {
        public int number;
        public bool completed;
        public int minutes;
        public int seconds;
        public string name;
    }

    private LevelData[] levels;

    public List<Button> selectButtons;
    public string[] levelNames;

    public TextMeshProUGUI levelNameText;
    public TextMeshProUGUI levelTimeText;
    public TextMeshProUGUI completionText;
    private DataConversion dataConversion;

    // Start is called before the first frame update
    void Start()
    {
        dataConversion = new DataConversion();
        levels = new LevelData[selectButtons.Count];
        VerifyLevelData();
        GetLevelData();
        UnlockStages();

        float completedStages = 0f;
        for (int i = 0; i < levels.Length; i++)
        {
            completedStages += (levels[i].completed == true ? 1f : 0f);
        }

        completionText.text = ((completedStages / levels.Length) * 100f).ToString("F0") + "% Complete";
    }

    // Checks to ensure level files exist, recreating them where they don't
    void VerifyLevelData()
    {
        int currentLevel = 0;

        while (currentLevel < selectButtons.Count)
        {
            if (File.Exists("LevelData/lv" + currentLevel + ".lsf") == false)
            {
                StreamWriter writer = new StreamWriter("LevelData/lv" + currentLevel + ".lsf");

                string data = currentLevel.ToString();
                data = dataConversion.Encrypt(data, (currentLevel + 1));
                writer.WriteLine(data);

                data = "false";
                data = dataConversion.Encrypt(data, (currentLevel + 1) * 2);
                writer.WriteLine(data);

                data = "-1";
                data = dataConversion.Encrypt(data, (currentLevel + 1) * 3);
                writer.WriteLine(data);

                data = "-1";
                data = dataConversion.Encrypt(data, (currentLevel + 1) * 4);
                writer.WriteLine(data);

                data = levelNames[currentLevel];
                data = dataConversion.Encrypt(data, (currentLevel + 1) * 5);
                writer.WriteLine(data);

                writer.Close();
                writer.Dispose();
            }
            
            currentLevel += 1;
        }
    }

    // Reads level save data
    void GetLevelData()
    {
        int currentLevel = 0;
        
        while (currentLevel < selectButtons.Count)
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

    // Unlocks buttons based on appropriate stage completions
    void UnlockStages()
    {       
        for (int i = 0; i < levels.Length - 1; i++)
        {
            selectButtons[i + 1].interactable = levels[i].completed;
        }
    }

    // Update UI when button is hovered over
    public void DisplayLevelInfo(int stageNumber)
    {
        levelNameText.text = "- " + levels[stageNumber].name + " -";
        if (levels[stageNumber].minutes != -1 && levels[stageNumber].seconds != -1)
        {
            levelTimeText.text = "Fastest Time: " + levels[stageNumber].minutes + "m " + levels[stageNumber].seconds + "s";
        }
        else
        {
            levelTimeText.text = "No Time Recorded";
        }
    }

    // Load the level when clicked on button
    public void LoadStage(int stageNumber)
    {
        StartCoroutine(LoadSceneDelayed(stageNumber));
    }

    IEnumerator LoadSceneDelayed(int stageNumber)
    {
        yield return new WaitForSecondsRealtime(0.7f);
        SceneManager.LoadScene(stageNumber + 2);
        yield return null;
    }
}
