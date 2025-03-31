using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Splines;

public class IsometricCameraController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InGameMenuManager inGameMenuManager;
    [SerializeField] private TaskDialogManager taskDialogManager;
    [SerializeField] private TutorialManager tutorialManager;

    public List<GameObject> boundingPlanes;

    [Header("Variables - Pan")]
    public float panSpeed = 30f;

    [Header("Variables - Zoom")]
    public float zoomSpeed = 200;
    public float zoomSmoothness = 50;

    [Header("Variables - Rotation")]
    public float rotationDuration = 0.3f;

    private bool isLocked = false;
    private bool isRotating = false;
    private float currentZoom;
    private float minZoom, maxZoom;
    private Vector2 edgePanVelocity;
    private Vector2 panHorLimit, panVertLimit;
    private Vector2 screenSize;
    private Coroutine cameraMovement;

    const float ZOOM_PAN_RATIO = 2f;
    const float ZOOM_OFFSET = 4f;
    const float EDGE_THRESHOLD = 5f;
    const float DURATION = 3f;

    // :::::::::: MONO METHODS ::::::::::
    private void Awake()
    {
        Bounds initialBounds = boundingPlanes[0].GetComponent<Renderer>().bounds;

        // Zoom
        minZoom = initialBounds.min.y + ZOOM_OFFSET;
        maxZoom = initialBounds.max.y + 2 * ZOOM_OFFSET;
        currentZoom = (maxZoom + minZoom) / 2;

        // Limits
        panHorLimit = new Vector2(initialBounds.min.x + 5f, initialBounds.max.x);
        panVertLimit = new Vector2(initialBounds.min.z, initialBounds.max.z - 5f);
        transform.position = new Vector3((panHorLimit.x + panHorLimit.y) / 2, 0f, (panVertLimit.x + panVertLimit.y) / 2);

        screenSize = new Vector2(Screen.width, Screen.height);
    }

    private void OnEnable()
    {
        gameManager.MapStateAdvanced += NewAreaBounds;

        inGameMenuManager.MenuOpened += LockCamera;
        inGameMenuManager.MenuClosed += UnlockCamera;

        taskDialogManager.StrictDialogOpened += LockCamera;
        taskDialogManager.StrictDialogClosed += UnlockCamera;

        tutorialManager.TutorialSectionPresentationStarted += LockCamera;
        tutorialManager.TutorialCompleted += UnlockCamera;
    }
    private void OnDisable()
    {
        gameManager.MapStateAdvanced -= NewAreaBounds;

        inGameMenuManager.MenuOpened -= LockCamera;
        inGameMenuManager.MenuClosed -= UnlockCamera;

        taskDialogManager.StrictDialogOpened -= LockCamera;
        taskDialogManager.StrictDialogClosed -= UnlockCamera;

        tutorialManager.TutorialSectionPresentationStarted -= LockCamera;
        tutorialManager.TutorialCompleted -= UnlockCamera;
    }

    private void Start()
    {
        if (GameManager.Instance.MapState > 0)
            NewAreaBounds(GameManager.Instance.MapState);
    }

    private void Update()
    {
        if (!isLocked)
        {
            panSpeed = currentZoom * ZOOM_PAN_RATIO;

            CameraPan();
            HandleEdgePan();
            CameraZoom();
            CameraRotation();
        }
    }

    // :::::::::: PUBLIC METHODS ::::::::::
    // ::::: Get & Set Zoom
    public float GetCurrentZoom() { return currentZoom; }
    public void SetZoom(float zoom)
    {
        currentZoom = Mathf.Clamp(zoom, minZoom, maxZoom);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, currentZoom, zoomSmoothness * Time.deltaTime);
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Pan
    private void CameraPan()
    {
        Vector2 panPosition = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        transform.position += Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f)
            * new Vector3(panPosition.x, 0f, panPosition.y) * (panSpeed * Time.deltaTime);

        transform.position = new Vector3(Mathf.Clamp(transform.position.x, panHorLimit.x, panHorLimit.y),
            transform.position.y, Mathf.Clamp(transform.position.z, panVertLimit.x, panVertLimit.y));
    }

    private void HandleEdgePan()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // UI

        float acceleration = 5f;
        float damping = 10f;
        Vector2 targetEdgeInput = Vector2.zero;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.y >= screenSize.y - EDGE_THRESHOLD) // Up
            targetEdgeInput.y = 1;
        if (mousePos.x <= EDGE_THRESHOLD)                // Left
            targetEdgeInput.x = -1;
        if (mousePos.y <= EDGE_THRESHOLD)                // Down
            targetEdgeInput.y = -1;
        if (mousePos.x >= screenSize.x - EDGE_THRESHOLD) // Right
            targetEdgeInput.x = 1;

        edgePanVelocity = Vector2.Lerp(edgePanVelocity, targetEdgeInput, acceleration * Time.deltaTime);

        if (edgePanVelocity != Vector2.zero)
        {
            transform.position += Quaternion.Euler(0f, mainCamera.transform.eulerAngles.y, 0f)
                * new Vector3(edgePanVelocity.x, 0f, edgePanVelocity.y) * (panSpeed * Time.deltaTime);

            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, panHorLimit.x, panHorLimit.y),
                transform.position.y, Mathf.Clamp(transform.position.z, panVertLimit.x, panVertLimit.y)
            );
        }

        if (targetEdgeInput == Vector2.zero)
            edgePanVelocity = Vector2.Lerp(edgePanVelocity, Vector2.zero, damping * Time.deltaTime);
    }

    // ::::: Zoom
    private void CameraZoom()
    {
        currentZoom = Mathf.Clamp(currentZoom - Input.mouseScrollDelta.y * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
        mainCamera.orthographicSize = Mathf.Lerp(mainCamera.orthographicSize, currentZoom, zoomSmoothness * Time.deltaTime);
    }

    // ::::: Rotation
    private void CameraRotation()
    {
        if (!isRotating)
            if (Input.GetKey(KeyCode.Q))
                StartCoroutine(SmoothRotate(45f));
            else if (Input.GetKey(KeyCode.E))
                StartCoroutine(SmoothRotate(-45f));
    }

    private IEnumerator SmoothRotate(float angle)
    {
        isRotating = true;

        Quaternion startRotation = transform.rotation;
        Quaternion endRotation = startRotation * Quaternion.Euler(0, angle, 0);

        float elapsed = 0f;
        while (elapsed < rotationDuration)
        {
            transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsed / rotationDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = endRotation;
        isRotating = false;
    }

    // ::::: Cinematic Camera Movement
    public IEnumerator MoveCamera(float duration, Vector3? position = null, Quaternion? rotation = null, float? zoom = null)
    {
        LockCamera();

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        float startZoom = currentZoom;

        for (float elapsedTime = 0f;  elapsedTime < duration; elapsedTime += Time.deltaTime)
        {
            float t = elapsedTime / duration;

            if (position.HasValue) transform.position = Vector3.Lerp(startPosition, position.Value, t);
            if (rotation.HasValue) transform.rotation = Quaternion.Slerp(startRotation, rotation.Value, t);
            if (zoom.HasValue) SetZoom(Mathf.Lerp(startZoom, zoom.Value, t));

            yield return null;
        }

        UnlockCamera();
    }

    // :::::::::: EVENT METHODS ::::::::::
    // ::::: Menu? Blocking
    private void LockCamera() { isLocked = true; }
    private void UnlockCamera() { if (!TutorialManager.Instance.isTutorialActive) isLocked = false; }

    // ::::: Expand Map When Advancing Map State
    private void NewAreaBounds(int newMapState)
    {
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minZ = float.MaxValue;
        float maxZ = float.MinValue;

        for (int i = 0; i <= newMapState; i++)
        {
            Bounds bounds = boundingPlanes[i].GetComponent<Renderer>().bounds;

            if (bounds.min.x < minX) minX = bounds.min.x;
            if (bounds.max.x > maxX) maxX = bounds.max.x;
            if (bounds.min.z < minZ) minZ = bounds.min.z;
            if (bounds.max.z > maxZ) maxZ = bounds.max.z;
        }

        panHorLimit = new Vector2(minX + EDGE_THRESHOLD, maxX);
        panVertLimit = new Vector2(minZ, maxZ - EDGE_THRESHOLD);
    }
}
