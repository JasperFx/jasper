using System;
using Baseline;
using Jasper;
using Jasper.Configuration;
using Jasper.Http;
using Jasper.Messaging.Transports;
using Jasper.WebSockets;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

[assembly:JasperModule(typeof(WebSocketTransportExtension))]

namespace Jasper.WebSockets
{

    public class WebSocketTransportExtension : IJasperExtension
    {
        public void Configure(JasperRegistry registry)
        {
            registry.Publish
                .MessagesMatching(x => x.CanBeCastTo<ClientMessage>())
                .To("ws://default");


            registry.As<JasperRegistry>().Hosting.Configure(app => app.UseWebSockets());

            registry.Services.AddSingleton<WebSocketTransport>();
            registry.Services.AddTransient<ITransport>(x => x.GetService<WebSocketTransport>());
            registry.Services.AddSingleton<IWebSocketSender, WebSocketSender>();
        }
    }
}
