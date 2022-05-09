using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Jasper.Attributes;
using Jasper.Runtime.Handlers;
using Jasper.Tracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using TestingSupport;
using Xunit;

namespace Jasper.Testing.Acceptance
{
    public class message_timeout_mechanics
    {
        [Fact]
        public void set_timeout_on_handler_attribute()
        {
            using var host = JasperHost.Basic();
            var handlers = host.Services.GetRequiredService<HandlerGraph>();
            var chain = handlers.ChainFor(typeof(PotentiallySlowMessage));
            chain.ExecutionTimeoutInSeconds.ShouldBe(1); // coming from the attribute
        }

        [Fact]
        public async Task no_timeout()
        {
            PotentiallySlowMessageHandler.DidTimeout = false; // start clean

            using var host = JasperHost.Basic();

            await host.TrackActivity().EnqueueMessageAndWaitAsync(new DurationMessage { DurationInMilliseconds = 50 });

            PotentiallySlowMessageHandler.DidTimeout.ShouldBeFalse();
        }

        [Fact]
        public async Task timeout_using_global_timeout()
        {
            PotentiallySlowMessageHandler.DidTimeout = false; // start clean

            using var host = JasperHost.For(opts =>
            {
                opts.DefaultExecutionTimeout = 50.Milliseconds();
            });

            var session = await host
                .TrackActivity()
                .DoNotAssertOnExceptionsDetected()
                .EnqueueMessageAndWaitAsync(new DurationMessage { DurationInMilliseconds = 500 });

            var exceptions = session.AllExceptions();
            exceptions.Single().ShouldBeOfType<TaskCanceledException>();

        }

        [Fact]
        public async Task timeout_using_message_specific_timeout()
        {
            PotentiallySlowMessageHandler.DidTimeout = false; // start clean

            using var host = JasperHost.Basic();

            var session = await host
                .TrackActivity()
                .DoNotAssertOnExceptionsDetected()
                .EnqueueMessageAndWaitAsync(new PotentiallySlowMessage() { DurationInMilliseconds = 2500 });

            var exceptions = session.AllExceptions();
            exceptions.Single().ShouldBeOfType<TaskCanceledException>();
        }
    }

    public class PotentiallySlowMessage
    {
        public int DurationInMilliseconds { get; set; }
    }

    public class DurationMessage
    {
        public int DurationInMilliseconds { get; set; }
    }

    public class PotentiallySlowMessageHandler
    {
        public static bool DidTimeout { get; set; } = false;

        [MessageTimeout(1)]
        public async Task Handle(PotentiallySlowMessage message, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                while (stopwatch.ElapsedMilliseconds < message.DurationInMilliseconds)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DidTimeout = true;
                        break;
                    }
                    await Task.Delay(25, cancellationToken);
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            DidTimeout = cancellationToken.IsCancellationRequested;
        }

        public async Task Handle(DurationMessage message, CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                while (stopwatch.ElapsedMilliseconds < message.DurationInMilliseconds)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        DidTimeout = true;
                        break;
                    }
                    await Task.Delay(25, cancellationToken);
                }
            }
            finally
            {
                stopwatch.Stop();
            }

            DidTimeout = cancellationToken.IsCancellationRequested;
        }
    }
}
