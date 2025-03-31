using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer player;
    public TextMeshProUGUI cardTMP;
    public CanvasGroup cardCanvasGroup;

    private bool isAllowed = false;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable() { player.loopPointReached += OnLoopReached; }
    private void OnDisable() { player.loopPointReached -= OnLoopReached; }

    private void Start()
    {
        cardTMP.text =
            $"      Dear {PlayerNameManager.Instance.GetPlayerName()}," +
            $"\n\n" +
            $"      We are pleased to announce that you have been appointed as the new Chief Engineer for Bike Lanes Construction of our city." +
            $"\n\n" +
            $"      An inspector will be expecting you at the Bike Store tomorrow morning to assist you in becoming acquainted with your new role." +
            $"\n\n" +
            $"      Sincerely," +
            $"\n" +
            $"the Municipality.";
    }

    private void Update()
    {
        if (isAllowed && Input.GetMouseButtonDown(0))
        {
            LoadingScene.Instance.LoadScene(2);
            isAllowed = false;
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void OnLoopReached(VideoPlayer vp)
    {
        StartCoroutine(ShowUpText());
    }

    private IEnumerator ShowUpText()
    {
        yield return StartCoroutine(LoadingScene.Instance.FadeCanvasGroup(cardCanvasGroup, 0f, 1f, 1f));
        isAllowed = true;
    }
}
