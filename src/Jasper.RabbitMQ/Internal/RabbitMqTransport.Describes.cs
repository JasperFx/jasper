using System.Collections.Generic;
using System.Linq;
using Baseline;
using Oakton.Descriptions;
using Spectre.Console;

namespace Jasper.RabbitMQ.Internal
{
    public partial class RabbitMqTransport : ITreeDescriber
    {
        void ITreeDescriber.Describe(TreeNode parentNode)
        {
            var props = new Dictionary<string, object>{
                {"HostName", ConnectionFactory.HostName},
                {"Port", ConnectionFactory.Port == -1 ? 5672 : ConnectionFactory.Port},
                {nameof(AutoProvision), AutoProvision},
                {nameof(AutoPurgeOnStartup), AutoPurgeOnStartup}
            };

            var table = JasperOptions.BuildTableForProperties(props);
            parentNode.AddNode(table);


            if (Exchanges.Any())
            {
                var exchangesNode = parentNode.AddNode("Exchanges");
                foreach (var exchange in Exchanges)
                {
                    exchangesNode.AddNode(exchange.Name);
                }
            }

            var queueNode = parentNode.AddNode("Queues");
            foreach (var queue in Queues)
            {
                queueNode.AddNode(queue.Name);
            }

            if (Bindings.Any())
            {
                var bindings = parentNode.AddNode("Bindings");

                var bindingTable = new Table();
                bindingTable.AddColumn("Key");
                bindingTable.AddColumn("Exchange Name");
                bindingTable.AddColumn("Queue Name");
                bindingTable.AddColumn("Arguments");

                foreach (var binding in Bindings)
                {
                    bindingTable.AddRow(binding.BindingKey, binding.ExchangeName ?? string.Empty, binding.QueueName,
                        binding.Arguments.Select(pair => $"{pair.Key}={pair.Value}").Join(", "));
                }

                bindings.AddNode(bindingTable);
            }



        }
    }
}
