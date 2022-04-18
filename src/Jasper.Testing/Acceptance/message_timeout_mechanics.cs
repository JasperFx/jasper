using System.Diagnostics;
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

            using var host = JasperHost.For(opts => opts.Extensions.UseMessageTrackingTestingSupport());

            await host.TrackActivity().EnqueueMessageAndWait(new DurationMessage { DurationInMilliseconds = 50 });

            PotentiallySlowMessageHandler.DidTimeout.ShouldBeFalse();
        }

        [Fact]
        public async Task timeout_using_global_timeout()
        {
            PotentiallySlowMessageHandler.DidTimeout = false; // start clean

            using var host = JasperHost.For(opts =>
            {
                opts.DefaultExecutionTimeout = 50.Milliseconds();
                opts.Extensions.UseMessageTrackingTestingSupport();
            });

            var session = await host
                .TrackActivity()
                .DoNotAssertOnExceptionsDetected()
                .EnqueueMessageAndWait(new DurationMessage { DurationInMilliseconds = 500 });

            PotentiallySlowMessageHandler.DidTimeout.ShouldBeTrue();
        }

        [Fact]
        public async Task timeout_using_message_specific_timeout()
        {
            PotentiallySlowMessageHandler.DidTimeout = false; // start clean

            using var host = JasperHost.For(opts =>
            {
                opts.Extensions.UseMessageTrackingTestingSupport();
            });

            await host.TrackActivity().EnqueueMessageAndWait(new PotentiallySlowMessage() { DurationInMilliseconds = 2500 });

            PotentiallySlowMessageHandler.DidTimeout.ShouldBeTrue();
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
