using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTask : MonoBehaviour
{
    public static CurrentTask Instance { get; private set; }
    public Task PinnedTask { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;

    [Header("UI References - Texts")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI fromText;
    [SerializeField] private TextMeshProUGUI toText;

    [Header("UI References - Scores")]
    [SerializeField] private Image safetyFill;
    [SerializeField] private Image charmFill;
    [SerializeField] private RectTransform flowBackground;
    [SerializeField] private RectTransform flowMark;

    //[Header("Sliders")]
    //[SerializeField] private GameObject safetySlider;
    //[SerializeField] private GameObject charmSlider;
    //[SerializeField] private GameObject flowSlider;

    private Animator safetyAnimator, charmAnimator, flowAnimator;
    private float prevSafety, prevCharm, prevFlow = 0f;

    public event Action<Task> TaskPinned;
    public event Action TaskUnpinned;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void OnEnable()
    {
        taskManager.TaskAccepted += PinTask;
        taskManager.TaskActivated += PinTask;
        taskManager.TaskCompleted += UnpinTask;
    }
    private void OnDisable()
    {
        taskManager.TaskAccepted -= PinTask;
        taskManager.TaskActivated -= PinTask;
        taskManager.TaskCompleted -= UnpinTask;
    }

    private void Start()
    {
        //safetyAnimator = safetySlider.GetComponent<Animator>();
        //charmAnimator = charmSlider.GetComponent<Animator>();
        //flowAnimator = flowSlider.GetComponent<Animator>();
    }

    private void Update() { if (ThereIsPinned()) UpdateScoreUI(); }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Is There a Task Pinned?
    public bool ThereIsPinned() { return PinnedTask != null; }

    // ::::: Pin & Unpin a Task
    public void PinTask(Task task, bool isManualPin)
    {
        if (PinnedTask == task) return;
        if (!isManualPin && ThereIsPinned()) return;

        PinnedTask = task;
        TaskPinned?.Invoke(task);
        UpdateTaskUI();
    }
    public void UnpinTask(Task task)
    {
        if (!ThereIsPinned()) return;

        PinnedTask = null;
        TaskUnpinned?.Invoke();
        UpdateTaskUI();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: 
    private void UpdateScoreUI()
    {
        Task task = PinnedTask;

        // Sliders
        // Safety
        float safetyUI = task.info.safetyRequirement
            ? (float)task.currentSafetyCount / (float)task.info.minSafetyCount
            : 0f;
        safetyFill.fillAmount = Mathf.Clamp(safetyUI, 0f, 1f);
        if (safetyUI != prevSafety)
        {
            //if (safetyUI > prevSafety)
            //    SliderAnimation(safetyAnimator, "SliderUp");
            //else
            //    SliderAnimation(safetyAnimator, "SliderDown");
            prevSafety = safetyUI;
        }

        // Charm
        float charmUI = task.info.charmRequirement
            ? (float)task.currentCharmCount / (float)task.info.minCharmCount
            : 0f;
        charmFill.fillAmount = Mathf.Clamp(charmUI, 0f, 1f);
        if (charmUI != prevCharm)
        {
            //if (charmUI > prevCharm)
            //    SliderAnimation(charmAnimator, "SliderUp");
            //else
            //    SliderAnimation(charmAnimator, "SliderDown");
            prevCharm = charmUI;
        }

        // Flow
        float flowUI = task.info.flowRequirement
            ? task.currentFlowPercentage
            : 0f;
        flowUI = Mathf.Clamp(flowUI, 0f, 1f);

        if (flowUI != prevFlow)
        {
            prevFlow = flowUI;
            float newX = flowUI * flowBackground.rect.width;
            flowMark.anchoredPosition = new Vector2(newX, flowMark.anchoredPosition.y);
        }
    }

    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        // Texts
        if (ThereIsPinned())
        {
            titleText.text = PinnedTask.info.title;
            fromText.text = PinnedTask.from.info.compoundName;
            toText.text = PinnedTask.to.info.compoundName;
        }
        else
        {
            titleText.text = "Task";
            fromText.text = "From";
            toText.text = "To";
            safetyFill.fillAmount = 0f;
            charmFill.fillAmount = 0f;
            flowMark.anchoredPosition = new Vector2(0f, flowMark.anchoredPosition.y);
        }
    }

    //public void SliderAnimation(Animator animator, String animation)
    //{
    //    animator.Play(animation);
    //}
}
