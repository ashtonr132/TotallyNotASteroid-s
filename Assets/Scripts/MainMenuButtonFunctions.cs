using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuButtonFunctions : MonoBehaviour {
    [SerializeField]
    private GameObject StartB, HighsB, SettiB, CrediB, ExitGB, MusicSlider, SFXSlider, Music;
    [SerializeField]
    private Text AdditionalText;
    [SerializeField]
    private PostProcessingProfile ppb;
    // Use this for initialization
    private void Awake()
    {
        SaveLoad.Load();
    }
    void Start ()
    {
        MusicSlider.GetComponent<Slider>().value = SaveLoad.musicVol;
        SFXSlider.GetComponent<Slider>().value = SaveLoad.fXVol;
        Music.GetComponent<AudioSource>().volume = SaveLoad.musicVol;
        MusicSlider.SetActive(false);
        SFXSlider.SetActive(false);
        StartB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(StartB.name);});
        HighsB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(HighsB.name);});
        SettiB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(SettiB.name);});
        CrediB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(CrediB.name);});
        ExitGB.GetComponent<Button>().onClick.AddListener(delegate{MenuButtons(ExitGB.name);});
    }
    private void Update()
    {
        var ppp = ppb.colorGrading.settings;
        SaveLoad.hueShift += 0.1f;
        ppp.basic.hueShift = SaveLoad.hueShift;
        ppb.colorGrading.settings = ppp;
        if (MusicSlider.activeSelf)
        {
            SaveLoad.musicVol = MusicSlider.GetComponent<Slider>().value;
            SaveLoad.fXVol = SFXSlider.GetComponent<Slider>().value;
            Music.GetComponent<AudioSource>().volume = SaveLoad.musicVol;
        }
    }
    void MenuButtons(string name)
    {
        switch (name)
        {
            case "Start Game":
                SceneManager.LoadScene("Play");
                break;
            case "HighScores":
                if (MusicSlider.activeSelf)
                {
                    MusicSlider.SetActive(false);
                    SFXSlider.SetActive(false);
                }
                AdditionalText.text = "HighScores!";
                if (SaveLoad.scores != null)
                {
                    SaveLoad.scores = SaveLoad.scores.OrderBy(w => w.Score).ToList();
                    for (int i = SaveLoad.scores.Count - 1, j = 1; i >= 0; i--, j++)
                    {
                        if (j < 21)
                        {
                            ScoreFormat sf = SaveLoad.scores[i];
                            AdditionalText.text += System.Environment.NewLine + j + ". " + sf.Score + ", Level. " + sf.Level + ", Name. " +  sf.Name;
                        }
                    }
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
                                    + "VFX;" + System.Environment.NewLine
                                    + "Particle Systems/Magic, 'UETools'" + System.Environment.NewLine + System.Environment.NewLine
                                    + "Post Processing Stack;" + System.Environment.NewLine
                                    + "Unity Essentials, 'Unity Technologies'" + System.Environment.NewLine + System.Environment.NewLine
                                    + "Font;" + System.Environment.NewLine
                                    + "Demonized, 'GreyWolf Webworks'" + System.Environment.NewLine + System.Environment.NewLine
                                    + "Invaluable Help;" + System.Environment.NewLine
                                    + "'Ethan Bruins'" + System.Environment.NewLine + System.Environment.NewLine
                                    + "PlayTesters;" + System.Environment.NewLine
                                    + "'Dominik Bauer'" + System.Environment.NewLine
                                    + "'James Morgan'" + System.Environment.NewLine + System.Environment.NewLine
                                    + "The Rest;" + System.Environment.NewLine
                                    + "Robert Ashton";
                break;
            case "Exit Game":
                    SaveLoad.Save();
                    Application.Quit();
                break;
        }
    }
}
