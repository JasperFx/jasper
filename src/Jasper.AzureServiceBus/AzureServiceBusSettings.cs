using System;
using Jasper.Util;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;

namespace Jasper.AzureServiceBus
{
    public class AzureServiceBusSettings
    {
        public string ConnectionString { get; set; }

        public IQueueClient BuildClient(Uri uri)
        {
            var queueName = uri.QueueName();
            return new QueueClient(ConnectionString, queueName);
        }

        public IMessageReceiver BuildReceiver(Uri uri)
        {
            var queueName = uri.QueueName();
            return new MessageReceiver(ConnectionString, queueName);
        }

        public IMessageSender BuildSender(Uri destination)
        {
            var queueName = destination.QueueName();
            return new MessageSender(ConnectionString, queueName);
        }
    }
}