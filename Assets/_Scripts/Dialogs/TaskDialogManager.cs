using System;
using System.Collections.Generic;
using UnityEngine;

public class TaskDialogManager : DialogManager
{
    [Header("Tasks")]
    [SerializeField] private TaskManager taskManager;
    public GameObject taskDialogUI;

    public event Action StrictDialogOpened;
    public event Action StrictDialogClosed;
    public event Action DialogEnded;

    // :::::::::: MONO METHODS ::::::::::
    protected override void OnEnable()
    {
        taskManager.TaskSealed += StartTaskRewardDialogs;
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        taskManager.TaskSealed -= StartTaskRewardDialogs;
        base.OnDisable();
    }

    // :::::::::: EXCLUSIVE METHODS ::::::::::
    public void StartTaskDialogs(Task task)
    {
        dialogs = task.info.dialogs;
        portrait.sprite = task.info.character.portrait;
        characterName.text = task.info.character.characterName;

        StrictDialogOpened?.Invoke();
        taskDialogUI.SetActive(true);
        
        StartDialog();
    }

    private void StartTaskRewardDialogs(Task task)
    {
        dialogs = task.info.rewardDialogs;
        if (task.info.flavorDetails.flavorType != FlavorType.None && task.flavorMet)
            dialogs.Insert(task.info.extraDialogIndex, task.info.extraDialog);

        portrait.sprite = task.info.character.portrait;
        characterName.text = task.info.character.characterName;

        taskDialogUI.SetActive(true);
        StrictDialogOpened?.Invoke();
        StartDialog();
    }

    // :::::::::: OERRIDE METHODS ::::::::::
    public override void EndDialog()
    {
        taskDialogUI.SetActive(false);
        StrictDialogClosed?.Invoke();

        if (TaskReceiver.Instance.ThereIsReceived())
            InGameMenuManager.Instance.OnReceiveTaskPress();

        DialogEnded?.Invoke();
        base.EndDialog();
    }
}
