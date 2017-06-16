using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using JasperBus.Runtime;
using JasperBus.Tests.Runtime;
using JasperBus.Transports.LightningQueues;
using Microsoft.DotNet.InternalAbstractions;
using Shouldly;
using Xunit;
using Platform = Baseline.Platform;

namespace JasperBus.Tests.Transports.LightningQueues
{
    public class LQ_integration_specs : IntegrationContext
    {
        private readonly MessageTracker theTracker = new MessageTracker();

        public LQ_integration_specs()
        {
            LightningQueuesTransport.DeleteAllStorage();

            with(_ =>
            {
                _.ListenForMessagesFrom("lq.tcp://localhost:2200/incoming");
                _.SendMessage<Message1>().To("lq.tcp://localhost:2200/incoming");

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
            if (!Platform.IsWindows)
            {
                return;
            }

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