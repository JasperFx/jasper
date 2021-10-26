using System;
using Jasper.Logging;
using Jasper.Runtime;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class HandlerPipelineMessageConsumer : MessageConsumerBase
    {
        private readonly RabbitMqSender _sender;
        private readonly IHandlerPipeline _pipeline;

        public HandlerPipelineMessageConsumer(RabbitMqSender sender, IHandlerPipeline pipeline, ITransportLogger logger,
            IModel channel, IRabbitMqProtocol mapper, Uri address, RabbitMqSender rabbitMqSender) : base(logger, channel, mapper, address, rabbitMqSender)
        {
            _sender = sender;
            _pipeline = pipeline;
        }

        protected override void executeEnvelope(ulong deliveryTag, Envelope envelope)
        {
            _pipeline.Invoke(envelope, RabbitMqChannelCallback.Instance).ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogException(t.Exception, envelope.Id, "Failure to receive an incoming message");
                    try
                    {
                        _channel.BasicNack(deliveryTag, false, true);
                    }
                    catch (Exception e)
                    {
                        _logger.LogException(e, envelope.CorrelationId,"Error when trying to Nack a Rabbit MQ message that failed in the HandlerPipeline");
                    }
                }
            });
        }
    }
}
