using System;
using System.Net;

namespace StatisticsAnalysisTool.Common
{
    public class WebDownload : WebClient
    {
        public WebDownload() : this(30000)
        {
        }

        public WebDownload(int timeout)
        {
            Timeout = timeout;
        }

        /// <summary>
        ///     Time in milliseconds
        /// </summary>
        public int Timeout { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = base.GetWebRequest(address);
            if (request != null)
                request.Timeout = Timeout;

            return request;
        }
    }
}