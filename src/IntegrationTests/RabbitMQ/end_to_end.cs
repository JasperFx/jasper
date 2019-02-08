using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper;
using Jasper.Messaging;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Jasper.Messaging.Transports;
using Jasper.Persistence.Marten;
using Jasper.RabbitMQ;
using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace IntegrationTests.RabbitMQ
{
    [Collection("marten")]
    public class end_to_end : RabbitMQContext
    {
        [Fact]
        public async Task can_stop_and_start()
        {
            using (var runtime = JasperHost.For<RabbitMqUsingApp>())
            {
                var root = runtime.Get<IMessagingRoot>();
                root.ListeningStatus = ListeningStatus.TooBusy;
                root.ListeningStatus = ListeningStatus.Accepting;


                var tracker = runtime.Get<MessageTracker>();

                var watch = tracker.WaitFor<ColorChosen>();

                await runtime.Messaging.Send(new ColorChosen {Name = "Red"});

                await watch;

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }

        }

        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq()
        {
            using (var runtime = JasperHost.For<RabbitMqUsingApp>())
            {
                var tracker = runtime.Get<MessageTracker>();

                var watch = tracker.WaitFor<ColorChosen>();

                await runtime.Messaging.Send(new ColorChosen {Name = "Red"});

                await watch;

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }
        }


        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_using_connection_string()
        {
            using (var runtime = JasperHost.For<RabbitMqUsingApp2>())
            {
                var tracker = runtime.Get<MessageTracker>();

                var watch = tracker.WaitFor<ColorChosen>();

                await runtime.Messaging.Send(new ColorChosen {Name = "Red"});

                await watch;

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");
            }
        }

        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq_with_durable_transport_option()
        {
            var uri = "rabbitmq://localhost:5672/durable/messages2";

            var publisher = JasperHost.For(_ =>
            {
                _.Publish.AllMessagesTo(uri);

                _.Include<MartenBackedPersistence>();

                _.Settings.MartenConnectionStringIs(Servers.PostgresConnectionString);
            });

            publisher.Get<IDocumentStore>().Advanced.Clean.CompletelyRemoveAll();

            var receiver = JasperHost.For(_ =>
            {
                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();

                _.Include<MartenBackedPersistence>();

                _.Settings.MartenConnectionStringIs(Servers.PostgresConnectionString);
            });

            var wait = receiver.Get<MessageTracker>().WaitFor<ColorChosen>();

            try
            {
                await publisher.Messaging.Send(new ColorChosen {Name = "Orange"});

                await wait;

                receiver.Get<ColorHistory>().Name.ShouldBe("Orange");
            }
            finally
            {
                publisher.Dispose();
                receiver.Dispose();
            }
        }

        [Fact]
        public async Task use_fan_out_exchange()
        {
            var uri = "rabbitmq://localhost:5672/fanout/north/messages";

            var publisher = JasperHost.For(_ =>
            {
                _.Publish.AllMessagesTo(uri);
            });

            var receiver1 = JasperHost.For(_ =>
            {
                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();
            });

            var receiver2 = JasperHost.For(_ =>
            {
                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();
            });

            var receiver3 = JasperHost.For(_ =>
            {
                _.Transports.ListenForMessagesFrom(uri);
                _.Services.AddSingleton<ColorHistory>();
                _.Services.AddSingleton<MessageTracker>();
            });

            var wait1 = receiver1.Get<MessageTracker>().WaitFor<ColorChosen>();
            var wait2 = receiver2.Get<MessageTracker>().WaitFor<ColorChosen>();
            var wait3 = receiver3.Get<MessageTracker>().WaitFor<ColorChosen>();

            try
            {
                await publisher.Messaging.Send(new ColorChosen {Name = "Purple"});

                await wait1;
                //await wait2;
                //await wait3;

                receiver1.Get<ColorHistory>().Name.ShouldBe("Purple");
                //receiver2.Get<ColorHistory>().Name.ShouldBe("Purple");
                //receiver3.Get<ColorHistory>().Name.ShouldBe("Purple");
            }
            finally
            {
                publisher.Dispose();
                receiver1.Dispose();
                receiver2.Dispose();
                receiver3.Dispose();
            }
        }
    }


    public class MessageTracker
    {
        private readonly ConcurrentBag<ITracker> _trackers = new ConcurrentBag<ITracker>();

        private readonly LightweightCache<Type, List<TaskCompletionSource<Envelope>>>
            _waiters = new LightweightCache<Type, List<TaskCompletionSource<Envelope>>>(t =>
                new List<TaskCompletionSource<Envelope>>());

        public void Record(object message, Envelope envelope)
        {
            foreach (var tracker in _trackers) tracker.Check(envelope, message);

            var messageType = message.GetType();
            var list = _waiters[messageType];

            list.Each(x => x.SetResult(envelope));

            list.Clear();
        }

        public Task<Envelope> WaitFor<T>()
        {
            var source = new TaskCompletionSource<Envelope>();
            _waiters[typeof(T)].Add(source);

            Task.Delay(30.Seconds()).ContinueWith(x => { source.TrySetCanceled(); });

            return source.Task;
        }
    }

    public interface ITracker
    {
        void Check(Envelope envelope, object message);
    }

    public class CountTracker<T> : ITracker
    {
        private readonly TaskCompletionSource<bool> _completion = new TaskCompletionSource<bool>();
        private readonly int _expected;
        private readonly List<ITracker> _trackers;
        private int _count;

        public CountTracker(int expected, List<ITracker> trackers)
        {
            _expected = expected;
            _trackers = trackers;
        }

        public Task<bool> Completion => _completion.Task;

        public void Check(Envelope envelope, object message)
        {
            if (message is T)
            {
                Interlocked.Increment(ref _count);

                if (_count >= _expected)
                {
                    _completion.TrySetResult(true);
                    _trackers.Remove(this);
                }
            }
        }
    }

    public class RabbitMqUsingApp : JasperRegistry
    {
        public RabbitMqUsingApp()
        {
            Transports.ListenForMessagesFrom("rabbitmq://localhost:5672/messages3");

            Services.AddSingleton<ColorHistory>();
            Services.AddSingleton<MessageTracker>();

            Publish.AllMessagesTo("rabbitmq://localhost:5672/messages3");

            Include<MessageTrackingExtension>();
        }
    }

    public class RabbitMqUsingApp2 : JasperRegistry
    {
        public RabbitMqUsingApp2()
        {
            Settings.Alter<RabbitMqSettings>(settings =>
            {
                settings.Connections.Add("messages3", "host=localhost;queue=messages3");
            });

            Transports.ListenForMessagesFrom("rabbitmq://messages3");

            Services.AddSingleton<ColorHistory>();
            Services.AddSingleton<MessageTracker>();

            Publish.AllMessagesTo("rabbitmq://messages3");

            Include<MessageTrackingExtension>();
        }
    }

    public class ColorHandler
    {
        public void Handle(ColorChosen message, ColorHistory history, Envelope envelope, MessageTracker tracker)
        {
            history.Name = message.Name;
            history.Envelope = envelope;
            tracker.Record(message, envelope);
        }
    }

    public class ColorHistory
    {
        public string Name { get; set; }
        public Envelope Envelope { get; set; }
    }

    public class ColorChosen
    {
        public string Name { get; set; }
    }
}
