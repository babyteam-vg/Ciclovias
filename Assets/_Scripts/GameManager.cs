using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int MapState { get; private set; } = 0;
    public int SmokeState { get; private set; } = 0;
    public int MaterialAmount { get; private set; } = 120;

    [Header("Dependencies")]
    [SerializeField] private Graph graph;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TaskDiary taskDiary;
    [SerializeField] private InputManager inputManager; // PLACEHOLDER!!!
    private StorageManager storageManager = new StorageManager();

    [Header("UI References")]
    [SerializeField] private GameObject materialCounter;
    [SerializeField] private TextMeshProUGUI amountText;

    private Animator animator;
    private bool placeholderTutorial = true;

    public event Action<int> MapStateAdvanced;
    public event Action<int> SmokeStateReduced;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        taskManager.TaskSealed += OnTaskSealed;
        inputManager.OnCursorMove += PlaceHolderTutorial; // PLACEHOLDER!!!
    }
    private void OnDisable()
    {
        taskManager.TaskSealed -= OnTaskSealed;
        inputManager.OnCursorMove -= PlaceHolderTutorial; // PLACEHOLDER!!!
    }

    private void Start()
    {
        ApplyLoadedGameData(GameStateManager.Instance.LoadedGameData);

        amountText.text = "x" + MaterialAmount.ToString();
        animator = materialCounter.GetComponent<Animator>();

        List<Task> unlockedTasks = TaskDiary.Instance.tasks.Where(t => t.state == TaskState.Unlocked).ToList();
        foreach (Task acceptedTask in unlockedTasks)
            acceptedTask.from.GetNextAvailableTask(MapState);
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Map
    public void AdvanceMapState() {
        MapState++;
        MapStateAdvanced?.Invoke(MapState); // !
    }

    // ::::: Smoke
    public void ReduceSmokeState()
    {
        SmokeState--;
        SmokeStateReduced?.Invoke(SmokeState); // !
    }

    // ::::: Material
    public void AddMaterial(int amount)
    {
        MaterialAmount += amount;
        amountText.text = "x" + MaterialAmount.ToString();
        MaterialCounterAnimation();
    }
    public bool ConsumeMaterial(int amount)
    {
        if (MaterialAmount >= amount)
        {
            MaterialAmount -= amount;
            amountText.text = "x" + MaterialAmount.ToString();
            MaterialCounterAnimation();
            return true;
        }
        else return false;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void PlaceHolderTutorial(Vector2Int _) // PLACEHOLDER!!!
    {
        if (placeholderTutorial && TaskDiary.Instance.tasks[0].state == TaskState.Unlocked)
        {
            placeholderTutorial = false;
            TutorialManager.Instance.PlayTutorial(new Vector2Int(0, 1));
        }
    }

    private void MaterialCounterAnimation()
    {
        animator.Play("MaterialCounter");
    }

    private void OnTaskSealed(Task task)
    {
        SaveGame(); // Auto Save
    }

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Save
    public void SaveGame(string fileName = null)
    {
        GameData gameData = new GameData
        {
            MapState = MapState,
            SmokeState = SmokeState,
            MaterialAmount = MaterialAmount,
            graph = graph.SaveGraph(),
            tasks = TaskDiary.Instance.SaveTasks()
        };

        bool success = fileName == null
            ? storageManager.AutoSaveGame(gameData)
            : storageManager.ManualSaveGame(fileName, gameData);

        if (success) Debug.Log("GAME SAVED");
        else Debug.LogError("ERROR SAVING");
    }

    // ::::: Load
    private void ApplyLoadedGameData(GameData gameData)
    {
        if (gameData != null)
        {
            MapState = gameData.MapState;
            SmokeState = gameData.SmokeState;
            MaterialAmount = gameData.MaterialAmount;
            graph.LoadGraph(gameData.graph);
            taskDiary.LoadTasks(gameData.tasks);

            amountText.text = "x" + MaterialAmount.ToString();

            Debug.Log("GAME LOADED");
        }
        else Debug.Log("NO GAME DATA");
    }
}
