using System;
using Baseline.Dates;

namespace Jasper.Http
{
    public class HttpTransportSettings : IHttpTransportConfiguration
    {
        public TimeSpan ConnectionTimeout { get; set; } = 10.Seconds();
        public string RelativeUrl { get; set; } = "messages";


        public bool EnableMessageTransport { get; set; }

        IHttpTransportConfiguration IHttpTransportConfiguration.EnableListening(bool enabled)
        {
            EnableMessageTransport = enabled;
            return this;
        }

        IHttpTransportConfiguration IHttpTransportConfiguration.RelativeUrl(string url)
        {
            RelativeUrl = url;
            return this;
        }

        IHttpTransportConfiguration IHttpTransportConfiguration.ConnectionTimeout(TimeSpan span)
        {
            ConnectionTimeout = span;
            return this;
        }
    }
}