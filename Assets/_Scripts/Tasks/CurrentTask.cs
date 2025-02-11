using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTask : MonoBehaviour
{
    public static CurrentTask Instance { get; private set; }
    public Task PinnedTask { get; private set; }

    [SerializeField] private TaskWaypoint taskWaypoint;

    [Header("UI References - Texts")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI fromText;
    [SerializeField] private TextMeshProUGUI toText;

    [Header("UI References - Sliders")]
    [SerializeField] private Image safetyFill;
    [SerializeField] private Image charmFill;
    [SerializeField] private Image flowFill;

    [Header("Sliders")]
    [SerializeField] private GameObject safetySlider;
    [SerializeField] private GameObject charmSlider;
    [SerializeField] private GameObject flowSlider;

    private Animator safetyAnimator, charmAnimator, flowAnimator;
    private float prevSafety, prevCharm, prevFlow = 0f;

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

        safetyAnimator = safetySlider.GetComponent<Animator>();
        charmAnimator = charmSlider.GetComponent<Animator>();
        flowAnimator = flowSlider.GetComponent<Animator>();
    }

    private void Update()
    {
        if (ThereIsPinned())
        {
            Task task = PinnedTask;

            // Sliders
            // Safety
            float safetyUI = CurrentTask.Instance.PinnedTask.info.safetyRequirement ?
                (float)task.currentSafetyCount / (float)task.info.minSafetyCount : 0f;
            safetyFill.fillAmount = Mathf.Clamp(safetyUI, 0f, 1f);
            if (safetyUI != prevSafety)
            {
                if (safetyUI > prevSafety)
                    SliderAnimation(safetyAnimator, "SliderUp");
                else
                    SliderAnimation(safetyAnimator, "SliderDown");
                prevSafety = safetyUI;
            }

            // Charm
            float charmUI = CurrentTask.Instance.PinnedTask.info.charmRequirement ?
                (float)task.currentCharmCount / (float)task.info.minCharmCount : 0f;
            charmFill.fillAmount = Mathf.Clamp(charmUI, 0f, 1f);
            if (charmUI != prevCharm)
            {
                if (charmUI > prevCharm)
                    SliderAnimation(charmAnimator, "SliderUp");
                else
                    SliderAnimation(charmAnimator, "SliderDown");
                prevCharm = charmUI;
            }

            // Flow
            float flowUI = CurrentTask.Instance.PinnedTask.info.flowRequirement ?
                task.currentFlowPercentage / task.info.minFlowPercentage : 0f;
            flowFill.fillAmount = Mathf.Clamp(flowUI, 0f, 1f);
            if (flowUI != prevFlow)
            {
                if (flowUI > prevFlow)
                    SliderAnimation(flowAnimator, "SliderUp");
                else
                    SliderAnimation(flowAnimator, "SliderDown");
                prevFlow = flowUI;
            }
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Is There a Task Pinned?
    public bool ThereIsPinned() { return PinnedTask != null; }

    // ::::: Pin & Unpin a Task
    public void PinTask(Task task)
    {
        if (PinnedTask == task) return;

        PinnedTask = task;
        UpdateTaskUI();
        taskWaypoint.UpdateTaskWaypoints(PinnedTask.fromCompound.transform, PinnedTask.toCompound.transform);
    }
    public void UnpinTask(Task task)
    {
        if (!ThereIsPinned()) return;

        PinnedTask = null;
        UpdateTaskUI();
        taskWaypoint.HideTaskWaypoints();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        // Texts
        if (ThereIsPinned())
        {
            titleText.text = PinnedTask.info.title;
            fromText.text = PinnedTask.info.from.compoundName;
            toText.text = PinnedTask.info.to.compoundName;
        }
        else
        {
            titleText.text = "Task";
            fromText.text = "From";
            toText.text = "To";
            safetyFill.fillAmount = 0f;
            charmFill.fillAmount = 0f;
            flowFill.fillAmount = 0f;
        }
    }

    public void SliderAnimation(Animator animator, String animation)
    {
        animator.Play(animation);
    }
}
