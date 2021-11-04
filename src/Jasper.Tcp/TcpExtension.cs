using Baseline;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Tcp;

[assembly:JasperModule(typeof(TcpExtension))]

namespace Jasper.Tcp
{
    internal class TcpExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            options.Endpoints.As<TransportCollection>().Get<TcpTransport>();
        }
    }
}
