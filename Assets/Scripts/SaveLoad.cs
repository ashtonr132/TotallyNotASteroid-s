using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveLoad : MonoBehaviour {

    internal static List<ScoreFormat> scores;
    internal static float musicVol = 0.5f, fXVol = 0.5f;

    internal static void Save()
    {
        SaveData saveData = new SaveData()
        {
            Scores = scores,
            MusicVol = musicVol,
            FXVol = fXVol
        };
        BinaryFormatter formatter = new BinaryFormatter();
        FileStream saveFile = File.Open("Highscores.binary", FileMode.Create);
        formatter.Serialize(saveFile, saveData);
        saveFile.Close();
    }
    internal static void Load()
    {
        if (File.Exists("Highscores.binary") && scores != null)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            FileStream saveFile = File.Open("Highscores.binary", FileMode.Open);
            try
            {
                SaveData saveData = (SaveData)formatter.Deserialize(saveFile);
                scores = saveData.Scores;
                musicVol = saveData.MusicVol;
                fXVol = saveData.FXVol;
            }
            catch (System.Exception)
            {
                Debug.Log("File Corrupted");
                throw;
            }
            saveFile.Close();
        }
        else if(scores == null)
        {
            scores = new List<ScoreFormat>();
        }
    }
}
[System.Serializable]
internal class SaveData
{
    internal List<ScoreFormat> Scores;
    internal float MusicVol, FXVol;
}
[System.Serializable]
internal class ScoreFormat
{
    internal float Score;
    internal int Level;
    internal string Name;
}
