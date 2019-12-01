using System;
using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging.Transports.Tcp;
using Microsoft.Extensions.Hosting;
using TestingSupport;

namespace Jasper.Testing.Messaging
{
    public abstract class SendingContext : IDisposable
    {
        private readonly JasperOptions receiverOptions = new JasperOptions();
        private readonly JasperOptions senderOptions = new JasperOptions();
        protected IHost theReceiver;
        protected IHost theSender;
        protected MessageTracker theTracker;

        public SendingContext()
        {
            theTracker = new MessageTracker();
            receiverOptions.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<MessageConsumer>();

            receiverOptions.Services.For<MessageTracker>().Use(theTracker);
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
