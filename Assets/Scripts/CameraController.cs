using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;
    [SerializeField] private GameObject boundingPlane;
    [SerializeField] private LaneConstructor laneConstructor;
    [SerializeField] private LaneDestructor laneDestructor;

    public bool lockCamera = false;

    // Cursor Movement
    public bool enableEdgeScrolling = true;
    public float edgeThreshold = 5f;
    private Vector2 screenSize;

    // WASD Movement
    public float movementSpeed;
    public float movementTime = 10f;
    private Vector3 newPosition;
    const float MIN_MOV_SPEED = 15;
    const float BASE_MOV_SPEED = 50f;
    const float MAX_MOV_SPEED = 60f;

    // Rotation
    public float rotationAmount = 0.2f;
    private Quaternion newRotation;
    private Vector3 rotateStartPosition;
    private Vector3 rotatePosition;

    // Zoom
    public Transform cameraTransform;
    public Vector3 zoomAmount;
    private Vector3 newZoom;

    // Limits
    private Bounds areaBounds;
    private float minZoomHeight;
    private float midZoomHeight;
    private float maxZoomHeight;

    // === Methods ===
    private void Start()
    {
        movementSpeed = BASE_MOV_SPEED;

        areaBounds = boundingPlane.GetComponent<Renderer>().bounds;
        minZoomHeight = areaBounds.min.y + 100f;
        maxZoomHeight = areaBounds.max.y + 300f;
        midZoomHeight = (minZoomHeight + maxZoomHeight) / 2f;

        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;

        screenSize = new Vector2(Screen.width, Screen.height);
    }

    private void OnEnable()
    {
        laneConstructor.OnLaneBuilt += LockCamera;
        laneConstructor.OnLaneFinished += UnlockCamera;

        laneDestructor.OnLaneDestroyed += LockCamera;
        laneDestructor.OnLaneFinished += UnlockCamera;
    }
    private void OnDisable()
    {
        laneConstructor.OnLaneBuilt -= LockCamera;
        laneConstructor.OnLaneFinished -= UnlockCamera;

        laneDestructor.OnLaneDestroyed -= LockCamera;
        laneDestructor.OnLaneFinished -= UnlockCamera;
    }

    private void LateUpdate()
    {
        if (!lockCamera)
        {
            HandleMouseInput();
            HandleMovementInput();
            HandleEdgeScrolling();
        }

        // Apply
        float clampedX = Mathf.Clamp(newPosition.x, areaBounds.min.x, areaBounds.max.x);
        float clampedZ = Mathf.Clamp(newPosition.z, areaBounds.min.z, areaBounds.max.z);
        newPosition = new Vector3(clampedX, newPosition.y, clampedZ);
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);

        float clampedZoomY = Mathf.Clamp(newZoom.y, minZoomHeight, maxZoomHeight);
        newZoom = new Vector3(0f, clampedZoomY, newZoom.z);
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    private void LockCamera(Vector2Int gridPosition) { lockCamera = true; }
    private void UnlockCamera(Vector2Int gridPosition) { lockCamera = false; }

    private void HandleEdgeScrolling()
    {
        if (!enableEdgeScrolling) return;
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return; // UI

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
            newPosition += edgeDirection * movementSpeed * Time.deltaTime;
        }
    }

    // Camera Control w/Mouse
    private void HandleMouseInput()
    { //                       Zoom <¬
        if (Input.mouseScrollDelta.y != 0)
        {
            newZoom += Input.mouseScrollDelta.y * zoomAmount;

            // Zoom : MovSpeed
            float zoomFactor = Mathf.InverseLerp(minZoomHeight, maxZoomHeight, newZoom.y);
            if (newZoom.y < midZoomHeight)
                movementSpeed = Mathf.Lerp(MIN_MOV_SPEED, BASE_MOV_SPEED, zoomFactor * 2f);
            else
                movementSpeed = Mathf.Lerp(BASE_MOV_SPEED, MAX_MOV_SPEED, (zoomFactor - 0.5f) * 2f);
        }

        // Self Rotate
        if (Input.GetMouseButtonDown(2))
        {
            enableEdgeScrolling = false;
            rotateStartPosition = Input.mousePosition;
        }
            

        if (Input.GetMouseButton(2))
        {
            rotatePosition = Input.mousePosition;
            Vector3 difference = rotateStartPosition - rotatePosition;
            rotateStartPosition = rotatePosition;
            newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        }

        if (Input.GetMouseButtonUp(2))
            enableEdgeScrolling = true;
    }

    // Camera Control w/Keyboard
    private void HandleMovementInput()
    {
        // Horizontal and Vertical Movement
        Vector3 direction = Vector3.zero;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            direction += transform.forward;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            direction -= transform.right;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            direction -= transform.forward;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            direction += transform.right;

        if (direction.magnitude > 1)
            direction.Normalize();
        newPosition += direction * movementSpeed * Time.deltaTime;

        // 90° Rotations
        if (Input.GetKey(KeyCode.Q))
            newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        if (Input.GetKey(KeyCode.E))
            newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
    }
}