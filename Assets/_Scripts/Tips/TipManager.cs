using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;

public class TipManager : MonoBehaviour
{
    public static TipManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private LaneDestructor laneDestructor;

    [Header("")]
    public GameObject tipUI;
    public List<Tip> tips;

    private bool isShowingTip;
    private Queue<Tip> tipsQueue = new Queue<Tip>();
    private TextMeshProUGUI tipTMP;

    private Dictionary<int, Action> tipActions = new Dictionary<int, Action>();

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        tipTMP = tipUI.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        if (tips.Count > 0)
            SubscribeToTriggers();
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: 
    public void AddTip(Tip tip)
    {
        if (tip.seen) return;

        tip.seen = true;
        tipsQueue.Enqueue(tip);
        UnsubscribeFromTip(tip);

        if (!isShowingTip)
            StartCoroutine(ShowTips());
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: 
    private IEnumerator ShowTips()
    {
        isShowingTip = true;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxs[2]);

        while (tipsQueue.Count > 0)
        {
            Tip tip = tipsQueue.Dequeue();
            tipTMP.text = tip.info.tipText;
            tipUI.SetActive(true);

            yield return new WaitForSeconds(12f);

            tipUI.SetActive(false);
        }

        isShowingTip = false;
    }

    // :::::::::: SUBSCRIPTION METHODS ::::::::::
    private void SubscribeToTriggers()
    {
        foreach (Tip tip in tips)
        {
            if (tip.seen) continue;

            Action action = () => AddTip(tip);
            tipActions[tip.info.id] = action;

            switch (tip.info.id)
            {
                case 0:
                    laneDestructor.OnTryDestroySealed += () => AddTip(tip);
                    break;
                case 1: // OnGameStart
                    //GameManager.OnGameStart += () => AddTip(tip);
                    break;

                case 2: // OnLevelComplete
                    //LevelManager.OnLevelComplete += () => AddTip(tip);
                    break;

                case 3: // OnFirstDeath
                    //Player.OnDeath += () =>
                    //{
                    //    if (!tip.seen) AddTip(tip);
                    //};
                    break;
            }
        }
    }

    private void UnsubscribeFromTip(Tip tip)
    {
        if (!tipActions.TryGetValue(tip.info.id, out Action action)) return;

        switch (tip.info.id)
        {
            case 1:
                //GameManager.OnGameStart -= action;
                break;

            case 2:
                //LevelManager.OnLevelComplete -= action;
                break;

            case 3:
                //Player.OnDeath -= action;
                break;
        }

        tipActions.Remove(tip.info.id);
    }

    // :::::::::: STORAGE ::::::::::
    // ::::: Tips -> TipsData
    public List<TipData> SaveTips()
    {
        List<TipData> tipsData = new List<TipData>();
        foreach (Tip tip in tips)
            tipsData.Add(tip.SaveTip());
        return tipsData;
    }

    // ::::: TipsData -> Tips
    public void LoadTips(List<TipData> tipsData)
    {
        if (tipsData.Count != tips.Count) return;

        for (int i = 0; i < tips.Count; i++)
        {
            Tip tip = tips[i];
            TipData tipData = tipsData[i];

            tip.seen = tipData.seen;
        }
    }
}