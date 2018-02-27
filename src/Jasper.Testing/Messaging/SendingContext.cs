using System;
using Jasper.Http;
using Jasper.Testing.Messaging.Lightweight;
using Xunit;

namespace Jasper.Testing.Messaging
{
    [Collection("integration")]
    public abstract class SendingContext : IDisposable
    {
        private readonly JasperHttpRegistry senderRegistry = new JasperHttpRegistry();
        private readonly JasperHttpRegistry receiverRegistry = new JasperHttpRegistry();
        protected JasperRuntime theSender;
        protected JasperRuntime theReceiver;
        protected MessageTracker theTracker;

        public SendingContext()
        {
            theTracker = new MessageTracker();
            receiverRegistry.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<MessageConsumer>();

            receiverRegistry.Services.For<MessageTracker>().Use(theTracker);

        }

        protected void StartTheSender(Action<JasperRegistry> configure)
        {
            configure(senderRegistry);
            theSender = JasperRuntime.For(senderRegistry);
        }

        protected void RestartTheSender()
        {
            theSender = JasperRuntime.For(senderRegistry);
        }

        protected void StopTheSender()
        {
            theSender?.Dispose();
        }

        protected void StartTheReceiver(Action<JasperHttpRegistry> configure)
        {
            configure(receiverRegistry);
            theReceiver = JasperRuntime.For(receiverRegistry);
        }

        protected void RestartTheReceiver()
        {
            theSender = JasperRuntime.For(receiverRegistry);
        }

        protected void StopTheReceiver()
        {
            theSender?.Dispose();
        }


        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }
    }
}
