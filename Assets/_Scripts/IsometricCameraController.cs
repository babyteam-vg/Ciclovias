using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private bool isCameraLocked = false;
    private bool isRotating = false;
    private Bounds areaBounds;
    private float currentZoom;
    private float minZoom, maxZoom;
    private Vector2 panHorLimit, panVertLimit;

    const float ZOOM_PAN_RATIO = 2f;
    const float ZOOM_OFFSET = 3f;

    // :::::::::: MONO METHODS ::::::::::
    private void Start()
    {
        areaBounds = boundingPlane.GetComponent<Renderer>().bounds;

        minZoom = areaBounds.min.y + ZOOM_OFFSET;
        maxZoom = areaBounds.max.y + 5 * ZOOM_OFFSET;

        panHorLimit = new Vector2(areaBounds.min.x, areaBounds.max.x);
        panVertLimit = new Vector2(areaBounds.min.z, areaBounds.max.z);
    }

    private void Update()
    {
        if (!isCameraLocked)
        {
            panSpeed = currentZoom * ZOOM_PAN_RATIO;

            CameraPan();
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
