using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LaneScores : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;
    [SerializeField] private CurrentTask currentTask;
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

    [Header("UI References - Material")]
    //[SerializeField] private TextMeshProUGUI minMaterial;
    [SerializeField] private TextMeshProUGUI usedMaterial;
    [SerializeField] private TextMeshProUGUI maxMaterial;

    [Header("UI References - Flavor")]
    [SerializeField] private GameObject flavorFill;

    private String tutorialSafetyScore, tutorialCharmScore, tutorialFlowScore, tutorialUsedMaterial;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        taskManager.ActiveTaskScoresUpdated += HandleTaskLaneUpdated;
        taskManager.TaskSealed += ClearTaskScoresRequirements;

        currentTask.TaskPinned += UpdateTaskScoresRequirements;

        tutorialManager.ActiveTutorialScoresUpdated += HandleTutorialLaneUpdated;
        tutorialManager.TutorialStarted += UpdateTutorialScoresRequirements;
        tutorialManager.TutorialSectionStarted += UpdateTutorialScoresRequirements;
        tutorialManager.TutorialCompleted += ClearScoresRequirements;
    }
    private void OnDisable()
    {
        taskManager.ActiveTaskScoresUpdated -= HandleTaskLaneUpdated;
        taskManager.TaskSealed -= ClearTaskScoresRequirements;

        currentTask.TaskPinned -= UpdateTaskScoresRequirements;

        tutorialManager.TutorialStarted -= UpdateTutorialScoresRequirements;
        tutorialManager.TutorialSectionStarted -= UpdateTutorialScoresRequirements;
        tutorialManager.TutorialCompleted -= ClearScoresRequirements;
    }

    // :::::::::: TASK METHODS ::::::::::
    private void HandleTaskLaneUpdated()
    {
        if (CurrentTask.Instance.ThereIsPinned())
        {
            Task task = CurrentTask.Instance.PinnedTask;

            if (task.info.safetyRequirement) currentSafety.text = ((int)(task.currentSafety * 100)).ToString();
            if (task.info.charmRequirement) currentCharm.text = ((int)(task.currentCharm * 100)).ToString();
            if (task.info.flowRequirement) currentFlow.text = ((int)(task.currentFlow * 100)).ToString();
            
            if (task.info.maxMaterialRequirement || task.info.minMaterialRequirement) usedMaterial.text = task.usedMaterial.ToString();
            
            if (task.flavorMet) flavorFill.SetActive(true);
            else flavorFill.SetActive(false);
        }
    }

    private void UpdateTaskScoresRequirements(Task task)
    {
        if (task.info.safetyRequirement) reqSafety.text = ((int)(task.info.minSafety * 100)).ToString();
        if (task.info.charmRequirement) reqCharm.text = ((int)(task.info.minCharm * 100)).ToString();
        if (task.info.flowRequirement) reqFlow.text = ((int)(task.info.minFlow * 100)).ToString();
        if (task.info.maxMaterialRequirement) maxMaterial.text = task.info.maxMaterial.ToString();
    }

    private void ClearTaskScoresRequirements(Task task) { ClearScoresRequirements(); }

    // :::::::::: TUTORIAL METHODS ::::::::::
    private void HandleTutorialLaneUpdated(Tutorial tutorial)
    {
        if (reqSafety.text != "-") currentSafety.text = ((int)(tutorial.currentSafety * 100)).ToString();
        if (reqCharm.text != "-") currentCharm.text = ((int)(tutorial.currentCharm * 100)).ToString();
        if (reqFlow.text != "-") currentFlow.text = ((int)(tutorial.currentFlow * 100)).ToString();
        if (maxMaterial.text != "-") usedMaterial.text = tutorial.usedMaterial.ToString();
    }

    private void UpdateTutorialScoresRequirements(Tutorial tutorial)
    {
        tutorialSafetyScore = tutorial.info.safetyRequirement ? ((int)(tutorial.info.minSafety * 100)).ToString() : "-";
        tutorialCharmScore = tutorial.info.charmRequirement ? ((int)(tutorial.info.minCharm * 100)).ToString() : "-";
        tutorialFlowScore = tutorial.info.flowRequirement ? ((int)(tutorial.info.minFlow * 100)).ToString() : "-";
        tutorialUsedMaterial = tutorial.info.maxMaterialRequirement ? tutorial.info.maxMaterial.ToString() : "-";
    }
    private void UpdateTutorialScoresRequirements(TutorialSection section)
    {
        if (section.checkRequirements)
        {
            reqSafety.text = tutorialSafetyScore;
            reqCharm.text = tutorialCharmScore;
            reqFlow.text = tutorialFlowScore;
            maxMaterial.text = tutorialUsedMaterial;
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void ClearScoresRequirements()
    {
        currentSafety.text = "-";
        currentCharm.text = "-";
        currentFlow.text = "-";
        usedMaterial.text = "-";

        reqSafety.text = "-";
        reqCharm.text = "-";
        reqFlow.text = "-";
        maxMaterial.text = "-";

        flavorFill.SetActive(false);
    }
}
