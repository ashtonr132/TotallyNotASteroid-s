using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtonFunctions : MonoBehaviour {

    private GameObject StartB, HighsB, SettiB, CrediB, ExitGB, MusicSlider, SFXSlider;
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
        CrediB = GameObject.Find("Credits");
        ExitGB = GameObject.Find("Exit Game");
        MusicSlider = GameObject.Find("MusicVol");
        SFXSlider = GameObject.Find("Sound Effects Vol");
        MusicSlider.SetActive(false);
        SFXSlider.SetActive(false);
        MusicSlider.GetComponent<Slider>().value = SaveLoad.musicVol;
        SFXSlider.GetComponent<Slider>().value = SaveLoad.fXVol;
        AdditionalText = GameObject.Find("AdditionalText").GetComponent<Text>();
        StartB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(StartB.name);});
        HighsB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(HighsB.name);});
        SettiB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(SettiB.name);});
        CrediB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(CrediB.name);});
        ExitGB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(ExitGB.name);});
    }
    private void Update()
    {
        if (MusicSlider.activeSelf)
        {
            SaveLoad.musicVol = MusicSlider.GetComponent<Slider>().value;
            SaveLoad.fXVol = SFXSlider.GetComponent<Slider>().value;
            GameObject.Find("Music").GetComponent<AudioSource>().volume = SaveLoad.musicVol;
        }
    }
    void MenuButtons(string name)
    {
        switch (name)
        {
            case "Start Game":
                SaveLoad.Save();
                SceneManager.LoadScene("Play");
                break;
            case "HighScores":
                if (MusicSlider.activeSelf)
                {
                    MusicSlider.SetActive(false);
                    SFXSlider.SetActive(false);
                }
                AdditionalText.text = string.Empty;
                AdditionalText.text = "HighScores!";
                if (SaveLoad.scores != null)
                {
                    SaveLoad.scores.Sort();
                    for (int i = SaveLoad.scores.Count - 1, j = 1; i >= 0; i--, j++)
                    {
                        if (j < 21)
                        {
                            ScoreFormat sf = SaveLoad.scores[i];
                            AdditionalText.text += System.Environment.NewLine + j + ". " + sf.Score + ", " + sf.Level + ", " +  sf.Name;
                        }
                    }
                }
                else
                {
                    SaveLoad.Load();
                }
                if (AdditionalText.text == "HighScores!")
                {
                    AdditionalText.text += System.Environment.NewLine + "No highscores set yet!";
                }
                break;
            case "Settings":
                MusicSlider.SetActive(true);
                SFXSlider.SetActive(true);
                AdditionalText.text = string.Empty;
                break;
            case "Credits":
                if (MusicSlider.activeSelf)
                {
                    MusicSlider.SetActive(false);
                    SFXSlider.SetActive(false);
                }
                AdditionalText.text = "Music;" + System.Environment.NewLine;
                AdditionalText.text += "Space Boss Battle Theme, 'Matthew Pablo'" + System.Environment.NewLine
                                    + "Wheres My Spaceship, 'Spuispuin'" + System.Environment.NewLine
                                    + "Space Boss Battle, 'Hitctrl'" + System.Environment.NewLine
                                    + "Hypersspace, 'MidFag'" + System.Environment.NewLine
                                    + "Less-Appealing, 'Macro'" + System.Environment.NewLine
                                    + "Through-Space, 'Maxstack'" + System.Environment.NewLine
                                    + "Galactic Temple, 'yd'" + System.Environment.NewLine
                                    + "Space Music, 'MrPoly'" + System.Environment.NewLine + System.Environment.NewLine
                                    + "Sound Effects;" + System.Environment.NewLine
                                    + "8-Bit Sound Effect Pack Vol001, 'Xenocity'" + System.Environment.NewLine + System.Environment.NewLine
                                    + "Art;" + System.Environment.NewLine
                                    + "Asteroids, 'Sumbada'" + System.Environment.NewLine
                                    + "HorseHead Nebula, 'HDSpaceWallpapers4305'" + System.Environment.NewLine + System.Environment.NewLine
                                    + "Font;" + System.Environment.NewLine 
                                    + "Demonized, 'GreyWolf Webworks'" + System.Environment.NewLine;
                break;
            case "Exit Game":
                    SaveLoad.Save();
                    Application.Quit();
                break;
        }
    }
}
