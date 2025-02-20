using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Splines;

public class IsometricCameraController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private GameObject boundingPlane;

    [Header("Variables - Pan")]
    public float panSpeed = 25f;

    [Header("Variables - Zoom")]
    public float zoomSpeed = 200;
    public float zoomSmoothness = 50;

    [Header("Variables - Rotation")]
    public float rotationDuration = 0.25f;

    private bool isLocked = false;
    private bool isRotating = false;
    private Bounds areaBounds;
    private float currentZoom;
    private float minZoom, maxZoom;
    private Vector2 panHorLimit, panVertLimit;
    private Vector2 screenSize;

    const float ZOOM_PAN_RATIO = 2f;
    const float ZOOM_OFFSET = 4f;
    const float EDGE_THRESHOLD = 5f;

    // :::::::::: MONO METHODS ::::::::::
    private void Start()
    {
        areaBounds = boundingPlane.GetComponent<Renderer>().bounds;

        minZoom = areaBounds.min.y + ZOOM_OFFSET;
        maxZoom = areaBounds.max.y + 3 * ZOOM_OFFSET;
        currentZoom = (maxZoom + minZoom) / 2;

        panHorLimit = new Vector2(areaBounds.min.x, areaBounds.max.x);
        panVertLimit = new Vector2(areaBounds.min.z - 10f, areaBounds.max.z);

        screenSize = new Vector2(Screen.width, Screen.height);
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

        float edgeThreshold = EDGE_THRESHOLD;
        Vector3 edgeDirection = Vector3.zero;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.y >= screenSize.y - edgeThreshold) // Up
            edgeDirection += transform.forward;
        if (mousePos.x <= edgeThreshold)                // Left
            edgeDirection -= transform.right;
        if (mousePos.y <= edgeThreshold)                // Down
            edgeDirection -= transform.forward;
        if (mousePos.x >= screenSize.x - edgeThreshold) // Right
            edgeDirection += transform.right;

        if (edgeDirection != Vector3.zero)
        {
            edgeDirection.Normalize();
            transform.position += edgeDirection * (panSpeed * Time.deltaTime);

            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, panHorLimit.x, panHorLimit.y),
                transform.position.y, Mathf.Clamp(transform.position.z, panVertLimit.x, panVertLimit.y)
            );
        }
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
            if (Input.GetKeyDown(KeyCode.Q))
                StartCoroutine(SmoothRotate(45f));
            else if (Input.GetKeyDown(KeyCode.E))
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
}
