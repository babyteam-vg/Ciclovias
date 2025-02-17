using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogManager : MonoBehaviour
{
    public static DialogManager Instance { get; private set; }

    public Image portrait;
    public TextMeshProUGUI dialog;
    public string[] dialogs;
    public float textSpeed = 0.05f;

    private int index;
    private bool inDialog = false;

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
        dialog.text = string.Empty;
    }

    private void Update()
    {
        if (inDialog && Input.GetMouseButtonDown(0))
            if (dialog.text == dialogs[index])
                NextDialog();
            else
            {
                StopAllCoroutines();
                dialog.text = dialogs[index];
            }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void StartDialog()
    {
        portrait.sprite = TaskReceiver.Instance.ReceivedTask.info.character.portrait;
        inDialog = true;
        index = 0;
        dialogs = TaskReceiver.Instance.ReceivedTask.info.dialogs;
        StartCoroutine(TypeDialog());
    }

    public void NextDialog()
    {
        if (index < dialogs.Length - 1)
        {
            index++;
            dialog.text = string.Empty;
            StartCoroutine(TypeDialog());
        }
        else // End Dialog
        {
            inDialog = false;
            MenuManager.Instance.OnCloseDialog();
            MenuManager.Instance.OnReceiveTaskPress();
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
