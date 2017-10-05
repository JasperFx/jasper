using Baseline;
using Jasper;
using Jasper.Bus.Transports;
using Jasper.Configuration;
using Jasper.Remotes.Messaging;
using Jasper.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly:JasperModule(typeof(WebTransportExtension))]

namespace Jasper.WebSockets
{

    public class WebTransportExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Publish
                .MessagesMatching(x => x.CanBeCastTo<ClientMessage>())
                .To("ws://default");


            registry.Http.Configure(app => app.UseWebSockets());

            registry.Services.AddSingleton<WebSocketTransport>();
            registry.Services.AddSingleton<ITransport>(x => x.GetService<WebSocketTransport>());

        }
    }
}
