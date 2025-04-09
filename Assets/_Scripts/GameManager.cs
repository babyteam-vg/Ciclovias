using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int MapState { get; private set; } = -1;
    public int SmokeState { get; private set; } = -1;

    [Header("Dependencies")]
    [SerializeField] private Graph graph;
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private TaskDialogManager taskDialogManager;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private SplineManager splineManager;
    private StorageManager storageManager = new StorageManager();

    private bool pendingSave = false;

    public event Action<int> MapStateAdvanced;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        taskManager.TaskSealed += OnTaskSealed;
        taskDialogManager.DialogEnded += OnDialogEnded;
        tutorialManager.TutorialCompleted += OnTutorialCompleted;
    }
    private void OnDisable()
    {
        taskManager.TaskSealed -= OnTaskSealed;
        taskDialogManager.DialogEnded -= OnDialogEnded;
        tutorialManager.TutorialCompleted -= OnTutorialCompleted;
    }

    private void Start()
    {
        ApplyLoadedGameData(GameStateManager.Instance.LoadedGameData);
        StartCoroutine(DelayedInitialize());
    }

    private void LateUpdate()
    {
        if (pendingSave)
        {
            SaveGame();
            pendingSave = false;
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Map
    public void AdvanceMapState() {
        MapState++;
        Debug.Log($"MapState Advanced to {MapState}");
        MapStateAdvanced?.Invoke(MapState); // !
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private IEnumerator DelayedInitialize()
    {
        yield return new WaitForEndOfFrame();
        InitializeState();
    }
    // :::::
    private void InitializeState()
    {
        List<Task> unlockedTasks = TaskDiary.Instance.tasks.Where(t => t.state == TaskState.Unlocked).ToList();
        foreach (Task acceptedTask in unlockedTasks)
            acceptedTask.from.GetNextAvailableTask(MapState);

        if (MapState == -1)
        {
            AdvanceMapState();
            MaterialManager.Instance.AddMaterial(100);
        }

        if (CheckNewStateReached())
            AdvanceMapState();
    }

    // ::::: Advancing Map State Conditions
    private bool CheckNewStateReached()
    {
        List<Task> necessaryTasks = TaskDiary.Instance.tasks.Where(t => t.info.id.x == MapState).ToList();
        foreach (Task necessaryTask in necessaryTasks)
        {
            if (necessaryTask.state != TaskState.Sealed)
                return false;
        }
        return true;
    }

    // :::::::::: EVENT METHODS ::::::::::
    // ::::: Event Methods (Save Game)
    private void OnTaskSealed(Task task) { pendingSave = true; } // Auto Save
    private void OnTutorialCompleted() { pendingSave = true; } // Auto Save

    private void OnDialogEnded()
    {
        if (CheckNewStateReached())
        {
            AdvanceMapState();
            //ReduceSmokeState();
        }
    }

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Save
    private void SaveGame(string fileName = null, Action onComplete = null)
    {
        GameData gameData = new GameData
        {
            mapState = MapState,
            smokeState = SmokeState,
            materialAmount = MaterialManager.Instance.MaterialAmount,
            graph = graph.SaveGraph(),
            tasks = TaskDiary.Instance.SaveTasks(),
            tutorials = TutorialManager.Instance.SaveTutorials(),
            tips = TipManager.Instance.SaveTips(),
            splines = splineManager.SaveSplines(),
        };

        bool success = fileName == null
            ? storageManager.AutoSaveGame(gameData)
            : storageManager.ManualSaveGame(fileName, gameData);

        if (success)
        {
            Debug.Log("GAME SAVED");
            onComplete?.Invoke();
        }
        else Debug.LogError("ERROR SAVING");
    }

    // ::::: Load
    private void ApplyLoadedGameData(GameData gameData)
    {
        if (gameData != null)
        {
            MapState = gameData.mapState;
            SmokeState = gameData.smokeState;
            MaterialManager.Instance.LoadMaterial(gameData.materialAmount);
            graph.LoadGraph(gameData.graph);
            TaskDiary.Instance.LoadTasks(gameData.tasks);
            TutorialManager.Instance.LoadTutorials(gameData.tutorials);
            TipManager.Instance.LoadTips(gameData.tips);
            splineManager.LoadSplines(gameData.splines);

            Debug.Log("GAME LOADED");
        }
        else Debug.Log("NO GAME DATA");
    }
}