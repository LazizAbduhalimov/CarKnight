using System;
using UnityEngine;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour
{
    [SerializeField] private Image _panelImage;
    [SerializeField] private Image _iconImage;

    [SerializeField] private Text _nameText;
    [SerializeField] private Text _descriptionText;

    [SerializeField] private Text _progressText;
    [SerializeField] private Image _progressImage;

    private TaskSO _task;



    public void Bind(TaskSO task)
    {
        if (_task == task)
            return;

        if (_task != null)
        {
            _task.OnTaskUpdated -= UpdateTask;
        }

        _task = task;
        _task.OnTaskUpdated += UpdateTask;

        UpdateTask(task);
    }

    public void UnBind()
    {
        if (_task != null)
        {
            _task.OnTaskUpdated -= UpdateTask;
        }
        _task = null;
    }

    public void UpdateTask(TaskSO task)
    {

        Debug.Log($"{gameObject.name} task updated by {task.name}");

        _panelImage.color = task.color;

        if (task.icon == null)
        {
            _iconImage.gameObject.SetActive(false);
        }
        else
        {
            _iconImage.gameObject.SetActive(true);
            _iconImage.sprite = task.icon;
        }

        _nameText.text = task.taskName;
        _descriptionText.text = task.taskDescription;

        var progreesBar = CalculateProgress(task.taskProgress, task.taskMaxProgress);
        _progressImage.fillAmount = progreesBar.Item2;
        _progressText.text = progreesBar.Item1;
    }

    private (string, float) CalculateProgress(int taskProgress, int taskMaxProgress)
    {
        string progressString = $"{taskProgress} / {taskMaxProgress}";

        float progress = taskMaxProgress == 0
    ? 0f
    : (float)taskProgress / taskMaxProgress;

        return (progressString, progress);
    }
    private void OnDestroy()
    {
        UnBind();
    }
}
