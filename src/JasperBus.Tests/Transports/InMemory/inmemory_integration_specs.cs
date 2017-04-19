using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using JasperBus.Runtime;
using JasperBus.Tests.Runtime;
using Microsoft.DotNet.InternalAbstractions;
using Shouldly;
using Xunit;
using Platform = Baseline.Platform;

namespace JasperBus.Tests.Transports.InMemory
{
    public class inmemory_integration_specs : IntegrationContext
    {
        private readonly MessageTracker theTracker = new MessageTracker();

        public inmemory_integration_specs()
        {
            with(_ =>
            {
                _.UseInMemoryTransport();
                _.ListenForMessagesFrom("memory://incoming");
                _.SendMessage<Message1>().To("memory://incoming");

                _.Services.For<MessageTracker>().Use(theTracker);

                _.Services.Scan(x =>
                {
                    x.TheCallingAssembly();
                    x.WithDefaultConventions();
                });
            });
        }


        [Fact]
        public async Task send_a_message_and_get_the_response()
        {
            var bus = Runtime.Container.GetInstance<IServiceBus>();

            var task = theTracker.WaitFor<Message1>();

            await bus.Send(new Message1());

            task.Wait(20.Seconds());

            if (!task.IsCompleted)
            {
                throw new Exception("Got no envelope!");
            }

            var envelope = task.Result;

            envelope.ShouldNotBeNull();
        }
        
    }
}