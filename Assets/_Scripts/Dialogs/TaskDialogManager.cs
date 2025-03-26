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
        if (inDialog) return;

        StrictDialogOpened?.Invoke();
        //StartDialog(task.info.dialogs, task.info.character.portrait, task.info.character.characterName);
    }

    private void StartTaskRewardDialogs(Task task)
    {
        List<string> dialogs = task.info.dialogs;
        Sprite portrait = task.info.character.portrait;
        string name = task.info.character.characterName;

        taskDialogUI.SetActive(true);
        //StartDialog(dialogs, portrait, name);
    }

    // :::::::::: OERRIDE METHODS ::::::::::
    public override void EndDialog()
    {
        taskDialogUI.SetActive(false);
        StrictDialogClosed?.Invoke();
        base.EndDialog();
    }
}
