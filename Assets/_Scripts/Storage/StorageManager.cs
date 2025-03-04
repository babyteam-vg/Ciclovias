using System;
using System.IO;
using UnityEngine;

public class StorageManager
{
    private JsonDataService dataService = new JsonDataService();

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Save
    public bool SaveGame(GameData gameData, string fileName = "/savegame.json", bool encrypted = false)
    {
        try
        {
            return dataService.SaveData(fileName, gameData, encrypted);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error trying to save: {e.Message}");
            return false;
        }
    }

    // ::::: Load
    public GameData LoadGame(string fileName = "/savegame.json", bool encrypted = false)
    {
        try
        {
            return dataService.LoadData<GameData>(fileName, encrypted);
        }
        catch (FileNotFoundException)
        {
            Debug.LogWarning("Savefile not found.");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error trying to load: {e.Message}");
            return null;
        }
    }

    // ::::: Delete
    public bool DeleteSaveFile(string fileName = "/savegame.json")
    {
        string path = Application.persistentDataPath + fileName;

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Savefile successfully deleted: " + path);
            return true;
        }

        Debug.LogWarning("Savefile not found: " + path);
        return false;
    }
}