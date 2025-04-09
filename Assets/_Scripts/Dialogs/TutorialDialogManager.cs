using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialDialogManager : DialogManager
{
    [Header("Tutorials")]
    [SerializeField] private TutorialManager tutorialManager;
    public GameObject tutorialDialogUI;

    private TutorialSection currentSection;

    public event Action<TutorialSection> ReadyToCompleteSection;
    public event Action StrictDialogOpened;
    public event Action StrictDialogClosed;

    // :::::::::: MONO METHODS ::::::::::
    protected override void OnEnable()
    {
        tutorialManager.TutorialStarted += StartTutorialDialogs;
        tutorialManager.TutorialSectionStarted += OnTutorialSectionStarted;
        base.OnEnable();
    }
    protected override void OnDisable()
    {
        tutorialManager.TutorialStarted -= StartTutorialDialogs;
        tutorialManager.TutorialSectionStarted -= OnTutorialSectionStarted;
        base.OnDisable();
    }

    // :::::::::: EXCLUSIVE METHODS ::::::::::
    private void StartTutorialDialogs(Tutorial tutorial)
    {
        portrait.sprite = tutorial.info.character.portrait;
        characterName.text = tutorial.info.character.characterName;
    }

    private void OnTutorialSectionStarted(TutorialSection section)
    {
        currentSection = section;
        dialogs = section.dialogs;

        if (currentSection.type != SectionType.Close)
            FinishedTyping += OnFinishedTyping;
        StrictDialogOpened?.Invoke();
        tutorialDialogUI.SetActive(true);

        StartDialog();
    }

    private void OnFinishedTyping()
    {
        if (currentIndex == dialogs.Count - 1)
        {
            ReadyToCompleteSection?.Invoke(currentSection);
            StrictDialogClosed?.Invoke();
            tutorialAllowsIt = false;
        }       
    }

    // :::::::::: OERRIDE METHODS ::::::::::
    public override void NextDialog()
    {
        currentIndex++;
        if (currentIndex < dialogs.Count)
            ShowNextDialog();
        else if (currentSection.type == SectionType.Close)
            EndDialog();
    }

    protected override void ShowNextDialog()
    {
        if (currentSection.type != SectionType.Close
            && currentIndex == dialogs.Count - 1) next.gameObject.SetActive(false);
        else next.gameObject.SetActive(true);

        base.ShowNextDialog();
    }

    public override void EndDialog()
    {
        tutorialAllowsIt = true;
        FinishedTyping -= OnFinishedTyping;

        if (currentSection.type == SectionType.Close)
        {
            StrictDialogClosed?.Invoke();
            tutorialManager.StopTutorial();
        }

        tutorialDialogUI.SetActive(false);
        base.EndDialog();
    }
}
