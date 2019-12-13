using System;
using System.Collections.Generic;
using RabbitMQ.Client;

namespace Jasper.RabbitMQ.Internal
{
    public class RabbitMqQueue
    {
        public string Name { get; }

        public RabbitMqQueue(string name)
        {
            Name = name;
        }

        internal void Declare(IModel channel)
        {
            channel.QueueDeclare(Name, IsDurable, IsExclusive, AutoDelete, Arguments);
        }

        public bool AutoDelete { get; set; } = false;

        public bool IsExclusive { get; set; } = false;

        public bool IsDurable { get; set; } = true;

        public IDictionary<string, object> Arguments { get; } = new Dictionary<string, object>();

        public void Teardown(IModel channel)
        {
            channel.QueueDeleteNoWait(Name);
        }

        public void Purge(IModel channel)
        {
            try
            {
                channel.QueuePurge(Name);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unable to purge queue " + Name);
                Console.WriteLine(e);
            }
        }
    }
}
