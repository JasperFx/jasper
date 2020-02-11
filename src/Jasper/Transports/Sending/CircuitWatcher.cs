using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Transports.Sending
{
    public interface ICircuitTester
    {
        Task<bool> TryToResume(CancellationToken cancellationToken);
    }

    public interface ICircuit : ICircuitTester
    {

        Task Resume(CancellationToken cancellationToken);

        TimeSpan RetryInterval { get; }
    }

    public class CircuitWatcher
    {
        private Task _task;
        private readonly ICircuit _circuit;
        private readonly CancellationToken _cancellation;

        public CircuitWatcher(ICircuit circuit, CancellationToken cancellation)
        {
            _circuit = circuit;
            _cancellation = cancellation;

            _task = Task.Run(pingUntilConnected, _cancellation);
        }

        private async Task pingUntilConnected()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                await Task.Delay(_circuit.RetryInterval, _cancellation);

                try
                {
                    var pinged = await _circuit.TryToResume(_cancellation);

                    if (pinged)
                    {
                        await _circuit.Resume(_cancellation);
                        return;
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
