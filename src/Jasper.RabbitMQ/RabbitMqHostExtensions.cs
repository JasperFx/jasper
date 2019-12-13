using Jasper.Configuration;
using Jasper.Messaging;
using Jasper.RabbitMQ.Internal;
using LamarCodeGeneration.Util;
using Microsoft.Extensions.Hosting;

namespace Jasper.RabbitMQ
{
    public static class RabbitMqHostExtensions
    {
        private static RabbitMqTransport RabbitMqTransport(this IHost host)
        {
            return host
                .Get<IMessagingRoot>()
                .Options
                .Endpoints
                .As<TransportCollection>()
                .Get<RabbitMqTransport>();
        }

        public static void DeclareAllRabbitMqObjects(this IHost host)
        {
            host.RabbitMqTransport().InitializeAllObjects();
        }

        public static void TryPurgeAllRabbitMqQueues(this IHost host)
        {
            host.RabbitMqTransport().PurgeAllQueues();
        }

        public static void TearDownAllRabbitMqObjects(this IHost host)
        {
            host.RabbitMqTransport().TeardownAll();
        }
    }
}
