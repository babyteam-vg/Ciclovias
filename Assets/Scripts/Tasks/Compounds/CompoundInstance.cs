using UnityEngine;

public class CompoundInstance : MonoBehaviour
{
    [SerializeField] private Compound compound;
    [SerializeField] private GameObject taskIconPrefab;

    private GameObject taskIconInstance;

    // :::::::::: MONO METHODS ::::::::::
    private void Start()
    {
        if (compound != null)
        {
            compound.OnTaskUnlocked += UpdateTaskIcon;
            UpdateTaskIcon();
        }
    }

    private void OnDestroy()
    {
        if (compound != null)
            compound.OnTaskUnlocked -= UpdateTaskIcon;
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Click on Compound (Mesh)
    private void OnMouseDown()
    {
        if (!IsPointerOverUI())
            compound.OnPlayerInteract();
    }

    // ::::: Prevent UI Interference
    private bool IsPointerOverUI() { return UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(); }

    // ::::: Handle Task Icon Visibility
    private void UpdateTaskIcon()
    {
        if (taskIconPrefab == null) return;

        if (compound.IsGivingTask())
        {
            if (taskIconInstance == null)
            {
                taskIconInstance = Instantiate(taskIconPrefab, transform);
                taskIconInstance.transform.localPosition = new Vector3(0, 4f, 0); // Ajustar altura sobre el Compound
            }
            taskIconInstance.SetActive(true);
        }
        else
        {
            if (taskIconInstance != null)
                taskIconInstance.SetActive(false);
        }
    }
}
