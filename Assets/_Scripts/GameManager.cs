using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public int MapState { get; private set; } = 0;
    public int SmokeState { get; private set; } = 0;
    public int MaterialAmount { get; private set; } = 80;

    [SerializeField] private GameObject materialCounter;
    [SerializeField] private TextMeshProUGUI amountText;

    private Animator animator;

    public event Action<int> MapStateAdvanced;
    public event Action<int> SmokeStateReduced;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        amountText.text = "x" + MaterialAmount.ToString();
        animator = materialCounter.GetComponent<Animator>();
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Map
    public void AdvanceMapState() {
        MapState++;
        MapStateAdvanced?.Invoke(MapState); // !
    }

    // ::::: Smoke
    public void ReduceSmokeState()
    {
        SmokeState--;
        SmokeStateReduced?.Invoke(SmokeState); // !
    }

    // ::::: Material
    public void AddMaterial(int cantidad)
    {
        MaterialAmount += cantidad;
        amountText.text = "x" + MaterialAmount.ToString();
        MaterialCounterAnimation();
    }
    public bool ConsumeMaterial(int cantidad)
    {
        if (MaterialAmount >= cantidad)
        {
            MaterialAmount -= cantidad;
            amountText.text = "x" + MaterialAmount.ToString();
            MaterialCounterAnimation();
            return true;
        }
        else
            return false;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void MaterialCounterAnimation()
    {
        animator.Play("MaterialCounter");
    }
}
