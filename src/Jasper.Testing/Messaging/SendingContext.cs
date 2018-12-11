using System;
using System.Threading.Tasks;
using Jasper.Messaging.Tracking;
using Jasper.Testing.Messaging.Lightweight;

namespace Jasper.Testing.Messaging
{
    public abstract class SendingContext : IDisposable
    {
        private readonly JasperRegistry receiverRegistry = new JasperRegistry();
        private readonly JasperRegistry senderRegistry = new JasperRegistry();
        protected JasperRuntime theReceiver;
        protected JasperRuntime theSender;
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

        protected async Task StartTheSender(Action<JasperRegistry> configure)
        {
            configure(senderRegistry);
            theSender = await JasperRuntime.ForAsync(senderRegistry);
        }

        protected async Task RestartTheSender()
        {
            theSender = await JasperRuntime.ForAsync(senderRegistry);
        }

        protected void StopTheSender()
        {
            theSender?.Dispose();
        }

        protected async Task StartTheReceiver(Action<JasperRegistry> configure)
        {
            configure(receiverRegistry);
            theReceiver = await JasperRuntime.ForAsync(receiverRegistry);
        }

        protected async Task RestartTheReceiver()
        {
            theSender = await JasperRuntime.ForAsync(receiverRegistry);
        }

        protected void StopTheReceiver()
        {
            theSender?.Dispose();
        }
    }
}
