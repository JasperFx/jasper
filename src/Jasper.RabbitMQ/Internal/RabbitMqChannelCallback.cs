using System.Threading.Tasks;
using Jasper.Transports;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqChannelCallback : IChannelCallback
    {
        private readonly IModel _model;
        private readonly ulong _deliveryTag;
        private readonly RabbitMqSender _sender;

        public RabbitMqChannelCallback(IModel model, ulong deliveryTag, RabbitMqSender sender)
        {
            _model = model;
            _deliveryTag = deliveryTag;
            _sender = sender;
        }

        public Task Complete(Envelope envelope)
        {
            _model.BasicAck(_deliveryTag, false);
            return Task.CompletedTask;
        }

        public Task Defer(Envelope envelope)
        {
            // TODO -- how can you do this transactionally?
            _model.BasicNack(_deliveryTag, false, true);
            return _sender.Send(envelope);
        }
    }
}
