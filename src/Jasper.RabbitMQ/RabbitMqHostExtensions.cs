using Jasper.Configuration;
using Jasper.RabbitMQ.Internal;
using Jasper.Runtime;
using LamarCodeGeneration.Util;

#if NETSTANDARD2_0
using IHost = Microsoft.AspNetCore.Hosting.IWebHost;
#else
using IHost = Microsoft.Extensions.Hosting.IHost;
#endif

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

        /// <summary>
        /// Connects to the configured Rabbit MQ broker and ensures that
        /// the exchanges, queues, and bindings declared by this Jasper application
        /// inside the JasperOptions.Endpoints.ConfigureRabbitMq() block
        /// have been created
        /// </summary>
        /// <param name="host"></param>
        public static void DeclareAllRabbitMqObjects(this IHost host)
        {
            host.RabbitMqTransport().InitializeAllObjects();
        }

        /// <summary>
        /// Connects to the configured Rabbit MQ broker and tries to purge
        /// the known queues of any messages. This should only be used at
        /// testing or development time
        /// </summary>
        /// <param name="host"></param>
        public static void TryPurgeAllRabbitMqQueues(this IHost host)
        {
            host.RabbitMqTransport().PurgeAllQueues();
        }

        /// <summary>
        /// Connects to the configured Rabbit MQ broker and deletes all
        /// the exchanges, queues, and bindings declared by this Jasper application
        /// inside the JasperOptions.Endpoints.ConfigureRabbitMq() block
        /// </summary>
        /// <param name="host"></param>
        public static void TearDownAllRabbitMqObjects(this IHost host)
        {
            host.RabbitMqTransport().TeardownAll();
        }
    }
}
