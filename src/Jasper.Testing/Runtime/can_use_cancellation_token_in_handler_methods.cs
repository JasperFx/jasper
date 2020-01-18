using System.Threading;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

#if NETSTANDARD2_0
using Microsoft.AspNetCore.Hosting;
using IHostEnvironment = Microsoft.AspNetCore.Hosting.IHostingEnvironment;
using IHostBuilder = Microsoft.AspNetCore.Hosting.IWebHostBuilder;
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
using Host = Microsoft.AspNetCore.WebHost;
#else
using Microsoft.Extensions.Hosting;
#endif

namespace Jasper.Testing.Runtime
{
    public class can_use_cancellation_token_in_handler_methods
    {
        [Fact]
        public void passes_the_token_into_handler_methods()
        {
            var host = Host.CreateDefaultBuilder().UseJasper(x =>
            {
                x.Handlers.DisableConventionalDiscovery()
                    .Discovery(s => s.IncludeType<CancellationTokenUsingMessageHandler>());
            }).Start();



            try
            {
                var message = new CancellationTokenUsingMessage();
                host.Get<ICommandBus>().Invoke(message);

                message.TokenUsed.ShouldNotBeNull();
            }
            finally
            {
                host.Dispose();
            }



        }
    }

    public class CancellationTokenUsingMessage
    {
        public CancellationToken TokenUsed { get; set; }
    }

    public class CancellationTokenUsingMessageHandler
    {
        public void Handle(CancellationTokenUsingMessage message, CancellationToken cancellation)
        {
            message.TokenUsed = cancellation;
        }
    }
}
