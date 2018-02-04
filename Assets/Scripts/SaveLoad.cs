using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class SaveLoad : MonoBehaviour {

    internal static List<float> scores;
    internal static float musicVol, fXVol;

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
        if (File.Exists("Highscores.binary"))
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
        else
        {
            scores = new List<float>();
        }
    }
}
[System.Serializable]
internal class SaveData
{
    internal List<float> Scores;
    internal float MusicVol, FXVol;
}
