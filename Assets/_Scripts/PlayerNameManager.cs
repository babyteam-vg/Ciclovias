using UnityEngine;
using UnityEngine.UI;

public class PlayerNameManager : MonoBehaviour
{
    public static PlayerNameManager Instance { get; private set; }

    private const string PlayerNameKey = "Green";

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: New Game
    public void SetPlayerName(string playerName)
    {
        PlayerPrefs.SetString(PlayerNameKey, playerName);
        PlayerPrefs.Save();
        Debug.Log("Name Saved: " + playerName);
    }

    public string GetPlayerName()
    {
        return PlayerPrefs.GetString(PlayerNameKey, "Green");
    }

    public void ClearPlayerName()
    {
        PlayerPrefs.DeleteKey(PlayerNameKey);
        PlayerPrefs.Save();
    }
}