using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Remotes.Messaging;
using Jasper.Util;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Shouldly;
using Xunit;

namespace Jasper.WebSockets.Testing
{
    public class end_to_end : IDisposable
    {
        private JasperRuntime theRuntime;

        public end_to_end()
        {
            theRuntime = JasperRuntime.For<WebSocketPingPongApp>();
        }

        public void Dispose()
        {
            theRuntime.Dispose();
        }

        [Fact]
        public async Task send_and_receive()
        {
            var timeout = new CancellationTokenSource();
            timeout.CancelAfter(60.Seconds());

            using (var client = new ClientWebSocket())
            {
                await client.ConnectAsync("ws://127.0.0.1:3010".ToUri(), timeout.Token);

                var outgoing = new SocketPing {Name = "Kareem Hunt"}.ToCleanJson();
                await client.SendMessageAsync(outgoing);

                var buffer = new ArraySegment<byte>(new byte[1000]);

                var result = await client.ReceiveAsync(buffer, timeout.Token);

                var json = buffer.ReadString(result);
                JsonSerialization.DeserializeMessage(json)
                    .ShouldBeOfType<SocketPong>()
                    .Name.ShouldBe("Kareem Hunt");
            }
        }
    }

    public class WebSocketPingPongApp : JasperRegistry
    {
        public WebSocketPingPongApp()
        {
            Http
                .UseKestrel()
                .UseUrls("http://localhost:3010")
                .Configure(app =>
                {
                    app.AddJasperWebSockets().Run(x =>
                    {
                        x.Response.ContentType = "text/plain";
                        return x.Response.WriteAsync("Hey, I'm here");
                    });
                });
        }
    }

    public class PingHandler
    {
        public SocketPong Handle(SocketPing ping)
        {
            return new SocketPong{Name = ping.Name};
        }
    }

    public class SocketPing : ClientMessage
    {
        public SocketPing() : base("socket-ping")
        {
        }

        public string Name { get; set; }
    }

    public class SocketPong : ClientMessage
    {
        public SocketPong() : base("socket-pong")
        {
        }

        public string Name { get; set; }
    }
}
