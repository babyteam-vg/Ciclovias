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
    [SerializeField] private TutorialDialogManager tutorialDialogManager;

    private Pathfinder pathfinder;
    private CellScoresCalculator cellScoresCalculator;

    [Header("Tutorials")]
    public bool isTutorialActive = false;
    public List<Tutorial> tutorials;

    private Tutorial activeTutorial;
    private bool readyToCompleteSection = false;
    private Dictionary<Vector2Int, Tutorial> tutorialsDictionary;
    private int currentSectionIndex;
    private MapData originalMapData;

    public event Action<Tutorial> ActiveTutorialScoresUpdated;
    public event Action<Tutorial> TutorialStarted;
    public event Action TutorialCompleted;

    public event Action<Tutorial> TutorialLaneConnected;
    public event Action TutorialLaneDisconnected;

    public event Action<TutorialSection> TutorialSectionStarted;
    public event Action<TutorialSection> TutorialSectionReadyToCompleteStarted;
    public event Action TutorialSectionPresentationStarted;
    public event Action TutorialSectionPresentationDone;
    public event Action<List<Vector2Int>> TutorialSectionSealed;
    public event Action TutorialSectionCompleted;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        InitializeTutorialsDictionary();
        pathfinder = new Pathfinder(graph);
        cellScoresCalculator = new CellScoresCalculator(grid, graph);
    }

    private void OnEnable()
    {
        gameManager.MapStateAdvanced += GetNextTutorial;
        tutorialDialogManager.ReadyToCompleteSection += OnReadyToCompleteSection;
    }
    private void OnDisable()
    {
        gameManager.MapStateAdvanced -= GetNextTutorial;
        tutorialDialogManager.ReadyToCompleteSection -= OnReadyToCompleteSection;
    }

    // :::::::::: MANAGE METHODS ::::::::::
    // ::::: Start a Tutorial
    public void PlayTutorial(Vector2Int tutorialID)
    {
        if (isTutorialActive) return;

        if (tutorialsDictionary.TryGetValue(tutorialID, out Tutorial tutorial))
        {
            activeTutorial = tutorial;
            isTutorialActive = true;

            // Events
            laneConstructor.OnLaneBuilt += CheckSectionProgress;
            laneDestructor.OnLaneDestroyed += CheckSectionProgress;

            // Updates
            currentSectionIndex = 0;
            originalMapData = gridGenerator.GetMapDataForCoordinates(activeTutorial.info.sections[currentSectionIndex].tutorialMap.coordinates);
            StartCoroutine(ExecuteSection(activeTutorial.info.sections[currentSectionIndex]));
            TutorialStarted?.Invoke(activeTutorial); // !
        }
    }

    // ::::: Manage Section of a Tutorial
    private void CheckSectionProgress(Vector2Int gridPosition)
    {
        if (!isTutorialActive) return;

        TutorialSection currentSection = activeTutorial.info.sections[currentSectionIndex];

        if (readyToCompleteSection)
        {
            // Pathfinding
            (bool pathFound, List<Vector2Int> path) = pathfinder.FindPath(currentSection.start, gridPosition, currentSection.end);

            activeTutorial.currentSafety = cellScoresCalculator.CalculatePathSafety(path);
            activeTutorial.currentCharm = cellScoresCalculator.CalculatePathCharm(path);
            activeTutorial.currentFlow = cellScoresCalculator.CalculatePathFlow(path);
            activeTutorial.usedMaterial = path.Count - 1;

            ActiveTutorialScoresUpdated?.Invoke(activeTutorial);

            if (currentSection.type == SectionType.Destroy) // Destroy Section
            {
                if (graph.GetNeighborsCount(currentSection.auxStart) == 0
                    && graph.GetNeighborsCount(currentSection.auxEnd) == 0
                    && !graph.AreConnectedByPath(currentSection.auxStart, currentSection.auxEnd))
                    CompleteSection();
            }
            else if (currentSection.type == SectionType.Build) // Build Section
            {
                if (graph.AreConnectedByPath(currentSection.auxStart, currentSection.auxEnd))
                {
                    TutorialLaneConnected?.Invoke(activeTutorial);

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
                else TutorialLaneDisconnected?.Invoke();
            }
            else CompleteSection();
        }
    }

    // ::::: End a Tutorial
    public void StopTutorial()
    {
        if (!isTutorialActive) return;

        // Events
        laneConstructor.OnLaneBuilt -= CheckSectionProgress;
        laneDestructor.OnLaneDestroyed -= CheckSectionProgress;

        // Updates
        activeTutorial.completed = true;
        isTutorialActive = false;
        gridGenerator.GenerateGridPortion(originalMapData);
        TutorialCompleted?.Invoke(); // !
    }

    // :::::::::: SECTION METHODS ::::::::::
    // ::::: Start a Section of the Tuutorial
    private IEnumerator ExecuteSection(TutorialSection section)
    {
        if (section.type != SectionType.Close)
        {
            gridGenerator.ClearGridPortion(section.tutorialMap.coordinates);
            gridGenerator.GenerateGridPortion(section.tutorialMap);
        }

        TutorialSectionStarted?.Invoke(section); // !

        if (section.dialogs.Count <= 1)
            readyToCompleteSection = true;
        else readyToCompleteSection = false;

        // Presentation
        TutorialSectionPresentationStarted?.Invoke(); // !
        Keyframe keyframe = section.keyframe;
        yield return StartCoroutine(cameraController.MoveCamera(keyframe.duration, keyframe.position, keyframe.rotation, keyframe.zoom));
        TutorialSectionPresentationDone?.Invoke(); // !
    }

    // ::::: Complete a Section
    private void CompleteSection(List<Vector2Int> path = null)
    {
        // Seal Path
        if (path != null && !activeTutorial.info.sections[currentSectionIndex].dontAddToPath)
        {
            graph.SealNodes(path);
            TutorialSectionSealed?.Invoke(path);
        }

        // End Dialog
        tutorialDialogManager.EndDialog();
        TutorialSectionCompleted?.Invoke();

        // Advance Section
        currentSectionIndex++;
        if (currentSectionIndex < activeTutorial.info.sections.Length)
            StartCoroutine(ExecuteSection(activeTutorial.info.sections[currentSectionIndex]));
    }

    // :::::::::: SUPPORT METHODS ::::::::::
    private void InitializeTutorialsDictionary()
    {
        tutorialsDictionary = new Dictionary<Vector2Int, Tutorial>();

        foreach (var tutorial in tutorials)
            tutorialsDictionary[tutorial.info.id] = tutorial;
    }

    // ::::: Get the Next Tutorial to Play
    private void GetNextTutorial(int currentMapState)
    {
        var nextTutorials = tutorials.Where(t => t.info.id.x == currentMapState && t.completed == false)
            .OrderBy(t => t.info.id.y).ToList();

        Tutorial nextTutorial = nextTutorials.FirstOrDefault();
        if (nextTutorial != null)
            PlayTutorial(nextTutorial.info.id);
    }

    // ::::: Every Section Dialog Saw
    private void OnReadyToCompleteSection(TutorialSection section)
    {
        readyToCompleteSection = true;
        TutorialSectionReadyToCompleteStarted?.Invoke(section);
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
