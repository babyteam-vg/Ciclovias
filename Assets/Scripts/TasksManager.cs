using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TaskManager : MonoBehaviour
{
    [SerializeField] private Grid grid;
    public Task task;


}

[System.Serializable]
public class Task
{
    [Header("Bools")]
    public bool active;
    public bool completed;
    public bool locked;

    [Header("Info")]
    public string title;
    public string description;
    public Vector2Int cellStart;
    public Vector2Int cellDestination;
}