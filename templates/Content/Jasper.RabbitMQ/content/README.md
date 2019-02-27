## Jasper Application with Rabbit MQ

There's just a couple pieces here:

1. The project file makes a reference to *Jasper.RabbitMQ* which brings *Jasper* along for the ride
1. You will need to edit the *appsettings.json* file for your actual Rabbit MQ server locations
1. The Jasper listeners and publishers can be configured in the `JasperConfig` class
1. The `Program.Main()` method bootstraps and runs the Jasper application

See also:

* [The Jasper Documentation](http://jasperfx.github.io/documentation/)
* [RabbitMQ Transport](https://jasperfx.github.io/documentation/messaging/transports/rabbitmq/) for more information about the Rabbit MQ integration with Jasper.