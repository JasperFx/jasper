using System;

namespace Jasper.Http
{
    public interface IHttpTransportConfiguration
    {
        IHttpTransportConfiguration EnableListening(bool enabled);
        IHttpTransportConfiguration RelativeUrl(string url);
        IHttpTransportConfiguration ConnectionTimeout(TimeSpan span);
    }
}