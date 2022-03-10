using Jasper;
using Jasper.RabbitMQ;
using Jasper.Tcp;
using LamarCodeGeneration;
using TestMessages;

namespace MyApp
{
    // SAMPLE: MyAppRegistryWithOptions
    public class MyAppOptions : JasperOptions
    {
        public MyAppOptions()
        {
            Endpoints.ListenAtPort(2222);

            Endpoints.PublishAllMessages().ToPort(2224);

            Advanced.CodeGeneration.TypeLoadMode = TypeLoadMode.Static;

            Endpoints.ConfigureRabbitMq(x =>
            {
                x.AutoProvision = true;
                x.AutoPurgeOnStartup = true;
                x.DeclareQueue("rabbit1");
                x.DeclareQueue("rabbit2");
                x.DeclareExchange("Main");
                x.DeclareBinding(new Binding
                {
                    BindingKey = "BKey",
                    QueueName = "queue1",
                    ExchangeName = "Main"

                });

            });

            Endpoints.ListenToRabbitQueue("rabbit1");
            Endpoints.PublishAllMessages().ToRabbit("rabbit2");
        }
    }
    // ENDSAMPLE


    public class MessageHandler
    {
        public void Handle(Message1 msg)
        {
        }

        public void Handle(Message2 msg)
        {
        }

        public void Handle(Message3 msg)
        {
        }

        public void Handle(Message4 msg)
        {
        }

        public void Handle(Message5 msg)
        {
        }
    }
}
