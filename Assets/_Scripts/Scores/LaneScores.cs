using System;
using TMPro;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class LaneScores : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private CurrentTask currentTask;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private TutorialDialogManager tutorialDialogManager;

    [Header("UI References - Safety")]
    [SerializeField] private TextMeshProUGUI currentSafety;
    [SerializeField] private TextMeshProUGUI reqSafety;
    [SerializeField] private Image safetyState;

    [Header("UI References - Charm")]
    [SerializeField] private TextMeshProUGUI currentCharm;
    [SerializeField] private TextMeshProUGUI reqCharm;
    [SerializeField] private Image charmState;

    [Header("UI References - Flow")]
    [SerializeField] private TextMeshProUGUI currentFlow;
    [SerializeField] private TextMeshProUGUI reqFlow;
    [SerializeField] private Image flowState;

    [Header("UI References - Material")]
    [SerializeField] private TextMeshProUGUI usedMaterial;
    [SerializeField] private TextMeshProUGUI minMaterial;
    [SerializeField] private TextMeshProUGUI maxMaterial;
    [SerializeField] private Image materialState;

    [Header("UI References - Safety")]
    [SerializeField] private Sprite check;
    [SerializeField] private Sprite caution;

    [Header("UI References - Flavor")]
    [SerializeField] private GameObject flavorFill;

    private String tutorialMinSafety, tutorialMinCharm, tutorialMinFlow, tutorialMinMaterial, tutorialMaxMaterial;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        taskManager.ActiveTaskScoresUpdated += HandleTaskLaneUpdated;
        taskManager.TaskSealed += ClearTaskScoresRequirements;
        taskManager.TaskSealed += ClearTaskStates;
        taskManager.TaskLaneConnected += UpdateTaskStates;

        currentTask.TaskPinned += UpdateTaskScoresRequirements;

        tutorialManager.ActiveTutorialScoresUpdated += HandleTutorialLaneUpdated;
        tutorialManager.TutorialStarted += UpdateTutorialScoresRequirements;
        tutorialDialogManager.ReadyToCompleteSection += UpdateTutorialScoresRequirements;
        tutorialManager.TutorialCompleted += ClearScoresRequirements;
        tutorialManager.TutorialCompleted += ClearStates;
        tutorialManager.TutorialLaneConnected += UpdateTutorialStates;
    }
    private void OnDisable()
    {
        taskManager.ActiveTaskScoresUpdated -= HandleTaskLaneUpdated;
        taskManager.TaskSealed -= ClearTaskScoresRequirements;
        taskManager.TaskSealed -= ClearTaskStates;
        taskManager.TaskLaneConnected -= UpdateTaskStates;

        currentTask.TaskPinned -= UpdateTaskScoresRequirements;

        tutorialManager.ActiveTutorialScoresUpdated -= HandleTutorialLaneUpdated;
        tutorialManager.TutorialStarted -= UpdateTutorialScoresRequirements;
        tutorialDialogManager.ReadyToCompleteSection -= UpdateTutorialScoresRequirements;
        tutorialManager.TutorialCompleted -= ClearScoresRequirements;
        tutorialManager.TutorialCompleted -= ClearStates;
        tutorialManager.TutorialLaneConnected -= UpdateTutorialStates;
    }

    private void Start() { ClearScoresRequirements(); }

    // :::::::::: TASK METHODS ::::::::::
    private void HandleTaskLaneUpdated(List<Vector2Int> _)
    {
        if (CurrentTask.Instance.ThereIsPinned())
        {
            Task task = CurrentTask.Instance.PinnedTask;

            if (task.info.safetyRequirement)
            {
                currentSafety.text = ConvertToUI(ScoreType.CurrentPercentage, task.currentSafety);
                if (task.currentSafety >= task.info.minSafety)
                {
                    safetyState.sprite = check;
                    safetyState.gameObject.SetActive(true);
                }
                else safetyState.gameObject.SetActive(false);
            }

            if (task.info.charmRequirement)
            {
                currentCharm.text = ConvertToUI(ScoreType.CurrentPercentage, task.currentCharm);
                if (task.currentCharm >= task.info.minCharm)
                {
                    charmState.sprite = check;
                    charmState.gameObject.SetActive(true);
                }
                else charmState.gameObject.SetActive(false);
            }

            if (task.info.flowRequirement)
            {
                currentFlow.text = ConvertToUI(ScoreType.CurrentPercentage, task.currentFlow);
                if (task.currentFlow >= task.info.minFlow)
                {
                    flowState.sprite = check;
                    flowState.gameObject.SetActive(true);
                }
                else flowState.gameObject.SetActive(false);
            }

            if (task.info.maxMaterialRequirement || task.info.minMaterialRequirement)
            {
                usedMaterial.text = ConvertToUI(ScoreType.CurrentAmount, task.usedMaterial);
                if (task.usedMaterial >= task.info.minMaterial || task.usedMaterial <= task.info.maxMaterial)
                {
                    materialState.sprite = check;
                    materialState.gameObject.SetActive(true);
                }
                else materialState.gameObject.SetActive(false);
            }

            if (task.flavorMet)
                flavorFill.SetActive(true);
            else flavorFill.SetActive(false);
        }
    }

    private void UpdateTaskStates(Task task)
    {
        if (task.info.safetyRequirement)
            if (task.currentSafety < task.info.minSafety)
            {
                safetyState.sprite = caution;
                safetyState.gameObject.SetActive(true);
            }

        if (task.info.charmRequirement)
            if (task.currentCharm < task.info.minCharm)
            {
                charmState.sprite = caution;
                charmState.gameObject.SetActive(true);
            }

        if (task.info.flowRequirement)
            if (task.currentFlow < task.info.minFlow)
            {
                flowState.sprite = caution;
                flowState.gameObject.SetActive(true);
            }

        if (task.info.minMaterialRequirement || task.info.maxMaterialRequirement)
            if (task.usedMaterial < task.info.minMaterial || task.usedMaterial > task.info.maxMaterial)
            {
                materialState.sprite = caution;
                materialState.gameObject.SetActive(true);
            }
    }

    private void ClearTaskStates(Task _) { ClearStates(); }

    private void UpdateTaskScoresRequirements(Task task)
    {
        if (task.info.safetyRequirement)
            reqSafety.text = ConvertToUI(ScoreType.RequirementPercentage, task.info.minSafety);

        if (task.info.charmRequirement)
            reqCharm.text = ConvertToUI(ScoreType.RequirementPercentage, task.info.minCharm);

        if (task.info.flowRequirement)
            reqFlow.text = ConvertToUI(ScoreType.RequirementPercentage, task.info.minFlow);

        if (task.info.minMaterialRequirement)
            maxMaterial.text = ConvertToUI(ScoreType.MinimumMaterial, task.info.minMaterial);

        if (task.info.maxMaterialRequirement)
            maxMaterial.text = ConvertToUI(ScoreType.MaximumMaterial, task.info.maxMaterial);
    }

    private void ClearTaskScoresRequirements(Task _) { ClearScoresRequirements(); }

    // :::::::::: TUTORIAL METHODS ::::::::::
    private void HandleTutorialLaneUpdated(Tutorial tutorial)
    {
        if (reqSafety.text != "-")
        {
            currentSafety.text = ConvertToUI(ScoreType.CurrentPercentage, tutorial.currentSafety);
            if (tutorial.currentSafety >= tutorial.info.minSafety)
            {
                safetyState.sprite = check;
                safetyState.gameObject.SetActive(true);
            }
            else safetyState.gameObject.SetActive(false);
        }

        if (reqCharm.text != "-")
        {
            currentCharm.text = ConvertToUI(ScoreType.CurrentPercentage, tutorial.currentCharm);
            if (tutorial.currentCharm >= tutorial.info.minCharm)
            {
                charmState.sprite = check;
                charmState.gameObject.SetActive(true);
            }
            else charmState.gameObject.SetActive(false);
        }

        if (reqFlow.text != "-")
        {
            currentFlow.text = ConvertToUI(ScoreType.CurrentPercentage, tutorial.currentFlow);
            if (tutorial.currentFlow >= tutorial.info.minFlow)
            {
                flowState.sprite = check;
                flowState.gameObject.SetActive(true);
            }
            else flowState.gameObject.SetActive(false);
        }

        if (maxMaterial.text != "-" || minMaterial.text != "-")
        {
            usedMaterial.text = ConvertToUI(ScoreType.MaximumMaterial, tutorial.usedMaterial);
            if (tutorial.usedMaterial >= tutorial.info.minMaterial || tutorial.usedMaterial <= tutorial.info.maxMaterial)
            {
                materialState.sprite = check;
                materialState.gameObject.SetActive(true);
            }
            else materialState.gameObject.SetActive(false);
        }
    }

    private void UpdateTutorialStates(Tutorial tutorial)
    {
        if (reqSafety.text != "-")
            if (tutorial.currentSafety < tutorial.info.minSafety)
            {
                safetyState.sprite = caution;
                safetyState.gameObject.SetActive(true);
            }

        if (reqCharm.text != "-")
            if (tutorial.currentCharm < tutorial.info.minCharm)
            {
                charmState.sprite = caution;
                charmState.gameObject.SetActive(true);
            }

        if (reqFlow.text != "-")
            if (tutorial.currentFlow < tutorial.info.minFlow)
            {
                flowState.sprite = caution;
                flowState.gameObject.SetActive(true);
            }

        if (maxMaterial.text != "-" || minMaterial.text != "-")
            if (tutorial.usedMaterial < tutorial.info.minMaterial || tutorial.usedMaterial > tutorial.info.maxMaterial)
            {
                materialState.sprite = caution;
                materialState.gameObject.SetActive(true);
            }
    }

    private void UpdateTutorialScoresRequirements(Tutorial tutorial)
    {
        tutorialMinSafety = tutorial.info.safetyRequirement
            ? ConvertToUI(ScoreType.RequirementPercentage, tutorial.info.minSafety) : "-";

        tutorialMinCharm = tutorial.info.charmRequirement
            ? ConvertToUI(ScoreType.RequirementPercentage, tutorial.info.minCharm) : "-";

        tutorialMinFlow = tutorial.info.flowRequirement
            ? ConvertToUI(ScoreType.RequirementPercentage, tutorial.info.minFlow) : "-";

        tutorialMinMaterial = tutorial.info.minMaterialRequirement
            ? ConvertToUI(ScoreType.MinimumMaterial, tutorial.info.minMaterial) : "-";

        tutorialMaxMaterial = tutorial.info.maxMaterialRequirement
            ? ConvertToUI(ScoreType.MaximumMaterial, tutorial.info.maxMaterial) : "-";
    }
    private void UpdateTutorialScoresRequirements(TutorialSection section)
    {
        if (section.checkRequirements)
        {
            reqSafety.text = tutorialMinSafety;
            reqCharm.text = tutorialMinCharm;
            reqFlow.text = tutorialMinFlow;
            minMaterial.text = tutorialMinMaterial;
            maxMaterial.text = tutorialMaxMaterial;
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public string ConvertToUI(ScoreType type, float? percentage = null, int? amount = null)
    {
        string convertion = string.Empty;

        switch (type)
        {
            case ScoreType.CurrentPercentage:
                convertion = ((int)(percentage * 100)).ToString();
                break;

            case ScoreType.CurrentAmount:
                convertion = amount.ToString();
                break;

            case ScoreType.RequirementPercentage:
                convertion = ((int)(percentage * 100)).ToString() + "%";
                break;

            case ScoreType.MinimumMaterial:
                convertion = ">" + amount.ToString();
                break;

            case ScoreType.MaximumMaterial:
                convertion = "<" + amount.ToString();
                break;
        }

        return convertion;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void ClearScoresRequirements()
    {
        currentSafety.text = "-";
        reqSafety.text = "-";

        currentCharm.text = "-";
        reqCharm.text = "-";

        currentFlow.text = "-";
        reqFlow.text = "-";

        usedMaterial.text = "-";
        minMaterial.text = "-";
        maxMaterial.text = "-";

        flavorFill.SetActive(false);
    }

    private void ClearStates()
    {
        safetyState.gameObject.SetActive(false);
        charmState.gameObject.SetActive(false);
        flowState.gameObject.SetActive(false);
        materialState.gameObject.SetActive(false);
    }
}

public enum ScoreType
{
    CurrentPercentage,
    CurrentAmount,
    RequirementPercentage,
    MinimumMaterial,
    MaximumMaterial,
}