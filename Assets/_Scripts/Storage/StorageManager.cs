using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class StorageManager
{
    private JsonDataService dataService = new JsonDataService();

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Manual Save
    public bool ManualSaveGame(string fileName, GameData gameData, bool encrypted = false)
    {
        if (!IsValidFileName(fileName))
        {
            Debug.LogError("Invalid file name.");
            return false;
        }

        string path = $"/{fileName}.json";

        try
        {
            return dataService.SaveData(path, gameData, encrypted);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error trying to save manually: {e.Message}");
            return false;
        }
    }

    // ::::: Auto Save
    public bool AutoSaveGame(GameData gameData, int maxAutoSaves = 3, bool encrypted = false)
    {
        try
        {
            RotateAutoSaves(maxAutoSaves);

            string fileName = "/auto_save_1.json";
            return dataService.SaveData(fileName, gameData, encrypted);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error trying to save automatically: {e.Message}");
            return false;
        }
    }

    // ::::: Load
    public GameData LoadGame(string fileName, bool encrypted = false)
    {
        string path = $"/{fileName}.json";

        try
        {
            return dataService.LoadData<GameData>(path, encrypted);
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
    public bool DeleteSaveFile(string fileName)
    {
        string path = Application.persistentDataPath + $"/{fileName}.json";

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("Savefile successfully deleted: " + path);
            return true;
        }

        Debug.LogWarning("Savefile not found: " + path);
        return false;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: List Save Files
    public List<string> ListSaveFiles()
    {
        List<string> saveFiles = new List<string>();
        string saveDirectory = Application.persistentDataPath;

        if (Directory.Exists(saveDirectory))
        {
            foreach (string filePath in Directory.GetFiles(saveDirectory, "*.json"))
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                saveFiles.Add(fileName);
            }
        }

        return saveFiles;
    }

    // For the Continue Button
    public string GetMostRecentSaveFile()
    {
        string saveDirectory = Application.persistentDataPath;
        string mostRecentFile = null;
        DateTime mostRecentDate = DateTime.MinValue;

        if (Directory.Exists(saveDirectory))
        {
            foreach (string filePath in Directory.GetFiles(saveDirectory, "*.json"))
            {
                DateTime lastModified = File.GetLastWriteTime(filePath);
                if (lastModified > mostRecentDate)
                {
                    mostRecentDate = lastModified;
                    mostRecentFile = Path.GetFileNameWithoutExtension(filePath);
                }
            }
        }

        return mostRecentFile;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Filename Validation
    private bool IsValidFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return false;

        char[] invalidChars = Path.GetInvalidFileNameChars();
        if (fileName.IndexOfAny(invalidChars) >= 0)
            return false;

        if (fileName.Length > 16)
            return false;

        return true;
    }

    // ::::: Auto Save Rotation
    private void RotateAutoSaves(int maxAutoSaves)
    {
        for (int i = maxAutoSaves - 1; i >= 1; i--)
        {
            string oldPath = Application.persistentDataPath + $"/auto_save_{i}.json";
            string newPath = Application.persistentDataPath + $"/auto_save_{i + 1}.json";

            if (File.Exists(oldPath))
            {
                if (File.Exists(newPath))
                    File.Delete(newPath);
                File.Move(oldPath, newPath);
            }
        }
    }
}