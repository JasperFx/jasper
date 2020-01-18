using System;
using Jasper.Testing.Messaging.Transports.Tcp;
using Jasper.Testing.Transports.Tcp;
using Jasper.Tracking;
using TestingSupport;

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
    public abstract class SendingContext : IDisposable
    {
        private readonly JasperOptions receiverOptions = new JasperOptions();
        private readonly JasperOptions senderOptions = new JasperOptions();
        protected IHost theReceiver;
        protected IHost theSender;

        public SendingContext()
        {
            receiverOptions.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<MessageConsumer>();

            receiverOptions.Extensions.UseMessageTrackingTestingSupport();

            senderOptions.Extensions.UseMessageTrackingTestingSupport();
        }


        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        protected void StartTheSender(Action<JasperOptions> configure)
        {
            configure(senderOptions);
            theSender = JasperHost.For(senderOptions);
        }

        protected void RestartTheSender()
        {
            theSender = JasperHost.For(senderOptions);
        }

        protected void StopTheSender()
        {
            theSender?.Dispose();
        }

        protected void StartTheReceiver(Action<JasperOptions> configure)
        {
            configure(receiverOptions);
            theReceiver = JasperHost.For(receiverOptions);
        }

        protected void RestartTheReceiver()
        {
            theSender = JasperHost.For(receiverOptions);
        }

        protected void StopTheReceiver()
        {
            theSender?.Dispose();
        }
    }
}
