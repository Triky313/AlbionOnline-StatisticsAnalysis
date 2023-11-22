using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class GetGuildAccountLogsResponse
{
    public readonly List<string> Usernames = new();
    public readonly List<FixPoint> Quantities = new();
    public readonly List<long> Timestamps = new();

    public GetGuildAccountLogsResponse(Dictionary<byte, object> parameters)
    {
        ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

        try
        {
            if (parameters.TryGetValue(0, out object usernames))
            {
                foreach (var username in (IEnumerable<string>) usernames ?? new List<string>())
                {
                    Usernames.Insert(0, username);
                }
            }

            if (parameters.TryGetValue(3, out object quantities))
            {
                var valueType = parameters[3].GetType();
                switch (valueType.IsArray)
                {
                    case true when typeof(byte[]).Name == valueType.Name:
                        {
                            foreach (var quantity in (IEnumerable<byte>) quantities ?? new List<byte>())
                            {
                                Quantities.Insert(0, FixPoint.FromInternalValue(quantity));
                            }
                            break;
                        }
                    case true when typeof(short[]).Name == valueType.Name:
                        {
                            foreach (var quantity in (IEnumerable<short>) quantities ?? new List<short>())
                            {
                                Quantities.Insert(0, FixPoint.FromInternalValue(quantity));
                            }
                            break;
                        }
                    case true when typeof(int[]).Name == valueType.Name:
                        {
                            foreach (var quantity in (IEnumerable<int>) quantities ?? new List<int>())
                            {
                                Quantities.Insert(0, FixPoint.FromInternalValue(quantity));
                            }
                            break;
                        }
                    case true when typeof(long[]).Name == valueType.Name:
                        {
                            foreach (var quantity in (IEnumerable<long>) quantities ?? new List<long>())
                            {
                                Quantities.Insert(0, FixPoint.FromInternalValue(quantity));
                            }
                            break;
                        }
                }
            }

            if (parameters.TryGetValue(4, out object timestamps))
            {
                foreach (var timestamp in (IEnumerable<long>) timestamps ?? new List<long>())
                {
                    Timestamps.Insert(0, timestamp);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", MethodBase.GetCurrentMethod()?.DeclaringType);
        }
    }
}