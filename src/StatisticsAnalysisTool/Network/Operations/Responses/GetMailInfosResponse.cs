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
                !parameters[3].GetType().IsArray ||
                (typeof(long[]).Name != parameters[3].GetType().Name &&
                 typeof(int[]).Name != parameters[3].GetType().Name))
            {
                return;
            }

            var guid = parameters[0].ObjectToGuid();

            long[] mailIdArray = [];

            // If the mails ID's are ever below 32.767, an error will appear here, but this should not happen on the current west and east servers, since the mail ID is above it and can never come below it again.
            if (typeof(int[]).Name == parameters[3].GetType().Name)
            {
                mailIdArray = Array.ConvertAll((int[]) parameters[3], x => (long) x);
            }
            else if (typeof(long[]).Name == parameters[3].GetType().Name)
            {
                mailIdArray = ((long[]) parameters[3]).ToArray();
            }

            if (mailIdArray is not { Length: > 0 })
            {
                return;
            }

            var subjectArray = ((string[]) parameters[7]).ToArray();
            var mailTypeTextArray = ((string[]) parameters[11]).ToArray();
            var timeStampArray = ((long[]) parameters[12]).ToArray();

            var length = Utilities.GetHighestLength(mailIdArray, subjectArray, mailTypeTextArray);

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
            Log.Error(nameof(GetMailInfosResponse), e);
        }
    }
}