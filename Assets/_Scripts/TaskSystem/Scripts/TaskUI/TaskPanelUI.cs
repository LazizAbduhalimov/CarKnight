using System.Collections.Generic;
using UnityEngine;

public class TaskPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject taskPref;
    [SerializeField] private List<TaskUI> _tasks = new List<TaskUI>();





    public void UpdateTasks(List<TaskSO> tasks)
    {
        int currentCount = _tasks.Count;
        int newCount = tasks.Count;

        for (int i = 0; i < newCount; i++)
        {
            if (i < currentCount)
            {
                _tasks[i].gameObject.SetActive(true);
            }
            else
            {

                var ui = CreateTaskButton();
                _tasks.Add(ui);
            }

            _tasks[i].Bind(tasks[i]);
        }

        for (int i = newCount; i < currentCount; i++)
        {
            _tasks[i].UnBind();
            _tasks[i].gameObject.SetActive(false);
        }
    }

    private TaskUI CreateTaskButton()
    {
        return Instantiate(taskPref, transform).GetComponent<TaskUI>();
    }
}
