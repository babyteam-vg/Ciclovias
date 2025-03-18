using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Dependencies")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Grid grid;
    [SerializeField] private GridGenerator gridGenerator;
    [SerializeField] private Graph graph;
    [SerializeField] private IsometricCameraController cameraController;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    private Pathfinder pathfinder;
    private CellScoresCalculator cellScoresCalculator;

    [Header("UI References")]
    [SerializeField] private GameObject tutorialCanvas;
    [SerializeField] private TextMeshProUGUI tutorialTitle;
    [SerializeField] private TextMeshProUGUI tutorialText;

    [Header("Tutorials")]
    public Tutorial activeTutorial = null;
    public List<Tutorial> tutorials;

    private Dictionary<Vector2Int, Tutorial> tutorialDictionary;
    private int currentSectionIndex;
    private bool isTutorialActive = false;
    private MapData originalMapData;

    public event Action<Tutorial> ActiveTutorialScoresUpdated;
    public event Action<Tutorial> TutorialStarted;
    public event Action<TutorialSection> TutorialSectionStarted;
    public event Action TutorialSectionPresentationStarted;
    public event Action TutorialSectionPresentationDone;
    public event Action<List<Vector2Int>> TutorialSectionSealed;
    public event Action TutorialCompleted;

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
        gameManager.MapStateAdvanced += GetNextTutorial;

        laneConstructor.OnLaneBuilt += CheckSectionProgress;
        laneDestructor.OnLaneDestroyed += CheckSectionProgress;
    }
    private void OnDisable()
    {
        gameManager.MapStateAdvanced -= GetNextTutorial;

        laneConstructor.OnLaneBuilt -= CheckSectionProgress;
        laneDestructor.OnLaneDestroyed -= CheckSectionProgress;
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Start a Tutorial
    public void PlayTutorial(Vector2Int tutorialID)
    {
        if (isTutorialActive) return;

        if (tutorialDictionary.TryGetValue(tutorialID, out Tutorial tutorial))
        {
            activeTutorial = tutorial;
            currentSectionIndex = 0;

            originalMapData = gridGenerator.GetMapDataForCoordinates(tutorial.info.sections[currentSectionIndex].tutorialMap.coordinates);

            StartCoroutine(ExecuteSection(activeTutorial.info.sections[currentSectionIndex]));

            if (tutorialCanvas != null)
            {
                tutorialCanvas.SetActive(true);
                tutorialTitle.text = activeTutorial.info.title;
            }

            TutorialStarted?.Invoke(activeTutorial); // !
        }
    }

    // ::::: Manage Section of a Tutorial
    public void CheckSectionProgress(Vector2Int gridPosition)
    {
        if (!isTutorialActive || activeTutorial == null) return;

        TutorialSection currentSection = activeTutorial.info.sections[currentSectionIndex];

        (bool pathFound, List<Vector2Int> path) = pathfinder.FindPath(currentSection.start, gridPosition, currentSection.end);

        activeTutorial.currentSafety = cellScoresCalculator.CalculatePathSafety(path);
        activeTutorial.currentCharm = cellScoresCalculator.CalculatePathCharm(path);
        activeTutorial.currentFlow = cellScoresCalculator.CalculatePathFlow(path, currentSection.end);
        activeTutorial.usedMaterial = path.Count - 1;

        ActiveTutorialScoresUpdated?.Invoke(activeTutorial);

        if (currentSection.destroyRequirement)
        {
            if (graph.GetNode(currentSection.start) == null && graph.GetNode(currentSection.end) == null)
                CompleteSection();
        }   
        else
        {
            if (graph.AreConnectedByPath(currentSection.start, currentSection.end))
            {
                if (currentSection.checkRequirements)
                {
                    if (activeTutorial.MeetsRequirements(
                        activeTutorial.currentSafety,
                        activeTutorial.currentCharm,
                        activeTutorial.currentFlow,
                        activeTutorial.usedMaterial)) CompleteSection(path);
                }
                else CompleteSection(path);
            }
        }
    }

    // ::::: End a Tutorial
    public void StopTutorial()
    {
        if (!isTutorialActive) return;

        activeTutorial.completed = true;
        StopAllCoroutines();
        isTutorialActive = false;
        activeTutorial = null;
        tutorialCanvas.SetActive(false);

        gridGenerator.GenerateGridPortion(originalMapData);
        TutorialCompleted?.Invoke(); // !
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    private void InitializeTutorialDictionary()
    {
        tutorialDictionary = new Dictionary<Vector2Int, Tutorial>();

        foreach (var tutorial in tutorials)
            tutorialDictionary[tutorial.info.id] = tutorial;
    }

    // ::::: Get the Next Tutorial to Play
    private void GetNextTutorial(int currentMapState)
    {
        var nextTutorials = tutorials.Where(t => t.info.id.x == currentMapState && t.completed == false)
            .OrderBy(t => t.info.id.y).ToList();
        Tutorial nextTutorial = nextTutorials.FirstOrDefault();
        PlayTutorial(nextTutorial.info.id);
    }

    // ::::: Start a Section of the Tuutorial
    private IEnumerator ExecuteSection(TutorialSection section)
    {
        isTutorialActive = true;
        tutorialCanvas.SetActive(true);
        tutorialText.text = section.text;

        gridGenerator.ClearGridPortion(section.tutorialMap.coordinates);
        gridGenerator.GenerateGridPortion(section.tutorialMap);

        TutorialSectionStarted?.Invoke(section); // !

        TutorialSectionPresentationStarted?.Invoke(); // !
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
        TutorialSectionPresentationDone?.Invoke(); // !
    }

    // ::::: Complete Section
    private void CompleteSection(List<Vector2Int> path = null)
    {
        if (path != null
            && !activeTutorial.info.sections[currentSectionIndex].dontAddToPath)
        {
            graph.SealNodes(path);
            TutorialSectionSealed(path);
        }

        currentSectionIndex++;
        AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxs[2]);

        if (currentSectionIndex < activeTutorial.info.sections.Length)
            StartCoroutine(ExecuteSection(activeTutorial.info.sections[currentSectionIndex]));
        else StopTutorial();
    }

    // :::::::::: STORAGE ::::::::::
    // ::::: Tutorials -> TutorialsData
    public List<TutorialData> SaveTutorials()
    {
        List<TutorialData> tutorialsData = new List<TutorialData>();
        foreach (Tutorial tutorial in tutorials)
            tutorialsData.Add(tutorial.SaveTutorial());
        return tutorialsData;
    }

    // ::::: TutorialsData -> Tutorials
    public void LoadTutorials(List<TutorialData> tutorialsData)
    {
        if (tutorialsData.Count != tutorials.Count) return;

        for (int i = 0; i < tutorials.Count; i++)
        {
            Tutorial tutorial = tutorials[i];
            TutorialData tutorialData = tutorialsData[i];

            tutorial.completed = tutorialData.completed;
        }
    }
}
