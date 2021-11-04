using System.Threading.Tasks;
using Jasper.Transports;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqChannelCallback : IChannelCallback
    {
        public static readonly RabbitMqChannelCallback Instance = new RabbitMqChannelCallback();

        private RabbitMqChannelCallback()
        {

        }

        public Task Complete(Envelope envelope)
        {
            if (envelope is RabbitMqEnvelope e)
            {
                e.Complete();
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public Task Defer(Envelope envelope)
        {
            if (envelope is RabbitMqEnvelope e)
            {
                return e.Defer();
            }

            return Task.CompletedTask;
        }
    }
}
