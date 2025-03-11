using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private TaskManager taskManager;

    [Header("Content")]
    public Image portrait;
    public TextMeshProUGUI characterName;
    public TextMeshProUGUI dialog;
    public string[] dialogs;
    public float textSpeed = 0.05f;

    private int index;
    private bool inDialog = false;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable() { taskManager.TaskSealed += StartDialog; }
    private void OnDisable() { taskManager.TaskSealed -= StartDialog; }

    private void Start() { dialog.text = string.Empty; }

    private void Update()
    {
        if (inDialog && Input.GetMouseButtonDown(0))
            if (dialog.text == dialogs[index])
                NextDialog();
            else
            {
                //StopAllCoroutines();
                dialog.text = dialogs[index];
            }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void StartDialog(Task task)
    {
        index = 0;

        dialogs = task.state == TaskState.Sealed
            ? task.info.rewardDialogs
            : task.info.dialogs;
        portrait.sprite = task.info.character.portrait;
        characterName.text = task.info.character.characterName;

        dialog.text = dialogs[0];
        inDialog = true;
        //StartCoroutine(TypeDialog());
    }

    public void NextDialog()
    {
        if (index < dialogs.Length - 1)
        {
            index++;
            dialog.text = dialogs[index];
            //dialog.text = string.Empty;
            //StartCoroutine(TypeDialog());
        }
        else // End Dialog
        {
            inDialog = false;
            dialog.text = string.Empty;
            dialogs = new string[0];

            InGameMenuManager.Instance.OnCloseDialog();

            if (TaskReceiver.Instance.ThereIsReceived())
                InGameMenuManager.Instance.OnReceiveTaskPress();
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    IEnumerator TypeDialog()
    {
        foreach (char c in dialogs[index].ToCharArray())
        {
            dialog.text += c;
            yield return new WaitForSeconds(textSpeed);
        }
    }
}
