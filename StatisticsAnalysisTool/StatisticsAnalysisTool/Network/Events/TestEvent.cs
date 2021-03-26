using System;
using System.Collections.Generic;
using System.Diagnostics;
using Albion.Network;

namespace StatisticsAnalysisTool.Network.Events
{
    public class TestEvent : BaseOperation
    {
        public string ClusterMap;

        public string ClusterName;

        public string ClusterOwner;

        public TestEvent(Dictionary<byte, object> parameters) : base(parameters)
        {
            Debug.Print("----- ChangeCluster -----");
            try
            {
                foreach (var parameter in parameters) Debug.Print($"{parameter}");

                if (parameters.ContainsKey(0)) ClusterName = string.IsNullOrEmpty(parameters[0].ToString()) ? string.Empty : parameters[0].ToString();

                if (parameters.ContainsKey(255))
                    ClusterMap = string.IsNullOrEmpty(parameters[255].ToString()) ? string.Empty : parameters[255].ToString();

                if (parameters.ContainsKey(253))
                    ClusterOwner = string.IsNullOrEmpty(parameters[253].ToString()) ? string.Empty : parameters[253].ToString();
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
            }
        }
    }
}