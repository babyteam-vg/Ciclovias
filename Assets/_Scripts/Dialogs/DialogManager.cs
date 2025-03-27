using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public abstract class DialogManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private InGameMenuManager inGameMenuManager;

    [Header("UI References")]
    [SerializeField] protected Image portrait;
    [SerializeField] protected TextMeshProUGUI characterName;
    [SerializeField] protected TextMeshProUGUI currentDialog;
    [SerializeField] protected Image next;

    protected List<string> dialogs = new List<string>();

    protected bool isAllowed = true;
    protected bool inDialog = false;
    protected bool isTyping = false;
    protected bool tutorialAllowsIt = true;
    protected int currentIndex = 0;
    protected float textSpeed = 0.025f;
    protected Coroutine typingCoroutine;

    public event Action FinishedTyping;

    // :::::::::: MONO METHODS ::::::::::
    protected virtual void OnEnable()
    {
        inGameMenuManager.MenuOpened += LockDialog;
        inGameMenuManager.MenuClosed += UnlockDialog;
    }
    protected virtual void OnDisable()
    {
        inGameMenuManager.MenuOpened -= LockDialog;
        inGameMenuManager.MenuClosed -= UnlockDialog;
    }

    protected virtual void Update()
    {
        if (isAllowed)
        {
            if (Input.GetMouseButtonDown(0)
            && inDialog && tutorialAllowsIt) // Manage Player Clicks
            {
                if (isTyping) CompleteTyping();
                else NextDialog();
            }
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public virtual void StartDialog()
    {
        if (inDialog) return;

        currentIndex = 0;
        inDialog = true;
        ShowNextDialog();
    }

    public virtual void NextDialog()
    {
        currentIndex++;
        if (currentIndex < dialogs.Count)
            ShowNextDialog();
        else EndDialog();
    }

    public virtual void EndDialog() { inDialog = false; }

    // :::::::::: PRIVATE METHODS ::::::::::
    protected virtual void ShowNextDialog()
    {
        currentDialog.text = "";
        typingCoroutine = StartCoroutine(TypeDialog(dialogs[currentIndex]));
    }

    protected IEnumerator TypeDialog(string dialog)
    {
        isTyping = true;
        currentDialog.text = "";

        int i = 0;
        while (i < dialog.Length)
        {
            if (dialog[i] == '<')
            {
                int endIndex = dialog.IndexOf('>', i);
                if (endIndex != -1)
                {
                    currentDialog.text += dialog.Substring(i, endIndex - i + 1);
                    i = endIndex + 1;
                    continue;
                }
            }

            currentDialog.text += dialog[i];
            i++;
            yield return new WaitForSeconds(textSpeed);
        }

        FinishedTyping?.Invoke(); // !
        isTyping = false;
    }

    protected void CompleteTyping()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
            typingCoroutine = null;
        }

        currentDialog.text = dialogs[currentIndex];
        FinishedTyping?.Invoke(); // !
        isTyping = false;
    }

    protected void LockDialog() { isAllowed = false; }
    protected void UnlockDialog() { isAllowed = true; }
}
