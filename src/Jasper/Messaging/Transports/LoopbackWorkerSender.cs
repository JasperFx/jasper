using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public class LoopbackWorkerSender : IDisposable, ILoopbackWorkerSender
    {
        private ISendingAgent _durable;
        private LoopbackSendingAgent _lightweight;

        public void Dispose()
        {
            _durable?.Dispose();
            _lightweight?.Dispose();
        }

        public Task EnqueueDurably(params Envelope[] envelopes)
        {
            return _durable.StoreAndForwardMany(envelopes);
        }

        public Task EnqueueLightweight(params Envelope[] envelopes)
        {
            return _lightweight.StoreAndForwardMany(envelopes);
        }

        public void Start(IMessagingRoot root)
        {
            _durable = root.BuildDurableLoopbackAgent(TransportConstants.DurableLoopbackUri);
            _lightweight = new LoopbackSendingAgent(TransportConstants.LoopbackUri, root.Workers);
        }
    }
}
