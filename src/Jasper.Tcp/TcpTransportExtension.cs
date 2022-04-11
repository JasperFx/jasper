using Baseline;
using Jasper.Attributes;
using Jasper.Configuration;
using Jasper.Tcp;

[assembly:JasperModule(typeof(TcpTransportExtension))]

namespace Jasper.Tcp
{
    /// <summary>
    /// Including this extension will enable Jasper's TCP socket transport
    /// </summary>
    public class TcpTransportExtension : IJasperExtension
    {
        public void Configure(JasperOptions options)
        {
            options.Get<TcpTransport>();
        }
    }
}
