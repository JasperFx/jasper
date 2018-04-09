using System;
using System.IO;
using System.Text;
using System.Threading;
using Jasper.Messaging;
using Jasper.Messaging.Transports;
using Jasper.Messaging.Transports.Sending;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
    public class RabbitMQTransport : ITransport
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public string Protocol { get; } = "rabbitmq";

        public ISendingAgent BuildSendingAgent(Uri uri, IMessagingRoot root, CancellationToken cancellation)
        {
            throw new NotImplementedException();
        }

        public Uri LocalReplyUri { get; }

        public void StartListening(IMessagingRoot root)
        {
            throw new NotImplementedException();
        }

        public void Describe(TextWriter writer)
        {
            throw new NotImplementedException();
        }
    }

    /*
     * Notes
     * Will need to be able to configure the RabbitMQ ConnectionFactory
     * Send the envelope through as a byte array in the body
     * All Agents will need a
     * Use the ServiceName.ToLowerCase-replies as the reply queue
     * Need to use the basic ack to finish the receiving
     */

    public class Scratchpad
    {
        public static void DoStuff()
        {
            var factory = new ConnectionFactory
            {
                HostName = "SomeServer",

            };

            var connection = factory.CreateConnection();

            var channel = connection.CreateModel();

            channel.QueueDeclare("queueName", durable: true, exclusive:false);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                //
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
            };


        }
    }
}
