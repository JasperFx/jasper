using Oakton;
using Oakton.AspNetCore;

namespace Jasper.RabbitMQ.CommandLine
{
    [Description("Declare, purge, or delete the configured Rabbit MQ exchanges, queues, and bindings", Name = "rabbitmq")]
    public class RabbitMqCommand : OaktonCommand<RabbitMqInput>
    {
        public override bool Execute(RabbitMqInput input)
        {
            using (var host = input.BuildHost())
            {
                switch (input.Action)
                {
                    case RabbitAction.setup:
                        host.DeclareAllRabbitMqObjects();
                        break;

                    case RabbitAction.purge:
                        host.TryPurgeAllRabbitMqQueues();
                        break;

                    case RabbitAction.teardown:
                        host.TearDownAllRabbitMqObjects();
                        break;
                }
            }

            return true;
        }
    }

    public enum RabbitAction
    {
        setup,
        purge,
        teardown
    }

    public class RabbitMqInput : NetCoreInput
    {
        [Description("What action to take")]
        public RabbitAction Action { get; set; }
    }
}
