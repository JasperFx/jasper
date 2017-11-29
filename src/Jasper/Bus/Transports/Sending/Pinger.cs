using System;
using System.Threading;
using System.Threading.Tasks;

namespace Jasper.Bus.Transports.Sending
{
    public class Pinger : IDisposable
    {
        private readonly ISender _sender;
        private readonly TimeSpan _cooldown;
        private readonly Action _callback;
        private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
        private Task _task;

        public Pinger(ISender sender, TimeSpan cooldown, Action callback)
        {
            _sender = sender;
            _cooldown = cooldown;
            _callback = callback;

            _task = Task.Run(pingUntilConnected, _cancellation.Token);
        }

        private async Task pingUntilConnected()
        {
            while (!_cancellation.IsCancellationRequested)
            {
                await Task.Delay(_cooldown, _cancellation.Token);

                try
                {
                    await _sender.Ping();

                    _callback();
                    return;
                }
                catch (Exception)
                {

                }
            }
        }

        public void Dispose()
        {
            _cancellation.Cancel();
        }
    }
}
