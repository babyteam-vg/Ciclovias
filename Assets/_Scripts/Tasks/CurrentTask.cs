using System;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CurrentTask : MonoBehaviour
{
    public static CurrentTask Instance { get; private set; }
    public Task PinnedTask { get; private set; }

    [Header("UI References - Texts")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI fromText;
    [SerializeField] private TextMeshProUGUI toText;

    [Header("UI References - Sliders")]
    [SerializeField] private Image safetyFill;
    [SerializeField] private Image charmFill;
    [SerializeField] private Image flowFill;

    [Header("UI References - Markers")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Canvas worldCanvas;
    [SerializeField] private Image fromMarker;
    [SerializeField] private Image toMarker;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (!ThereIsPinned() && GameStateManager.Instance.CurrentMapState == 0)
        {
            Task firstTask = TaskDiary.Instance.tasks[0];
            if (firstTask.state == 2)
                PinTask(firstTask); // Automatically Pin Task 0-1 "Tutorial"
        }
    }

    private void Update()
    {
        if (ThereIsPinned())
        {
            Task task = PinnedTask;

            // Sliders
            float safetyUI = 1f - (float)task.currentSafetyDiscount / (float)task.info.maxSafetyDiscount;
            safetyFill.fillAmount = Mathf.Clamp(safetyUI, 0f, 1f);

            float charmUI = (float)task.currentCharmCount / (float)task.info.minCharmCount;
            charmFill.fillAmount = Mathf.Clamp(charmUI, 0f, 1f);

            float flowUI = task.currentFlowPercentage / task.info.minFlowPercentage;
            flowFill.fillAmount = flowUI;
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Is There a Task Pinned?
    public bool ThereIsPinned() { return PinnedTask != null; }

    // ::::: Pin a Task
    public void PinTask(Task task)
    {
        if (PinnedTask == task) return;

        PinnedTask = task;
        UpdateTaskUI();
        //UpdateTaskMarkers();
    }
    public void UnpinTask(Task task)
    {
        if (!ThereIsPinned()) return;

        PinnedTask = null;
        UpdateTaskUI();
        fromMarker.gameObject.SetActive(false);
        toMarker.gameObject.SetActive(false);
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: UI Only Affected When Changing the Pinned Task
    private void UpdateTaskUI()
    {
        // Texts
        if (ThereIsPinned())
        {
            titleText.text = PinnedTask.info.title;
            fromText.text = PinnedTask.info.from.compoundName;
            toText.text = PinnedTask.info.to.compoundName;
        }
        else
        {
            titleText.text = "Task";
            fromText.text = "From";
            toText.text = "To";
        }
    }

    //private void UpdateTaskMarkers()
    //{
    //    if (!ThereIsPinned() || worldCanvas == null || mainCamera == null) return;

    //    // Obtener posiciones en el mundo (XZ)
    //    Vector3 fromWorldPos = PinnedTask.fromCompound.transform.position;
    //    Vector3 toWorldPos = PinnedTask.toCompound.transform.position;

    //    // Ajustar para proyectar en el plano XZ (forzar Y según la cámara)
    //    fromWorldPos.y = mainCamera.transform.position.y;
    //    toWorldPos.y = mainCamera.transform.position.y;

    //    // Convertir posiciones del mundo a coordenadas de pantalla
    //    Vector3 fromScreenPos = mainCamera.WorldToScreenPoint(fromWorldPos);
    //    Vector3 toScreenPos = mainCamera.WorldToScreenPoint(toWorldPos);

    //    // Convertir coordenadas de pantalla a posiciones dentro del Canvas
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //        worldCanvas.transform as RectTransform, fromScreenPos, mainCamera, out Vector2 fromCanvasPos);
    //    RectTransformUtility.ScreenPointToLocalPointInRectangle(
    //        worldCanvas.transform as RectTransform, toScreenPos, mainCamera, out Vector2 toCanvasPos);

    //    // Aplicar posiciones a los marcadores
    //    fromMarker.rectTransform.anchoredPosition = fromCanvasPos;
    //    toMarker.rectTransform.anchoredPosition = toCanvasPos;

    //    // Activar los marcadores
    //    fromMarker.gameObject.SetActive(true);
    //    toMarker.gameObject.SetActive(true);
    //}
}
