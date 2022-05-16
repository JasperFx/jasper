using System;
using System.Threading.Tasks;
using Jasper.Runtime;
using Jasper.Testing.Transports.Tcp;
using Jasper.Transports.Tcp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TestingSupport;

namespace Jasper.Testing
{
    public abstract class SendingContext : IAsyncDisposable
    {
        private IHost _sender;
        private IHost _receiver;
        private readonly int _senderPort;

        public SendingContext()
        {
            _senderPort = PortFinder.GetAvailablePort();
            ReceiverPort = PortFinder.GetAvailablePort();
        }

        public int ReceiverPort { get; }

        internal void SenderOptions(Action<JasperOptions> configure)
        {
            _sender = JasperHost.For(opts =>
            {
                configure(opts);
                opts.ListenAtPort(_senderPort);
            });
        }

        internal IJasperRuntime theSendingRuntime
        {
            get
            {
                return theSender.Services.GetRequiredService<IJasperRuntime>();
            }
        }

        internal IHost theSender
        {
            get
            {
                if (_sender == null)
                {
                    _sender = JasperHost.For(opts =>
                    {
                        opts.PublishAllMessages().ToPort(ReceiverPort);
                        opts.ListenAtPort(_senderPort);
                    });
                }

                return _sender;
            }
        }

        internal IHost theReceiver
        {
            get
            {
                if (_receiver == null)
                {
                    _receiver = JasperHost.For(opts => opts.ListenAtPort(ReceiverPort));
                }

                return _receiver;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_receiver != null)
            {
                await _receiver.StopAsync();
                _receiver.Dispose();
            }

            if (_sender != null)
            {
                await _sender.StopAsync();
                _sender.Dispose();
            }
        }
    }
}
