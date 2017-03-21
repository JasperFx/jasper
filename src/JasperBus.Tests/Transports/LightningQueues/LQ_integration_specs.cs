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
        public void send_a_message_and_get_the_response()
        {
            if (!Platform.IsWindows)
            {
                return;
            }

            var bus = Runtime.Container.GetInstance<IServiceBus>();

            var task = theTracker.WaitFor<Message1>();

            bus.Send(new Message1());

            task.Wait(20.Seconds());

            if (!task.IsCompleted)
            {
                throw new Exception("Got no envelope!");
            }

            var envelope = task.Result;


            envelope.ShouldNotBeNull();
        }
        
    }

    public class RecordingHandler
    {
        private readonly MessageTracker _tracker;

        public RecordingHandler(MessageTracker tracker)
        {
            _tracker = tracker;
        }

        public void Handle(Message1 message, Envelope envelope)
        {
            _tracker.Record(message, envelope);
        }
    }


    public class MessageTracker
    {

        private readonly LightweightCache<Type, List<TaskCompletionSource<Envelope>>>
            _waiters = new LightweightCache<Type, List<TaskCompletionSource<Envelope>>>(t => new List<TaskCompletionSource<Envelope>>());

        public void Record(object message, Envelope envelope)
        {
            var messageType = message.GetType();
            var list = _waiters[messageType];

            list.Each(x => x.SetResult(envelope));

            list.Clear();
        }

        public Task<Envelope> WaitFor<T>()
        {
            var source = new TaskCompletionSource<Envelope>();
            _waiters[typeof(T)].Add(source);

            return source.Task;
        }
    }
}