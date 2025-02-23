using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private Grid grid;
    [SerializeField] private GridGenerator gridGenerator;
    [SerializeField] private Graph graph;
    [SerializeField] private IsometricCameraController cameraController;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private List<TutorialData> tutorialDataList;

    private Pathfinder pathfinder;
    private CellScoresCalculator cellScoresCalculator;

    [Header("UI References")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private TextMeshProUGUI tutorialText;

    private Dictionary<Vector2Int, TutorialData> tutorialDictionary;
    private TutorialData activeTutorial;
    private int currentSectionIndex;
    private bool isTutorialActive = false;
    private MapData originalMapData;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeTutorialDictionary();
        pathfinder = new Pathfinder(graph);
        cellScoresCalculator = new CellScoresCalculator(grid);
    }

    private void OnEnable()
    {
        laneConstructor.OnLaneBuilt += CheckSectionProgress;
    }

    private void OnDisable()
    {
        laneConstructor.OnLaneBuilt -= CheckSectionProgress;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    public void PlayTutorial(Vector2Int tutorialID)
    {
        if (isTutorialActive) return;

        if (tutorialDictionary.TryGetValue(tutorialID, out TutorialData tutorialData))
        {
            activeTutorial = tutorialData;
            currentSectionIndex = 0;

            originalMapData = gridGenerator.GetMapDataForCoordinates(tutorialData.sections[currentSectionIndex].tutorialMap.coordinates);

            StartCoroutine(ExecuteSection(activeTutorial.sections[currentSectionIndex]));

            if (tutorialCanvas != null)
                tutorialCanvas.SetActive(true);
        }
    }

    public void CheckSectionProgress(Vector2Int gridPosition)
    {
        if (!isTutorialActive || activeTutorial == null) return;

        TutorialSection currentSection = activeTutorial.sections[currentSectionIndex];
        if (gridPosition == currentSection.end)
        {
            if (currentSection.checkRequirements)
            {
                (bool pathFound, List<Vector2Int> path) = pathfinder.FindPath(currentSection.start, gridPosition, currentSection.end);

                int safety = cellScoresCalculator.CalculatePathSafety(path);
                int charm = cellScoresCalculator.CalculatePathCharm(path);
                float flow = cellScoresCalculator.CalculatePathFlow(path, currentSection.end);
                int usedMaterial = path.Count;

                if (activeTutorial.MeetsRequirements(safety, charm, flow, usedMaterial))
                {
                    currentSectionIndex++;
                    if (currentSectionIndex < activeTutorial.sections.Length)
                        StartCoroutine(ExecuteSection(activeTutorial.sections[currentSectionIndex]));
                    else StopTutorial();
                }
            }
            else
            {
                currentSectionIndex++;
                if (currentSectionIndex < activeTutorial.sections.Length)
                    StartCoroutine(ExecuteSection(activeTutorial.sections[currentSectionIndex]));
                else StopTutorial();
            }
        }
    }

    public void StopTutorial()
    {
        if (!isTutorialActive) return;

        StopAllCoroutines();
        isTutorialActive = false;
        cameraController.UnblockCamera();
        activeTutorial = null;
        tutorialCanvas.SetActive(false);

        gridGenerator.GenerateGridPortion(originalMapData);
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void InitializeTutorialDictionary()
    {
        tutorialDictionary = new Dictionary<Vector2Int, TutorialData>();

        foreach (var tutorialData in tutorialDataList)
            tutorialDictionary[tutorialData.id] = tutorialData;
    }

    private IEnumerator ExecuteSection(TutorialSection section)
    {
        isTutorialActive = true;
        cameraController.BlockCamera();
        tutorialCanvas.SetActive(true);
        tutorialText.text = section.text;

        gridGenerator.ClearGridPortion(section.tutorialMap.coordinates);
        gridGenerator.GenerateGridPortion(section.tutorialMap);

        foreach (var keyframe in section.keyframes)
        {
            float elapsedTime = 0f;
            Vector3 startPosition = cameraController.transform.position;
            Quaternion startRotation = cameraController.transform.rotation;
            float startZoom = cameraController.GetCurrentZoom();

            while (elapsedTime < keyframe.duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / keyframe.duration;

                cameraController.transform.position = Vector3.Lerp(startPosition, keyframe.position, t);
                cameraController.transform.rotation = Quaternion.Slerp(startRotation, keyframe.rotation, t);
                cameraController.SetZoom(Mathf.Lerp(startZoom, keyframe.zoom, t));

                yield return null;
            }
        }
    }
}
