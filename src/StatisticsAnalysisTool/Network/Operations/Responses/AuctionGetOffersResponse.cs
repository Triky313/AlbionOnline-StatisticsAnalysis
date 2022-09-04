using log4net;
using StatisticsAnalysisTool.Common;
using StatisticsAnalysisTool.Enumerations;
using StatisticsAnalysisTool.Models;
using StatisticsAnalysisTool.Models.NetworkModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace StatisticsAnalysisTool.Network.Operations.Responses
{
    public class AuctionGetOffersResponse
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public List<AuctionGetOffer> AuctionGetOffers = new ();

        public AuctionGetOffersResponse(Dictionary<byte, object> parameters)
        {
            ConsoleManager.WriteLine(new ConsoleFragment(GetType().Name, parameters, ConsoleColorType.EventColor));

            try
            {
                if (parameters.ContainsKey(0) && parameters[0] != null)
                {
                    var valueType = parameters[0].GetType();
                    if (valueType.IsArray && typeof(string[]) == valueType)
                    {
                        var stringArray = ((string[])parameters[0]).ToDictionary();

                        foreach (var arrayElement in stringArray)
                        {
                            var auctionGetOffer = JsonSerializer.Deserialize<AuctionGetOffer>(arrayElement.Value ?? string.Empty);

                            if (auctionGetOffer == null)
                            {
                                continue;
                            }
                            AuctionGetOffers.Add(auctionGetOffer);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(nameof(AuctionGetOffersResponse), e);
            }
        }
    }
}