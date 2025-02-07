using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public int CurrentMapState { get; private set; } = 0;
    public int CurrentSmokeState { get; private set; } = 0;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Advance, Reduce
    public void AdvanceMapState() { CurrentMapState++; }
    public void ReduceSmokeState() { CurrentSmokeState--; }
}
