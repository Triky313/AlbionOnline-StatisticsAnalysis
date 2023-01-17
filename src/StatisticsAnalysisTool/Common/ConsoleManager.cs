using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace StatisticsAnalysisTool.Common;

public static class ConsoleManager
{
    private const int MaxEntries = 1000;
    public static ObservableCollectionEx<ConsoleFragment> Console = new();

    private static bool _isConsoleActive;

    public static void Start()
    {
        _isConsoleActive = true;
    }

    public static void Stop()
    {
        _isConsoleActive = false;
    }

    public static void Reset()
    {
        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            lock (Console)
            {
                Console.Clear();
            }
        });
    }

    public static void WriteLine(ConsoleFragment consoleFragment)
    {
        if (!_isConsoleActive)
        {
            return;
        }

        Application.Current.Dispatcher.InvokeAsync(() =>
        {
            lock (Console)
            {
                Console.Add(consoleFragment);

                if (Console.Count <= MaxEntries)
                {
                    return;
                }

                var firstItem = Console.FirstOrDefault();
                Console.Remove(firstItem);
            }
        });
    }

    public static void WriteLineForNetworkHandler(string name, Dictionary<byte, object> parameters)
    {
        WriteLine(new ConsoleFragment(name, parameters, ConsoleColorType.EventColor));
    }

    public static void WriteLineForWarning(Type declaringType, Exception e)
    {
        WriteLine(new ConsoleFragment(declaringType.ToString(), e.Message, ConsoleColorType.WarnColor));
    }

    public static void WriteLineForError(Type declaringType, Exception e)
    {
        WriteLine(new ConsoleFragment(declaringType.ToString(), e.Message, ConsoleColorType.ErrorColor));
        WriteLine(new ConsoleFragment(string.Empty, e.StackTrace, ConsoleColorType.ErrorColor));
    }

    public static void WriteLineForMessage(Type declaringType, string message, ConsoleColorType consoleColorType = ConsoleColorType.Default)
    {
        WriteLine(new ConsoleFragment(declaringType.ToString(), message, consoleColorType));
    }

    public static void WriteLineForMessage(string name, Dictionary<byte, object> parameters, ConsoleColorType consoleColorType = ConsoleColorType.Default)
    {
        WriteLine(new ConsoleFragment(name, parameters, consoleColorType));
    }
}