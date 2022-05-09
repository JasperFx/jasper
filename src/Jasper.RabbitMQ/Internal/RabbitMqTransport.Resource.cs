using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline;
using Oakton.Resources;
using RabbitMQ.Client.Exceptions;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace Jasper.RabbitMQ.Internal
{
    public partial class RabbitMqTransport : IStatefulResource
    {
        public Task Check(CancellationToken token)
        {
            var queueNames = allKnownQueueNames();
            if (!queueNames.Any())
            {
                return Task.CompletedTask;
            }

            using var connection = BuildConnection();
            using var channel = connection.CreateModel();

            var missing = new List<string>();

            foreach (var queueName in queueNames)
            {
                try
                {
                    channel.MessageCount(queueName);
                }
                catch (Exception)
                {
                    missing.Add(queueName);
                }
            }

            channel.Close();

            connection.Close();

            if (missing.Any())
            {
                throw new Exception($"Missing known queues: {missing.Join(", ")}");
            }

            return Task.CompletedTask;
        }

        public Task ClearState(CancellationToken token)
        {
            PurgeAllQueues();
            return Task.CompletedTask;
        }

        public Task Teardown(CancellationToken token)
        {
            TeardownAll();
            return Task.CompletedTask;
        }

        public Task Setup(CancellationToken token)
        {
            InitializeAllObjects();
            return Task.CompletedTask;
        }

        public Task<IRenderable> DetermineStatus(CancellationToken token)
        {
            var queues = allKnownQueueNames();

            var table = new Table();
            table.Alignment = Justify.Left;
            table.AddColumn("Queue");
            table.AddColumn("Count");

            using var connection = BuildConnection();
            using var channel = connection.CreateModel();

            foreach (var queue in queues)
            {
                try
                {
                    var count = channel.MessageCount(queue);
                    table.AddRow(queue, count.ToString());
                }
                catch (Exception)
                {
                    table.AddRow(new Markup(queue), new Markup("[red]Does not exist[/]"));
                }
            }

            return Task.FromResult((IRenderable)table);
        }

        string IStatefulResource.Type => "JasperTransport";

        internal void PurgeAllQueues()
        {
            using var connection = BuildConnection();
            using var channel = connection.CreateModel();

            foreach (var queue in Queues)
            {
                Console.WriteLine($"Purging Rabbit MQ queue '{queue}'");
                queue.Purge(channel);
            }

            var others = _endpoints.Select(x => x.QueueName).Where(x => x.IsNotEmpty())
                .Where(x => Queues.All(q => q.Name != x)).ToArray();

            foreach (var other in others)
            {
                Console.WriteLine($"Purging Rabbit MQ queue '{other}'");
                try
                {
                    channel.QueuePurge(other);
                }
                catch (OperationInterruptedException e)
                {
                    if (!e.Message.Contains("NOT_FOUND"))
                    {
                        throw;
                    }
                }
            }

            channel.Close();

            connection.Close();
        }

        internal void TeardownAll()
        {
            using var connection = BuildConnection();
            using var channel = connection.CreateModel();

            foreach (var binding in Bindings)
            {
                Console.WriteLine($"Tearing down Rabbit MQ binding {binding}");
                binding.Teardown(channel);
            }

            foreach (var exchange in Exchanges)
            {
                Console.WriteLine($"Tearing down Rabbit MQ exchange {exchange}");
                exchange.Teardown(channel);
            }

            foreach (var queue in Queues)
            {
                Console.WriteLine($"Tearing down Rabbit MQ queue {queue}");
                queue.Teardown(channel);
            }

            channel.Close();

            connection.Close();
        }

        internal void InitializeAllObjects()
        {
            using var connection = BuildConnection();
            using var channel = connection.CreateModel();

            foreach (var queue in Queues)
            {
                Console.WriteLine("Declaring Rabbit MQ queue " + queue);
                queue.Declare(channel);
            }

            foreach (var exchange in Exchanges)
            {
                Console.WriteLine("Declaring Rabbit MQ exchange " + exchange);
                exchange.Declare(channel);
            }

            foreach (var binding in Bindings)
            {
                Console.WriteLine("Declaring Rabbit MQ binding " + binding);
                binding.Declare(channel);
            }

            channel.Close();

            connection.Close();
        }

        private string[] allKnownQueueNames()
        {
            var bindingQueueNames = Bindings.Where(x => x.QueueName != null).Select(x => x.QueueName);
            return Queues.Select(x => x.Name)
                .Concat(bindingQueueNames).Distinct().ToArray()!;
        }
    }
}
