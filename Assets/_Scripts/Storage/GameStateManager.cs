using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameData LoadedGameData { get; private set; }

    private StorageManager storageManager;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else Destroy(gameObject);
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Whenever Trying to Load
    public void SetLoadedGameData(GameData gameData)
    {
        LoadedGameData = gameData;
    }

    // ::::: When Starting a New Game
    public void ResetLoadedGameData()
    {
        LoadedGameData = null;

        LoadedGameData = new GameData
        {
            MapState = 0,
            SmokeState = 0,
            MaterialAmount = 80,
            graph = new GraphData(),
            tasks = new List<TaskData>(),
        };

        Debug.Log("LoadedGameData reseted and ready for a new game.");
    }

    // ::::: List All Available Saves
    public void ListSaves()
    {
        List<string> saveFiles = storageManager.ListSaveFiles();

        if (saveFiles.Count > 0)
        {
            Debug.Log("Guardados disponibles:");
            foreach (string saveFile in saveFiles)
                Debug.Log(saveFile);
        }
        else Debug.Log("No hay guardados disponibles.");
    }
}