using System;
using System.Threading.Tasks;
using Jasper.Messaging.Runtime;
using Jasper.Messaging.Transports.Sending;

namespace Jasper.Messaging.Transports
{
    public class LocalWorkerSender : IDisposable, ILocalWorkerSender
    {
        private ISendingAgent _durable;
        private LoopbackSendingAgent _lightweight;

        public void Start(IMessagingRoot root)
        {
            _durable = root.Factory.BuildLocalAgent(TransportConstants.DurableLoopbackUri, root);
            _lightweight = new LoopbackSendingAgent(TransportConstants.LoopbackUri, root.Workers);
        }

        public Task EnqueueDurably(params Envelope[] envelopes)
        {
            return _durable.StoreAndForwardMany(envelopes);
        }

        public Task EnqueueLightweight(params Envelope[] envelopes)
        {
            return _lightweight.StoreAndForwardMany(envelopes);
        }

        public void Dispose()
        {
            _durable?.Dispose();
            _lightweight?.Dispose();
        }
    }
}
