using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Baseline.Dates;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Tracking;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;
using Xunit;

namespace Jasper.RabbitMQ.Testing
{
    public class end_to_end
    {
        [Fact]
        public async Task send_message_to_and_receive_through_rabbitmq()
        {
            var runtime = await JasperRuntime.ForAsync<RabbitMqUsingApp>();



            try
            {
                var tracker = runtime.Get<MessageTracker>();

                var watch = tracker.WaitFor<ColorChosen>();

                await runtime.Messaging.Send(new ColorChosen {Name = "Red"});

                await watch;

                var colors = runtime.Get<ColorHistory>();

                colors.Name.ShouldBe("Red");

                // TODO -- let's look at the envelope too
            }
            finally
            {
                await runtime.Shutdown();
            }
        }
    }

    public class MessageTracker
    {
        private readonly LightweightCache<Type, List<TaskCompletionSource<Envelope>>>
            _waiters = new LightweightCache<Type, List<TaskCompletionSource<Envelope>>>(t => new List<TaskCompletionSource<Envelope>>());

        private readonly ConcurrentBag<ITracker> _trackers = new ConcurrentBag<ITracker>();


        public MessageTracker()
        {
            Debug.WriteLine("What?");
        }

        public void Record(object message, Envelope envelope)
        {
            foreach (var tracker in _trackers)
            {
                tracker.Check(envelope, message);
            }

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
        private readonly int _expected;
        private readonly List<ITracker> _trackers;
        private readonly TaskCompletionSource<bool> _completion = new TaskCompletionSource<bool>();
        private int _count = 0;

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
            Hosting.ConfigureLogging(x =>
            {
                x.AddConsole();
                x.AddDebug();
            });

            Transports.ListenForMessagesFrom("rabbitmq://localhost:5672/messages");

            Services.AddSingleton<ColorHistory>();
            Services.AddSingleton<MessageTracker>();

            Publish.AllMessagesTo("rabbitmq://localhost:5672/messages");

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
