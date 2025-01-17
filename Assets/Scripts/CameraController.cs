using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private InputManager inputManager;

    // Mouse Dragging
    private Vector3 dragStartPosition;
    private Vector3 dragPosition;

    // WASD Movement
    public float normalMovementSpeed = 0.2f;
    public float fastMovementSpeed = 1.0f;
    private float movementSpeed;
    private float movementTime = 5f;
    private Vector3 newPosition;

    // Rotation
    public float rotationAmount = 1f;
    private Quaternion newRotation;
    private Vector3 rotateStartPosition;
    private Vector3 rotatePosition;

    // Zoom
    public Transform cameraTransform;
    public Vector3 zoomAmount;
    private Vector3 newZoom;

    // Limits
    public float minPanX = -20f;
    public float maxPanX = 20f;
    public float minPanZ = -10f;
    public float maxPanZ = 10f;

    public float minZoomY = 20f;
    public float maxZoomY = 140f;
    public float minZoomZ = -8f;
    public float maxZoomZ = 4f;

    // === Methods ===
    private void Start()
    {
        newPosition = transform.position;
        newRotation = transform.rotation;
        newZoom = cameraTransform.localPosition;
    }

    private void LateUpdate()
    {
        HandleMouseInput();
        HandleMovementInput();

        // Apply
        newPosition = new Vector3(Mathf.Clamp(newPosition.x, minPanX, maxPanX), 0.1f, Mathf.Clamp(newPosition.z, minPanZ, maxPanZ));
        transform.position = Vector3.Lerp(transform.position, newPosition, Time.deltaTime * movementTime);

        transform.rotation = Quaternion.Lerp(transform.rotation, newRotation, Time.deltaTime * movementTime);

        newZoom = new Vector3(0f, Mathf.Clamp(newZoom.y, minZoomY, maxZoomY), Mathf.Clamp(newZoom.z, minZoomZ, maxZoomZ));
        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, newZoom, Time.deltaTime * movementTime);
    }

    // Camera Control w/Mouse
    private void HandleMouseInput()
    {
        if (Input.mouseScrollDelta.y != 0) // Zoom
            newZoom += Input.mouseScrollDelta.y * zoomAmount;

        // Drag
        if (Input.GetMouseButtonDown(2))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float entry))
                dragStartPosition = ray.GetPoint(entry);
        }

        if (Input.GetMouseButton(2))
        {
            Plane plane = new Plane(Vector3.up, Vector3.zero);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (plane.Raycast(ray, out float entry))
            {
                dragPosition = ray.GetPoint(entry);
                newPosition = transform.position + dragStartPosition - dragPosition;
            }
        }

        // Self Rotate
        //if (Input.GetMouseButtonDown(2))
        //    rotateStartPosition = Input.mousePosition;

        //if(Input.GetMouseButton(2))
        //{
        //    rotatePosition = Input.mousePosition;

        //    Vector3 difference = rotateStartPosition - rotatePosition;
        //    rotateStartPosition = rotatePosition;

        //    newRotation *= Quaternion.Euler(Vector3.up * (-difference.x / 5f));
        //}               
    }

    // Camera Control w/Keyboard
    private void HandleMovementInput()
    {
        if (Input.GetKey(KeyCode.LeftShift))
            movementSpeed = fastMovementSpeed;
        else movementSpeed = normalMovementSpeed;

        // Horizontal and Vertical Movement
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            newPosition += (transform.forward * movementSpeed);
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            newPosition += (transform.right * -movementSpeed);
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            newPosition += (transform.forward * -movementSpeed);
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            newPosition += (transform.right * movementSpeed);

        // 90° Rotations
        //if (Input.GetKey(KeyCode.Q))
        //    newRotation *= Quaternion.Euler(Vector3.up * rotationAmount);
        //if (Input.GetKey(KeyCode.E))
        //    newRotation *= Quaternion.Euler(Vector3.up * -rotationAmount);
    }
}