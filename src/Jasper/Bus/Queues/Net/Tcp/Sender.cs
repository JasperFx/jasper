using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Jasper.Bus.Queues.Logging;
using Jasper.Bus.Runtime;

namespace Jasper.Bus.Queues.Net.Tcp
{
    public class Sender : IDisposable
    {
        private readonly ILogger _logger;
        private readonly ISendingProtocol _protocol;
        private readonly ISubject<OutgoingMessageFailure> _failedToSend;
        private IObservable<Envelope> _outgoingStream;
        private IObservable<Envelope> _successfullySent;
        private IDisposable _sendingSubscription;

        public Sender(ILogger logger, ISendingProtocol protocol)
        {
            _logger = logger;
            _protocol = protocol;
            _failedToSend = new Subject<OutgoingMessageFailure>();
        }

        public IObservable<OutgoingMessageFailure> FailedToSend() => _failedToSend;

        public IObservable<Envelope> StartSending(IObservable<Envelope> outgoingStream)
        {
            _outgoingStream = outgoingStream;
            _successfullySent = SuccessfullySentMessages().Publish().RefCount();
            _sendingSubscription = _successfullySent.Subscribe(x => { });
            return _successfullySent;
        }

        public IObservable<Envelope> SuccessfullySentMessages()
        {
            return ConnectedOutgoingMessageBatch()
                .Using(x => _protocol.Send(x).Timeout(TimeSpan.FromSeconds(5))
                .Catch<Envelope, Exception>(ex => HandleException<Envelope>(ex, x)));
        }

        public IObservable<OutgoingMessageBatch> ConnectedOutgoingMessageBatch()
        {
            return AllOutgoingMessagesBatchedByEndpoint()
                .SelectMany(batch =>
                {
                    _logger.DebugFormat("Preparing to send {0} messages to {1}", batch.Messages.Count, batch.Destination);
                    return Observable.FromAsync(batch.ConnectAsync, TaskPoolScheduler.Default)
                        .Timeout(TimeSpan.FromSeconds(5))
                        .Catch<Unit, Exception>(ex => HandleException<Unit>(ex, batch))
                        .Select(_ => batch);
                });
        }

        public IObservable<OutgoingMessageBatch> AllOutgoingMessagesBatchedByEndpoint()
        {
            return BufferedAllOutgoingMessages()
                .SelectMany(x =>
                {
                    return x.GroupBy(grouped => grouped.Destination)
                        .Select(grouped => new OutgoingMessageBatch(grouped.Key, grouped, new TcpClient()));
                });
        }

        public IObservable<IList<Envelope>> BufferedAllOutgoingMessages()
        {
            return AllOutgoingMessages().Buffer(TimeSpan.FromMilliseconds(200))
                .Where(x => x.Count > 0);
        }

        public IObservable<Envelope> AllOutgoingMessages()
        {
            return _outgoingStream;
        }

        private IObservable<T> HandleException<T>(Exception ex, OutgoingMessageBatch batch)
        {
            _logger.Error($"Got an error sending message to {batch.Destination}", ex);
            _failedToSend.OnNext(new OutgoingMessageFailure {Batch = batch, Exception = ex});
                return Observable.Empty<T>();
        }

        public void Dispose()
        {
            _logger.Info("Disposing Sender to " );
            if (_sendingSubscription != null)
            {
                _sendingSubscription.Dispose();
                _sendingSubscription = null;
            }
        }
    }
}
