using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.ViewModels;
using System.Linq;

namespace StatisticsAnalysisTool.Common;

public static class ToolTaskController
{
    private const int RemoveOn = 10;
    private static MainWindowViewModel _mainWindowViewModel;

    public static void SetToolTaskController(MainWindowViewModel mainWindowViewModel)
    {
        _mainWindowViewModel = mainWindowViewModel;
    }

    public static void Add(TaskTextObject taskTextObject)
    {
        _mainWindowViewModel.ToolTaskObjects.Insert(0, taskTextObject);
        RemoveIfThereAreTooManyObjects();
    }

    public static void Remove(TaskTextObject taskTextObject)
    {
        _mainWindowViewModel.ToolTaskObjects.Remove(taskTextObject);
    }

    private static void RemoveIfThereAreTooManyObjects()
    {
        foreach (var item in _mainWindowViewModel.ToolTaskObjects.OrderByDescending(x => x.CreateAt))
        {
            if (_mainWindowViewModel.ToolTaskObjects.Count > RemoveOn)
            {
                _mainWindowViewModel.ToolTaskObjects.Remove(item);
            }
        }
    }
}