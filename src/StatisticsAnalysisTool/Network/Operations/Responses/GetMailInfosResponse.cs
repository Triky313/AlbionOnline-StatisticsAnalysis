using Serilog;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Trade.Mails;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StatisticsAnalysisTool.Network.Operations.Responses;

public class GetMailInfosResponse
{
    public readonly List<MailNetworkObject> MailInfos = [];

    public GetMailInfosResponse(Dictionary<byte, object> parameters)
    {
        MailInfos.Clear();

        try
        {
            if (!parameters.ContainsKey(0) || parameters[0] == null ||
                !parameters.ContainsKey(3) || parameters[3] == null ||
                !parameters.ContainsKey(7) || parameters[7] == null ||
                !parameters.ContainsKey(11) || parameters[11] == null ||
                !parameters.ContainsKey(12) || parameters[12] == null)
            {
                return;
            }

            var guid = parameters[0].ObjectToGuid();

            if (!TryGetLongArray(parameters[3], out var mailIdArray)
                || !TryGetStringArray(parameters[7], out var subjectArray)
                || !TryGetStringArray(parameters[11], out var mailTypeTextArray)
                || !TryGetLongArray(parameters[12], out var timeStampArray))
            {
                return;
            }

            if (mailIdArray is not { Length: > 0 })
            {
                return;
            }

            var length = new[] { mailIdArray.Length, subjectArray.Length, mailTypeTextArray.Length, timeStampArray.Length }.Min();

            for (var i = 0; i < length; i++)
            {
                var mailId = mailIdArray[i];
                var subject = subjectArray[i];
                var mailTypeText = mailTypeTextArray[i];
                var timeStamp = timeStampArray[i];

                MailInfos.Add(new MailNetworkObject(guid, mailId, subject, mailTypeText, timeStamp));
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "{message}", nameof(GetMailInfosResponse));
        }
    }

    private static bool TryGetLongArray(object value, out long[] result)
    {
        result = value switch
        {
            long[] longArray => longArray,
            int[] intArray => Array.ConvertAll(intArray, x => (long) x),
            _ => []
        };

        return result.Length > 0;
    }

    private static bool TryGetStringArray(object value, out string[] result)
    {
        result = value as string[] ?? [];
        return result.Length > 0;
    }
}
