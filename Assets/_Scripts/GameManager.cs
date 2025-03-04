using System;
using System.Collections;
using System.Collections.Generic;
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
    private StorageManager _storageManager = new StorageManager();

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
        amountText.text = "x" + MaterialAmount.ToString();
        animator = materialCounter.GetComponent<Animator>();

        if (!CurrentTask.Instance.ThereIsPinned() && MapState == 0)
        {
            Task firstTask = TaskDiary.Instance.tasks[0];
            if (firstTask.state == TaskState.Unlocked)
                firstTask.from.GetNextAvailableTask(MapState);
        }
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
        SaveGame();
    }

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Save
    public void SaveGame()
    {
        GameData gameData = new GameData
        {
            graphData = graph.SaveGraphData(),
        };

        bool success = _storageManager.SaveGame(gameData);

        if (success) Debug.Log("Game Successfully Saved!");
        else Debug.LogError("Error Saving the Game :(");
    }

    // ::::: Load
    private void ApplyLoadedGameData(GameData gameData)
    {
        graph.LoadGraphData(gameData.graphData);

        Debug.Log("Partida cargada correctamente.");
    }
}
