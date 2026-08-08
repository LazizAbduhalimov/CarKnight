using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Tasks/EnemyTypeTask")]
public class EnemyTypeTask : TaskSO
{
    public long EnemyTypeID;
    private void EnemyKilled(long intID)
    {
        if (intID == EnemyTypeID)
        {
            taskProgress++;
            UpdateTask();
        }

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
        gameEvents.EnemyKilled -= EnemyKilled;

        taskManager.UpdateTasks();
        NotifyUI();
    }

    public override void StartTask(GameEvents events, TaskManager taskManager)
    {
        base.StartTask(events, taskManager);
        gameEvents.EnemyKilled += EnemyKilled;
    }
}

