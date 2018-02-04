using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtonFunctions : MonoBehaviour {

    private GameObject StartB, HighsB, SettiB, CCB, ExitGB;
    private Text AdditionalText;
    // Use this for initialization
    private void Awake()
    {
        SaveLoad.Load();
    }
    void Start ()
    {
        StartB = GameObject.Find("Start Game");
        HighsB = GameObject.Find("HighScores");
        SettiB = GameObject.Find("Settings");
        CCB = GameObject.Find("Credits and Copyrights");
        ExitGB = GameObject.Find("Exit Game");
        AdditionalText = GameObject.Find("AdditionalText").GetComponent<Text>();
        StartB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(StartB.name);});
        HighsB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(HighsB.name);});
        SettiB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(SettiB.name);});
        CCB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(CCB.name);});
        ExitGB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(ExitGB.name);});
    }
	
    void MenuButtons(string name)
    {
        switch (name)
        {
            case "Start Game":
                    SceneManager.LoadScene("Play");
                break;
            case "HighScores":
                AdditionalText.text = string.Empty;
                AdditionalText.text = "HighScores!";
                SaveLoad.scores.Sort();
                for (int i = SaveLoad.scores.Count - 1, j = 1; i >= 0; i--, j++)
                {
                    if (j < 21)
                    {
                        AdditionalText.text += System.Environment.NewLine + j + ". " + Mathf.Round(SaveLoad.scores[i]);
                    }
                }
                if (AdditionalText.text == "HighScores!")
                {
                    AdditionalText.text += System.Environment.NewLine + "No highscores yet!";
                }
                break;
            case "Settings":
                break;
            case "Credits and Copyrights":
                break;
            case "Exit Game":
                    Application.Quit();
                break;
        }
    }
}
