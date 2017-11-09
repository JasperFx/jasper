using System;
using System.Threading.Tasks;
using Jasper.Bus.Runtime;
using Jasper.Bus.Transports;
using Jasper.Bus.Transports.Sending;
using Jasper.Bus.WorkerQueues;

namespace Jasper.Http.Transport
{
    public class LocalWorkerSender : IDisposable, ILocalWorkerSender
    {
        private ISendingAgent _durable;
        private LoopbackSendingAgent _lightweight;

        public void Start(IPersistence persistence, IWorkerQueue workers)
        {
            _durable = persistence.BuildLocalAgent(TransportConstants.DurableLoopbackUri, workers);
            _lightweight = new LoopbackSendingAgent(TransportConstants.LoopbackUri, workers);
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
