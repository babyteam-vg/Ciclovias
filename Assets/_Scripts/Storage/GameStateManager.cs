using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameData LoadedGameData { get; private set; }
    public bool isLoaded { get; private set; }

    private StorageManager storageManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else Destroy(gameObject);
    }

    public void SetLoadedGameData(GameData gameData)
    {
        LoadedGameData = gameData;
    }
}