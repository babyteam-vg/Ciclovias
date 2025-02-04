using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTask : MonoBehaviour
{
    public static CurrentTask Instance { get; private set; }

    public Task PinnedTask { get; private set; }
    //public event Action<Task> OnTaskPinned;

    [SerializeField] private TextMeshProUGUI titleText;

    [SerializeField] private Slider safetySlider;
    [SerializeField] private Slider charmSlider;
    [SerializeField] private Slider flowSlider;

    // Methods

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
            float safetyUI = 1f - (float)task.currentSafety / (float)task.info.maxSafetyDiscount;
            safetySlider.value = Mathf.Clamp(safetyUI, 0f, 1f);

            float charmUI = (float)task.currentCharm / (float)task.info.minCharm;
            charmSlider.value = Mathf.Clamp(charmUI, 0f, 1f);

            float flowUI = task.currentFlow / task.info.minFlowPercentage;
            flowSlider.value = flowUI;
        }
    }

    public void PinTask(Task task)
    {
        if (PinnedTask == task) return;

        PinnedTask = task;
        UpdateTaskUI();
    }

    private void UpdateTaskUI()
    {
        Task task = PinnedTask;

        // Text
        titleText.text = task.info.title;
        // startText.text
        // destinationText.text
    }
}
