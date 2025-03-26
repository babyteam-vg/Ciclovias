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

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable() { taskManager.TaskSealed += StartTaskRewardDialogs; }
    private void OnDisable() { taskManager.TaskSealed -= StartTaskRewardDialogs; }

    // :::::::::: EXCLUSIVE METHODS ::::::::::
    public void StartTaskDialogs(Task task)
    {
        dialogs = task.info.dialogs;
        portrait.sprite = task.info.character.portrait;
        characterName.text = task.info.character.characterName;

        taskDialogUI.SetActive(true);
        StrictDialogOpened?.Invoke();
        StartDialog();
    }

    private void StartTaskRewardDialogs(Task task)
    {
        dialogs = task.info.rewardDialogs;
        portrait.sprite = task.info.character.portrait;
        characterName.text = task.info.character.characterName;

        taskDialogUI.SetActive(true);
        StartDialog();
    }

    // :::::::::: OERRIDE METHODS ::::::::::
    public override void EndDialog()
    {
        taskDialogUI.SetActive(false);
        StrictDialogClosed?.Invoke();

        if (TaskReceiver.Instance.ThereIsReceived())
            InGameMenuManager.Instance.OnReceiveTaskPress();

        base.EndDialog();
    }
}
