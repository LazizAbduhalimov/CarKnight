using System;
using UnityEngine;

public abstract class TaskSO : ScriptableObject
{
    public string taskName;
    [Tooltip("Task description that will be visible on table")]
    public string taskDescription;

    [Tooltip("Color that will be shown on task panel")]
    public Color color;

    [Tooltip("Image that will be shown as the task icon")]
    public Sprite icon;

    [Tooltip("Max progress value")]
    public int taskMaxProgress;

    [HideInInspector] public int taskProgress;
    public bool IsCompleted;

    protected GameEvents gameEvents;
    protected TaskManager taskManager;

    public event Action<TaskSO> OnTaskUpdated;

    public virtual void StartTask(GameEvents events, TaskManager manager)
    {
        gameEvents = events;
        taskManager = manager;
    }

    public abstract void UpdateTask();

    public virtual void CompleteTask()
    {
        IsCompleted = true;
    }

    protected void NotifyUI()
    {
        OnTaskUpdated?.Invoke(this);
    }
}