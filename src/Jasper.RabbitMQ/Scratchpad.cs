using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Jasper.RabbitMQ
{
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

            channel.QueueDeclare("queueName");

            channel.ExchangeDeclare(exchange:"name", type:"topic", durable:true);

            channel.BasicPublish(exchange:"", routingKey:"queueName", body:new byte[0]);

            IBasicProperties props = channel.CreateBasicProperties();




            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {




                //
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine(" [x] Received {0}", message);
            };

            channel.BasicConsume(queue: "queueName", autoAck: true, consumer: consumer);
        }
    }
}