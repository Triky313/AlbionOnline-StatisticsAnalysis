using Albion.Network;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace StatisticsAnalysisTool.Network.Events
{
    public class TestEvent3 : BaseOperation
    {
        public TestEvent3(Dictionary<byte, object> parameters) : base(parameters)
        {
            Debug.Print($"----- Join (Operations) -----");

            try
            {
                foreach (var parameter in parameters)
                {
                    Debug.Print($"{parameter}");
                }
            }
            catch(Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}