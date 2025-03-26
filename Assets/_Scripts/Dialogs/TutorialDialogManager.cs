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
    private void OnEnable()
    {
        tutorialManager.TutorialStarted += StartTutorialDialogs;
        tutorialManager.TutorialSectionStarted += OnTutorialSectionStarted;
    }
    private void OnDisable()
    {
        tutorialManager.TutorialStarted -= StartTutorialDialogs;
        tutorialManager.TutorialSectionStarted -= OnTutorialSectionStarted;
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

    public override void EndDialog()
    {
        tutorialAllowsIt = true;
        FinishedTyping -= OnFinishedTyping;

        if (currentSection.type == SectionType.Close)
        {
            tutorialManager.StopTutorial();
            StrictDialogClosed?.Invoke();
        }

        tutorialDialogUI.SetActive(false);
        base.EndDialog();
    }
}
