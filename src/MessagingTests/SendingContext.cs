using System;
using Jasper;
using Jasper.Messaging.Tracking;
using MessagingTests.Lightweight;
using TestingSupport;

namespace MessagingTests
{
    public abstract class SendingContext : IDisposable
    {
        private readonly JasperRegistry receiverRegistry = new JasperRegistry();
        private readonly JasperRegistry senderRegistry = new JasperRegistry();
        protected IJasperHost theReceiver;
        protected IJasperHost theSender;
        protected MessageTracker theTracker;

        public SendingContext()
        {
            theTracker = new MessageTracker();
            receiverRegistry.Handlers
                .DisableConventionalDiscovery()
                .IncludeType<MessageConsumer>();

            receiverRegistry.Services.For<MessageTracker>().Use(theTracker);
        }


        public void Dispose()
        {
            theSender?.Dispose();
            theReceiver?.Dispose();
        }

        protected void StartTheSender(Action<JasperRegistry> configure)
        {
            configure(senderRegistry);
            theSender = JasperHost.For(senderRegistry);
        }

        protected void RestartTheSender()
        {
            theSender = JasperHost.For(senderRegistry);
        }

        protected void StopTheSender()
        {
            theSender?.Dispose();
        }

        protected void StartTheReceiver(Action<JasperRegistry> configure)
        {
            configure(receiverRegistry);
            theReceiver = JasperHost.For(receiverRegistry);
        }

        protected void RestartTheReceiver()
        {
            theSender = JasperHost.For(receiverRegistry);
        }

        protected void StopTheReceiver()
        {
            theSender?.Dispose();
        }
    }
}
