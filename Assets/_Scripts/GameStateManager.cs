using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public int CurrentMapState { get; private set; } = 0;
    public int CurrentSmokeState { get; private set; } = 0;

    [SerializeField] private MenuManager menuManager;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (!CurrentTask.Instance.ThereIsPinned() && GameStateManager.Instance.CurrentMapState == 0)
        {
            Task firstTask = TaskDiary.Instance.tasks[0];
            if (firstTask.state == 1)
                firstTask.fromCompound.GetNextAvailableTask(GameStateManager.Instance.CurrentMapState);
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Advance, Reduce
    public void AdvanceMapState() { CurrentMapState++; }
    public void ReduceSmokeState() { CurrentSmokeState--; }
}
