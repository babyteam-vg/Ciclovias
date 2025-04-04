using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class IntroManager : MonoBehaviour
{
    public VideoPlayer player;
    public CanvasGroup playerCanvasGroup;
    public TextMeshProUGUI cardTMP;
    public CanvasGroup cardCanvasGroup;
    public CanvasGroup nextCanvasGroup;
    public GameObject background;

    private bool isAllowed = false;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable() { player.loopPointReached += OnLoopReached; }
    private void OnDisable() { player.loopPointReached -= OnLoopReached; }

    private void Start()
    {
        cardTMP.text =
            $"Dear {PlayerNameManager.Instance.GetPlayerName()}," +
            $"\n\n" +
            $"We are pleased to announce that you have been appointed as the new Chief Engineer in Bike Lanes of our city." +
            $"\n\n" +
            $"An inspector will be expecting you at the Bike Store tomorrow morning to assist you in becoming acquainted with your new role." +
            $"\n\n" +
            $"Sincerely," +
            $"\n" +
            $"the Municipality.";
    }

    private void Update()
    {
        background.transform.position -= new Vector3(0.1f, 0f, 0f);
        if (background.transform.position.x <= -960f)
            background.transform.position = new Vector3(0f, 540f, 0f);

        if (isAllowed && Input.GetMouseButtonDown(0))
        {
            isAllowed = false;
            LoadingScene.Instance.LoadScene(2);
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void OnLoopReached(VideoPlayer vp)
    {
        StartCoroutine(ShowUpText());
    }

    private IEnumerator ShowUpText()
    {
        StartCoroutine(LoadingScene.Instance.FadeCanvasGroup(playerCanvasGroup, 1f, 0f, 2f));
        yield return StartCoroutine(LoadingScene.Instance.FadeCanvasGroup(cardCanvasGroup, 0f, 1f, 2f));
        yield return new WaitForSeconds(5f);
        yield return StartCoroutine(LoadingScene.Instance.FadeCanvasGroup(nextCanvasGroup, 0f, 1f, 1f));
        isAllowed = true;
    }
}
