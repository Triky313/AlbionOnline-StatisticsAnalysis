﻿using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class RegisterToObjectRequest
{
    private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

    public long BuildingObjectId;

    public RegisterToObjectRequest(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            if (parameters.ContainsKey(0))
            {
                BuildingObjectId = parameters[0].ObjectToLong() ?? -1;
            }
        }
        catch (Exception e)
        {
            Log.Error(nameof(ReadMailResponse), e);
        }
    }
}