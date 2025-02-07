using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTask : MonoBehaviour
{
    public static CurrentTask Instance { get; private set; }
    public Task PinnedTask { get; private set; }

    [Header("UI References - Texts")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI startText;
    [SerializeField] private TextMeshProUGUI destinationText;

    [Header("UI References - Sliders")]
    [SerializeField] private Slider safetySlider;
    [SerializeField] private Slider charmSlider;
    [SerializeField] private Slider flowSlider;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (!ThereIsPinned() && GameStateManager.Instance.CurrentMapState == 0)
        {
            Task firstTask = TaskDiary.Instance.tasks[0];
            if (firstTask.state == 2)
                PinTask(firstTask); // Automatically Pin Task 0-1 "Tutorial"
        }
    }

    private void Update()
    {
        if (ThereIsPinned())
        {
            Task task = PinnedTask;

            // Sliders
            float safetyUI = 1f - (float)task.currentSafetyDiscount / (float)task.info.maxSafetyDiscount;
            safetySlider.value = Mathf.Clamp(safetyUI, 0f, 1f);

            float charmUI = (float)task.currentCharmCount / (float)task.info.minCharmCount;
            charmSlider.value = Mathf.Clamp(charmUI, 0f, 1f);

            float flowUI = task.currentFlowPercentage / task.info.minFlowPercentage;
            flowSlider.value = flowUI;
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Is There a Task Pinned?
    public bool ThereIsPinned() { return PinnedTask != null; }

    // ::::: Pin a Task
    public void PinTask(Task task)
    {
        if (PinnedTask == task) return;

        PinnedTask = task;
        UpdateTaskUI();
    }
    public void UnpinTask(Task task)
    {
        if (!ThereIsPinned()) return;

        PinnedTask = null;
        UpdateTaskUI();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        // Texts
        if (ThereIsPinned())
        {
            titleText.text = PinnedTask.info.title;
            startText.text = PinnedTask.info.from.compoundName;
            destinationText.text = PinnedTask.info.to.compoundName;
        }
        else
        {
            titleText.text = "Task";
            startText.text = "From";
            destinationText.text = "To";
        }
    }
}
