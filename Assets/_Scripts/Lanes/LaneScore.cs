using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LaneScore : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private CurrentTask currentTask;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;
    [SerializeField] private TutorialManager tutorialManager;

    [Header("UI References - Safety")]
    [SerializeField] private TextMeshProUGUI currentSafety;
    [SerializeField] private TextMeshProUGUI reqSafety;

    [Header("UI References - Charm")]
    [SerializeField] private TextMeshProUGUI currentCharm;
    [SerializeField] private TextMeshProUGUI reqCharm;

    [Header("UI References - Flow")]
    [SerializeField] private TextMeshProUGUI currentFlow;
    [SerializeField] private TextMeshProUGUI reqFlow;

    private bool inTutorial = false;
    private String tutorialSafetyScore, tutorialCharmScore, tutorialFlowScore;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        laneConstructor.OnLaneBuilt += HandleLaneUpdated;
        laneDestructor.OnLaneDestroyed += HandleLaneUpdated;

        taskManager.TaskCompleted += ClearTaskScoresRequirements;
        currentTask.TaskPinned += UpdateTaskScoresRequirements;

        tutorialManager.TutorialStarted += UpdateTutorialScoresRequirements;
        tutorialManager.TutorialSectionStarted += UpdateTutorialScoresRequirements;
        tutorialManager.TutorialCompleted += ClearScoresRequirements;
    }
    private void OnDisable()
    {
        laneConstructor.OnLaneBuilt -= HandleLaneUpdated;
        laneDestructor.OnLaneDestroyed -= HandleLaneUpdated;

        taskManager.TaskCompleted -= ClearTaskScoresRequirements;
        currentTask.TaskPinned -= UpdateTaskScoresRequirements;

        tutorialManager.TutorialStarted -= UpdateTutorialScoresRequirements;
        tutorialManager.TutorialSectionStarted -= UpdateTutorialScoresRequirements;
        tutorialManager.TutorialCompleted -= ClearScoresRequirements;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Whenever a Lane is Built or Destroyed
    private void HandleLaneUpdated(Vector2Int gridPosition)
    {
        if (inTutorial)
        {
            if (reqSafety.text != "-") currentSafety.text = tutorialManager.currentSafety.ToString();
            if (reqCharm.text != "-") currentCharm.text = tutorialManager.currentSafety.ToString();
            if (reqFlow.text != "-") currentFlow.text = tutorialManager.currentSafety.ToString();
        }
        else
        {
            if (CurrentTask.Instance.ThereIsPinned())
            {
                Task task = CurrentTask.Instance.PinnedTask;

                if (task.info.safetyRequirement) currentSafety.text = task.currentSafetyCount.ToString();
                if (task.info.charmRequirement) currentCharm.text = task.currentCharmCount.ToString();
                if (task.info.flowRequirement) currentFlow.text = task.currentFlowPercentage.ToString();
            }
        }
    }

    // ::::: Tasks
    private void UpdateTaskScoresRequirements(Task task)
    {
        if (task.info.safetyRequirement) reqSafety.text = task.info.minSafetyCount.ToString();
        if (task.info.charmRequirement) reqCharm.text = task.info.minCharmCount.ToString();
        if (task.info.flowRequirement) reqFlow.text = task.info.minFlowPercentage.ToString();
    }
    private void ClearTaskScoresRequirements(Task task) { ClearScoresRequirements(); }

    // ::::: Tutorials
    private void UpdateTutorialScoresRequirements(TutorialData tutorial)
    {
        inTutorial = true;

        tutorialSafetyScore = tutorial.safetyRequirement ? tutorial.minSafetyCount.ToString() : "-";
        tutorialCharmScore = tutorial.charmRequirement ? tutorial.minCharmCount.ToString() : "-";
        tutorialFlowScore = tutorial.flowRequirement ? tutorial.minFlowPercentage.ToString() : "-";
    }
    private void UpdateTutorialScoresRequirements(TutorialSection section)
    {
        if (section.checkRequirements)
        {
            reqSafety.text = tutorialSafetyScore;
            reqCharm.text = tutorialCharmScore;
            reqFlow.text = tutorialFlowScore;
        }
    }

    private void ClearScoresRequirements()
    {
        inTutorial = false;

        currentSafety.text = "-";
        currentCharm.text = "-";
        currentFlow.text = "-";

        reqSafety.text = "-";
        reqCharm.text = "-";
        reqFlow.text = "-";
    }
}
