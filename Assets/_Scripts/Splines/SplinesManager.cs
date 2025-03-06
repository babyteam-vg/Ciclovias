using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SplinesManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Graph graph;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    [Header("Variables")]
    [SerializeField] private Material splineMaterial;
    [SerializeField] private float splineWidth = 0.1f;

    private float tolerance = 0.9f;
    private SplineContainer splineContainer;
    private Spline spline;
    private Vector2Int previousNodePosition;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null)
            splineContainer = gameObject.AddComponent<SplineContainer>();

        spline = splineContainer.Spline;
    }

    private void OnEnable()
    {
        laneConstructor.OnLaneBuilt += HandleLaneBuilt;
        //laneDestructor.OnLaneDestroyed += HandleLaneDestroyed;
    }
    private void OnDisable()
    {
        laneConstructor.OnLaneBuilt -= HandleLaneBuilt;
        //laneDestructor.OnLaneDestroyed -= HandleLaneDestroyed;
    }

    private void Start()
    {
        
    }

    // :::::::::: PUBLIC METHODS ::::::::::

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: When a Lane is Built
    private void HandleLaneBuilt(Vector2Int addedNodePosition)
    {
        Node addedNode = graph.GetNode(addedNodePosition);
        if (addedNode == null) return;

        if (previousNodePosition == null) return;
        List<Vector2Int> previousNeighbors = graph.GetNeighborsPos(previousNodePosition);

        // Edge of the Lane
        if (previousNeighbors.Count < 2) // Current Spline
        {
            StartNewSpline(addedNode.worldPosition);
            //Debug.Log("LÍMITE");
            previousNodePosition = addedNodePosition;
            return;
        }

        // Check Collinearity
        bool isIntersection = false;
        foreach (var neighbor in previousNeighbors)
            if (!IsCollinear(previousNodePosition, neighbor, addedNodePosition))
            {
                isIntersection = true;
                break;
            }

        if (isIntersection)
        {
            StartNewSpline(addedNode.worldPosition);
            Debug.Log("INTERSECCIÓN");
        }
        else
        {
            AddKnotToCurrentSpline(addedNode.worldPosition);
            Debug.Log("COLINEALES");
        }

        previousNodePosition = addedNodePosition;
    }

    // ::::: Detect Collinearity
    private bool IsCollinear(Vector2Int a, Vector2Int b, Vector2Int c)
    {
        Vector2Int v1 = b - a;
        Vector2Int v2 = c - a;

        int crossProduct = v1.x * v2.y - v1.y * v2.x;

        return crossProduct == 0;
    }

    // ::::: New Spline (Limite or Intersection)
    private void StartNewSpline(Vector3 position)
    {
        // Crear una nueva spline
        Spline newSpline = new Spline();

        // Crear el primer BezierKnot (puedes ajustar las tangentes y rotación si es necesario)
        BezierKnot startKnot = new BezierKnot(position);

        // Añadir el primer knot a la nueva spline
        newSpline.Insert(0, startKnot, TangentMode.Broken, 0.5f);  // Puedes ajustar el modo de tangente y la tensión

        // Asignar la nueva spline al SplineContainer
        splineContainer.AddSpline(newSpline);

        // Debug para verificar que la nueva spline fue añadida
        Debug.Log("Nueva spline iniciada y añadida al contenedor.");

        // Si lo deseas, también puedes asignar la nueva spline a la variable spline para seguir trabajando con ella
        spline = newSpline;
    }

    private void AddKnotToCurrentSpline(Vector3 worldPosition)
    {
        // Crea un BezierKnot con la posición y sin tangentes por ahora (tú puedes definir las tangentes y rotación luego)
        BezierKnot newKnot = new BezierKnot(worldPosition);

        // Aquí verificamos si la spline ya está inicializada, si no es así, inicializamos el contenedor
        if (spline != null)
        {
            // Usamos el método Insert para agregar el nuevo knot en la posición deseada
            spline.Insert(spline.Count, newKnot, TangentMode.Broken, 0.5f);  // Puedes ajustar el modo de tangente y la tensión
            Debug.Log("Nuevo Knot añadido a la spline.");
        }
        else
        {
            Debug.LogWarning("No se pudo añadir el knot, la spline no está inicializada.");
        }
    }

}