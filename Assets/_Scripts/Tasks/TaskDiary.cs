using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TaskDiary : MonoBehaviour
{
    public static TaskDiary Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;
    [SerializeField] private LaneScores laneScores;
    [SerializeField] private TaskManager taskManager;

    [Header("UI References")]
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject taskPrefab;

    public List<Task> tasks = new List<Task>();

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        laneConstructor.OnLaneBuilt += HandleLaneUpdated;
        laneDestructor.OnLaneDestroyed += HandleLaneUpdated;
    }
    private void OnDisable()
    {
        laneConstructor.OnLaneBuilt -= HandleLaneUpdated;
        laneDestructor.OnLaneDestroyed -= HandleLaneUpdated;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Tasks Diary UI
    public void ShowAvailableTasks()
    {
        foreach (Transform child in contentTransform)
            Destroy(child.gameObject);

        foreach (Task task in tasks)
        { //       Accepted <¬          Active <¬
            if (task.state == TaskState.Accepted || task.state == TaskState.Active)
            {
                GameObject newItem = Instantiate(taskPrefab, contentTransform);
                newItem.gameObject.SetActive(true);

                // Info
                TextMeshProUGUI taskTitle = newItem.transform.Find("Info/Title TMP").GetComponent<TextMeshProUGUI>();
                Image portrait = newItem.transform.Find("Info/Portrait").GetComponent<Image>();
                TextMeshProUGUI characterName = newItem.transform.Find("Info/Character TMP").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI context = newItem.transform.Find("Info/Context TMP").GetComponent<TextMeshProUGUI>();

                // Requirements
                TextMeshProUGUI requirementsSafety = newItem.transform.Find("Requirements/Safety/Safety TMP").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI requirementsCharm = newItem.transform.Find("Requirements/Charm/Charm TMP").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI requirementsFlow = newItem.transform.Find("Requirements/Flow/Flow TMP").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI minimumMaterial = newItem.transform.Find("Requirements/Material/Minimum TMP").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI maximumMaterial = newItem.transform.Find("Requirements/Material/Maximum TMP").GetComponent<TextMeshProUGUI>();

                Button pinButton = newItem.transform.Find("Pin").GetComponent<Button>();

                // Set the Values
                taskTitle.text = task.info.title;
                portrait.sprite = task.info.character.portrait;
                characterName.text = task.info.character.characterName;
                context.text = task.info.context;

                requirementsSafety.text = task.info.safetyRequirement
                    ? laneScores.ConvertToUI(ScoreType.RequirementPercentage, task.info.minSafety) : "-";
                requirementsCharm.text = task.info.charmRequirement
                    ? laneScores.ConvertToUI(ScoreType.RequirementPercentage, task.info.minCharm) : "-";
                requirementsFlow.text = task.info.flowRequirement
                    ? laneScores.ConvertToUI(ScoreType.RequirementPercentage, task.info.minFlow) : "-";
                minimumMaterial.text = task.info.minMaterialRequirement
                    ? laneScores.ConvertToUI(ScoreType.MinimumMaterial, task.info.minMaterial) : "-";
                maximumMaterial.text = task.info.maxMaterialRequirement
                    ? laneScores.ConvertToUI(ScoreType.MaximumMaterial, task.info.maxMaterial) : "-";

                pinButton.onClick.AddListener(() => {
                    CurrentTask.Instance.PinTask(task, true);
                    UpdatePinButtons();
                });
                UpdatePinButton(pinButton, task);
            }
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: When a Lane is Built or Destroyed
    private void HandleLaneUpdated(Vector2Int newNode)
    {
        taskManager.UpdateActiveTasks(tasks);
        taskManager.TaskInProgress(newNode);
    }

    // ::::: Pin Tasks in the Diary
    private void UpdatePinButton(Button pinButton, Task task)
    {
        pinButton.enabled = CurrentTask.Instance.PinnedTask != task;
    }
    private void UpdatePinButtons()
    {
        foreach (Transform child in contentTransform)
        {
            Button pinButton = child.Find("Pin").GetComponent<Button>();
            Task task = tasks.Find(t => t.info.title == child.Find("Info/Title TMP").GetComponent<TextMeshProUGUI>().text);

            if (task != null)
                UpdatePinButton(pinButton, task);
        }
    }

    // :::::::::: STORAGE METHODS ::::::::::
    // ::::: Tasks -> TasksData
    public List<TaskData> SaveTasks()
    {
        List<TaskData> tasksData = new List<TaskData>();
        foreach (Task task in tasks)
            tasksData.Add(task.SaveTask());
        return tasksData;
    }

    // ::::: TasksData -> Tasks
    public void LoadTasks(List<TaskData> tasksData)
    {
        if (tasksData.Count != tasks.Count) return;

        for (int i = 0; i < tasks.Count; i++)
        {
            Task task = tasks[i];
            TaskData taskData = tasksData[i];

            task.state = taskData.state;

            task.path = taskData.path;

            task.currentSafety = taskData.currentSafety;
            task.currentCharm = taskData.currentCharm;
            task.currentFlow = taskData.currentFlow;
            task.usedMaterial = taskData.usedMaterial;

            task.currentToCross = taskData.currentToCross;
        }
    }
}
