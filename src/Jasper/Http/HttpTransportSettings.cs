using System;
using Baseline.Dates;
using Jasper.Messaging.Transports.Configuration;

namespace Jasper.Http
{
    public class HttpTransportSettings : IHttpTransportConfiguration
    {
        private readonly MessagingSettings _parent;

        public HttpTransportSettings(MessagingSettings parent)
        {
            _parent = parent;
        }

        public TimeSpan ConnectionTimeout { get; set; } = 10.Seconds();
        public string RelativeUrl { get; set; } = "messages";
        public bool IsEnabled => _parent.StateFor("http") == TransportState.Enabled;


        IHttpTransportConfiguration IHttpTransportConfiguration.EnableListening(bool enabled)
        {
            if (enabled)
            {
                _parent.EnableTransport("http");
            }
            else
            {
                _parent.DisableTransport("http");
            }

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
