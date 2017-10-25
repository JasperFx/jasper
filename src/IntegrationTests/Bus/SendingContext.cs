using System;
using Jasper;
using Jasper.Testing.Bus;
using Jasper.Testing.Bus.Lightweight;

namespace IntegrationTests.Bus
{
    public abstract class SendingContext : IDisposable
    {
        private readonly JasperRegistry senderRegistry = new JasperRegistry();
        private readonly JasperRegistry receiverRegistry = new JasperRegistry();
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

        protected void StartTheReceiver(Action<JasperRegistry> configure)
        {
            configure(receiverRegistry);
            theSender = JasperRuntime.For(receiverRegistry);
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
