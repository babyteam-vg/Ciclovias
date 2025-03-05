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
    public int MaterialAmount { get; private set; } = 80;

    [Header("Dependencies")]
    [SerializeField] private Graph graph;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TaskDiary taskDiary;
    private StorageManager storageManager = new StorageManager();

    [Header("UI References")]
    [SerializeField] private GameObject materialCounter;
    [SerializeField] private TextMeshProUGUI amountText;

    private Animator animator;

    public event Action<int> MapStateAdvanced;
    public event Action<int> SmokeStateReduced;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        taskManager.TaskCompleted += OnTaskCompleted;
    }
    private void OnDisable()
    {
        taskManager.TaskCompleted -= OnTaskCompleted;
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) TutorialManager.Instance.PlayTutorial(new Vector2Int(0, 1));
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
    public void AddMaterial(int cantidad)
    {
        MaterialAmount += cantidad;
        amountText.text = "x" + MaterialAmount.ToString();
        MaterialCounterAnimation();
    }
    public bool ConsumeMaterial(int cantidad)
    {
        if (MaterialAmount >= cantidad)
        {
            MaterialAmount -= cantidad;
            amountText.text = "x" + MaterialAmount.ToString();
            MaterialCounterAnimation();
            return true;
        }
        else
            return false;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void MaterialCounterAnimation()
    {
        animator.Play("MaterialCounter");
    }

    private void OnTaskCompleted(Task task)
    {
        SaveGame(); // Auto Save
    }

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Save
    public void SaveGame(string fileName = null)
    {
        GameData gameData = new GameData
        {
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
        graph.LoadGraph(gameData.graph);
        taskDiary.LoadTasks(gameData.tasks);

        Debug.Log("GAME LOADED");
    }
}
