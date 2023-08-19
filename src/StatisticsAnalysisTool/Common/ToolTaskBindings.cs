using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System.Collections.ObjectModel;
using System.Linq;

namespace StatisticsAnalysisTool.Common;

public class ToolTaskBindings : BaseViewModel
{
    private ObservableCollection<TaskTextObject> _toolTaskObjects = new();
    private const int RemoveOn = 10;

    public ObservableCollection<TaskTextObject> ToolTaskObjects
    {
        get => _toolTaskObjects;
        set
        {
            _toolTaskObjects = value;
            OnPropertyChanged();
        }
    }

    public void Add(TaskTextObject taskTextObject)
    {
        ToolTaskObjects.Insert(0, taskTextObject);
        RemoveIfThereAreTooManyObjects();
    }

    public void Remove(TaskTextObject taskTextObject)
    {
        ToolTaskObjects.Remove(taskTextObject);
    }

    private void RemoveIfThereAreTooManyObjects()
    {
        foreach (var item in ToolTaskObjects.OrderByDescending(x => x.CreateAt))
        {
            if (ToolTaskObjects.Count > RemoveOn)
            {
                ToolTaskObjects.Remove(item);
            }
        }
    }
}