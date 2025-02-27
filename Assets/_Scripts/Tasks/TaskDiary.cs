using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskDiary : MonoBehaviour
{
    public static TaskDiary Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;
    [SerializeField] private TaskManager taskManager;

    [Header("UI References")]
    [SerializeField] private Transform contentTransform;
    [SerializeField] private GameObject acceptedTaskPrefab;

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
                GameObject newItem = Instantiate(acceptedTaskPrefab, contentTransform);
                newItem.gameObject.SetActive(true);

                // Find Each Element
                TextMeshProUGUI taskTitle = newItem.transform.Find("Header/Task Title").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI taskFrom = newItem.transform.Find("Body/From/Compound Name").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI taskTo = newItem.transform.Find("Body/To/Compound Name").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI requirementsSafety = newItem.transform.Find("Body/Requirements/Safety/Text (TMP)").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI requirementsCharm = newItem.transform.Find("Body/Requirements/Charm/Text (TMP)").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI requirementsFlow = newItem.transform.Find("Body/Requirements/Flow/Text (TMP)").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI minimumMaterial = newItem.transform.Find("Body/Requirements/Material/Minimum").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI maximumMaterial = newItem.transform.Find("Body/Requirements/Material/Maximum").GetComponent<TextMeshProUGUI>();
                Button pinButton = newItem.transform.Find("Body/Button").GetComponent<Button>();

                // Set the Values
                taskTitle.text = task.info.title;
                taskFrom.text = task.from.info.compoundName;
                taskTo.text = task.to.info.compoundName;

                requirementsSafety.text = task.info.minSafetyCount.ToString();
                requirementsCharm.text = task.info.minCharmCount.ToString();
                requirementsFlow.text = task.info.minFlowPercentage.ToString() + "%";
                minimumMaterial.text = "Min. " + task.info.minMaterial.ToString();
                maximumMaterial.text = "Max. " + task.info.maxMaterial.ToString();

                pinButton.onClick.AddListener(() => {
                    CurrentTask.Instance.PinTask(task, true);
                });

                //taskCharacter.text = task.info.character.characterName;
                //taskDialog.text = task.info.dialog;
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
}
