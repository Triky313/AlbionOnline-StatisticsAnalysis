using StatisticsAnalysisTool.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Events;

public class TestEvent2
{
    public TestEvent2(Dictionary<byte, object> parameters)
    {
        Debug.Print("----- NewMob (Events) -----");

        try
        {
            foreach (var parameter in parameters) Debug.Print($"{parameter}");
        }
        catch (Exception e)
        {
            ConsoleManager.WriteLineForError(MethodBase.GetCurrentMethod()?.DeclaringType, e);
        }
    }
}