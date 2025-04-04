using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class MaterialManager : MonoBehaviour
{
    public static MaterialManager Instance { get; private set; }
    public int MaterialAmount { get; private set; } = 0;

    [Header("Dependencies")]
    [SerializeField] private InputManager inputManager;

    [Header("UI References")]
    public Image materialIcon;
    public TextMeshProUGUI materialTMP;
    public Sprite buildImg;
    public Sprite destroyImg;

    public event Action NotEnoughMaterial;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnEnable()
    {
        inputManager.OnRightClickDown += ChangeToDestroy;
        inputManager.OnRightClickUp += ChangeToBuild;
        inputManager.InputBlocked += ChangeToBuild;
    }
    private void OnDisable()
    {
        inputManager.OnRightClickDown -= ChangeToDestroy;
        inputManager.OnRightClickUp -= ChangeToBuild;
        inputManager.InputBlocked -= ChangeToBuild;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void AddMaterial(int amount)
    {
        MaterialAmount += amount;
        materialTMP.text = MaterialAmount.ToString();
    }
    public bool ConsumeMaterial(int amount)
    {
        if (MaterialAmount >= amount)
        {
            MaterialAmount -= amount;
            materialTMP.text = MaterialAmount.ToString();
            return true;
        }
        else
        {
            NotEnoughMaterial?.Invoke();
            return false;
        }
    }

    // ::::: Load Game
    public void LoadMaterial(int amount)
    {
        MaterialAmount = amount;
        materialTMP.text = MaterialAmount.ToString();
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void ChangeToBuild(Vector2Int _)
    {
        materialIcon.sprite = buildImg;
    }
    private void ChangeToDestroy(Vector2Int _)
    {
        materialIcon.sprite = destroyImg;
    }
}
