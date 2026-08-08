using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class TaskManager : MonoBehaviour
{
    [SerializeField]private List<TaskSO> _activeTasks = new();
    [SerializeField]private List<TaskSO> _completedTasks = new();

    private GameEvents _events;
    private TaskPanelUI _taskUI;

    [Inject]private void Initialize(GameEvents events, TaskPanelUI taskUI)
    {
        _events = events;
        _taskUI = taskUI;

        for (int i = 0; i < _activeTasks.Count; i++)
        {
            var task = Instantiate(_activeTasks[i]);
            _activeTasks[i] = task;
            _activeTasks[i].StartTask(_events, this);
        }

        _taskUI.UpdateTasks(_activeTasks);
    }
    public void AddTask(TaskSO task)
    {
        var newTask = Instantiate(task);
        _activeTasks.Add(newTask);

        newTask.StartTask(_events, this);

        _taskUI.UpdateTasks(_activeTasks);
    }

    public void UpdateTasks()
    {
        for (int i = _activeTasks.Count - 1; i >= 0; i--)
        {
            var task = _activeTasks[i];

            if (task.IsCompleted)
            {
                _activeTasks.RemoveAt(i);
                _completedTasks.Add(task);
            }
        }
        _taskUI.UpdateTasks(_activeTasks);
    }
}
