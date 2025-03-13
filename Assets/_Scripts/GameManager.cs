using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int MapState { get; private set; } = -1;
    public int SmokeState { get; private set; } = -1;
    public int MaterialAmount { get; private set; } = -1;

    [Header("Dependencies")]
    [SerializeField] private Graph graph;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private SplinesManager splinesManager;
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
    }

    private void OnEnable()
    {
        taskManager.TaskSealed += OnTaskSealed;
    }
    private void OnDisable()
    {
        taskManager.TaskSealed -= OnTaskSealed;
    }

    private void Start()
    {
        ApplyLoadedGameData(GameStateManager.Instance.LoadedGameData);

        amountText.text = "x" + MaterialAmount.ToString();
        animator = materialCounter.GetComponent<Animator>();

        StartCoroutine(DelayedInitialize());
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
        SmokeState++;
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
    private IEnumerator DelayedInitialize()
    {
        yield return new WaitForEndOfFrame();
        InitializeState();
    }

    private void InitializeState()
    {
        List<Task> unlockedTasks = TaskDiary.Instance.tasks.Where(t => t.state == TaskState.Unlocked).ToList();
        foreach (Task acceptedTask in unlockedTasks)
            acceptedTask.from.GetNextAvailableTask(MapState);

        if (MapState == -1) AdvanceMapState();
        if (SmokeState == -1) ReduceSmokeState();
        if (MaterialAmount == -1) AddMaterial(120);
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
    private void SaveGame(string fileName = null)
    {
        GameData gameData = new GameData
        {
            MapState = MapState,
            SmokeState = SmokeState,
            MaterialAmount = MaterialAmount,
            graph = graph.SaveGraph(),
            tasks = TaskDiary.Instance.SaveTasks(),
            tutorials = TutorialManager.Instance.SaveTutorials(),
            splines = splinesManager.SaveSplines(),
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
            TaskDiary.Instance.LoadTasks(gameData.tasks);
            TutorialManager.Instance.LoadTutorials(gameData.tutorials);
            splinesManager.LoadSplines(gameData.splines);

            amountText.text = "x" + MaterialAmount.ToString();

            Debug.Log("GAME LOADED");
        }
        else Debug.Log("NO GAME DATA");
    }
}
