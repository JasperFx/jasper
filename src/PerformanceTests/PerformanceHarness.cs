using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Jasper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit.Abstractions;

namespace PerformanceTests
{

    public abstract class PerformanceHarness : IDisposable
    {
        private readonly ITestOutputHelper _output;
        private IHost _sender;
        private IHost _receiver;
        private MessageCounter _counter;

        protected PerformanceHarness(ITestOutputHelper output)
        {
            _output = output;
        }

        protected void startTheReceiver(Action<JasperOptions> configure)
        {
            _counter = new MessageCounter();
            _receiver = Host
                .CreateDefaultBuilder()
                .UseJasper(configure)
                .ConfigureServices(s => s.AddSingleton(_counter))
                .Start();
        }

        protected void startTheSender(Action<JasperOptions> configure)
        {
            _sender = Host.CreateDefaultBuilder().UseJasper(configure).Start();
        }

        public void Dispose()
        {
            _sender?.Dispose();
            _receiver?.Dispose();
        }

        protected async Task time(Func<Task> action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            await action();

            stopwatch.Stop();

            _output.WriteLine($"The elapsed time was {stopwatch.ElapsedMilliseconds} ms");
        }

        protected Task waitForMessagesToBeProcessed(int count)
        {
            return time(() => _counter.WaitForExpectedCount(count));
        }

        protected async Task sendMessages(int countPerThread, int threads)
        {
            var tasks = new Task[threads];
            for (int i = 0; i < tasks.Length; i++)
            {
                tasks[i] = Task.Factory.StartNew(async () =>
                {
                    for (int j = 0; j < countPerThread; j++)
                    {
                        await _sender.Send(new SmallMessage());
                    }
                });
            }

            await Task.WhenAll(tasks);
        }
    }

    public class SmallMessage
    {
        public Guid Id { get; set; }
    }

    public class MessageCounter
    {
        private int _expected;
        private TaskCompletionSource<bool> _completion;


        public Task WaitForExpectedCount(int count)
        {
            Count = 0;
            _expected = count;
            _completion = new TaskCompletionSource<bool>();

            return _completion.Task;
        }

        public void Increment()
        {
            Interlocked.Increment(ref Count);
            if (Count >= _expected)
            {
                _completion.SetResult(true);
            }
        }

        public int Count;
    }

    public class SmallMessageHandler
    {
        public void Handle(SmallMessage message, MessageCounter counter)
        {
            counter.Increment();
        }
    }
}
