using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class ReadMailResponse
{
    public long MailId;
    public string Content;

    public ReadMailResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            if (parameters.ContainsKey(0))
            {
                MailId = parameters[0].ObjectToLong() ?? -1;
            }

            if (parameters.ContainsKey(1))
            {
                Content = string.IsNullOrEmpty(parameters[1].ToString()) ? string.Empty : parameters[1].ToString();
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}