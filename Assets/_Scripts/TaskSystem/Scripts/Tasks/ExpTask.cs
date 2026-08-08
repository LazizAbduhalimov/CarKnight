using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Tasks/ExpTask")]
public class ExpTask : TaskSO
{
    private void ExpCollected(int amount)
    {
        taskProgress = Mathf.Min(taskProgress + amount, taskMaxProgress);
        UpdateTask();
    }

    public override void UpdateTask()
    {
        if (taskProgress >= taskMaxProgress)
        {
            CompleteTask();
        }
        else
        {
            NotifyUI();
        }
    }

    public override void CompleteTask()
    {
        base.CompleteTask();
        gameEvents.ExpCollected -= ExpCollected;
        taskManager.UpdateTasks();
        NotifyUI();
    }

    public override void StartTask(GameEvents events, TaskManager taskManager)
    {
        base.StartTask(events, taskManager);
        gameEvents.ExpCollected += ExpCollected;
    }
}

