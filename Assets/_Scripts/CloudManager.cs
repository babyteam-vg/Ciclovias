using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameManager gameManager;

    [Header("World References")]
    public GameObject clouds;
    public List<GameObject> cloudState;

    private Vector3 initialCloudsPosition;
    private Coroutine currentCloudMovement;

    // :::::::::: MONO METHODS ::::::::::
    private void OnEnable()
    {
        gameManager.MapStateAdvanced += DeactivateClouds;
    }
    private void OnDisable()
    {
        gameManager.MapStateAdvanced -= DeactivateClouds;
    }

    private void Start()
    {
        initialCloudsPosition = clouds.transform.position;
        DeactivateClouds(GameManager.Instance.MapState);
    }

    private void Update()
    {
        if (currentCloudMovement == null)
        {
            float duration = Random.Range(3f, 6f);

            Vector3 movements = new Vector3(
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f),
                Random.Range(-1f, 1f)
            );

            currentCloudMovement = StartCoroutine(CloudMovement(movements, duration));
        }
    }

    // :::::::::: PRIVATE METHODS ::::::::::
    // ::::: Move the Cloud in the World Map
    private IEnumerator CloudMovement(Vector3 movements, float duration)
    {
        Vector3 startPos = clouds.transform.position;
        Vector3 rawEndPos = startPos + movements;
        Vector3 endPos = new Vector3(
            Mathf.Clamp(rawEndPos.x, initialCloudsPosition.x - 2f, initialCloudsPosition.x + 2f),
            Mathf.Clamp(rawEndPos.y, initialCloudsPosition.y - 1f, initialCloudsPosition.y + 1f),
            Mathf.Clamp(rawEndPos.z, initialCloudsPosition.z - 2f, initialCloudsPosition.z + 2f)
        );

        float elapsed = 0f;
        while (elapsed < duration)
        {
            clouds.transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        clouds.transform.position = endPos;
        currentCloudMovement = null;
    }

    private void DeactivateClouds(int newMapState)
    {
        if (newMapState > 4)
            return;

        for(int i = 0; i < newMapState; i++)
            cloudState[i].SetActive(false);
    }
}
