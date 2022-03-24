using System;
using Jasper.RabbitMQ.Internal;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Tests
{
    #region sample_AppListeningToRabbitMQ
    public class AppListeningToRabbitMQ : JasperOptions
    {
        public AppListeningToRabbitMQ()
        {
            // Port is optional if you're using the default RabbitMQ
            // port of 5672, but shown here for completeness
            //Transports.ListenForMessagesFrom("rabbitmq://rabbitserver:5672/messages");
        }
    }
    #endregion

    #region sample_AppPublishingToRabbitMQ
    public class AppPublishingToRabbitMQ : JasperOptions
    {
        public AppPublishingToRabbitMQ()
        {
            //Publish.AllMessagesTo("rabbitmq://rabbitserver:5672/messages");
        }
    }
    #endregion


}
