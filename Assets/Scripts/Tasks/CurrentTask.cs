using System;
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

    private void Update()
    {
        if (PinnedTask != null)
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
    // ::::: Pin a Task
    public void PinTask(Task task)
    {
        if (PinnedTask == task) return;

        PinnedTask = task;
        UpdateTaskUI();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        Task task = PinnedTask;

        // Texts
        titleText.text = task.info.title;
        startText.text = task.info.from.name;
        destinationText.text = task.info.to.name;
    }
}
