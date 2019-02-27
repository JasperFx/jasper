## Jasper Application with Azure Service Bus

There's just a couple pieces here:

1. The project file makes a reference to *Jasper.AzureServiceBus* which brings *Jasper* along for the ride
1. You will need to edit the *appsettings.json* file for your actual Azure Service Bus connection string and queue/topic/subscription information
1. The Jasper listeners and publishers can be configured in the `JasperConfig` class
1. The `Program.Main()` method bootstraps and runs the Jasper application


See the [Jasper Documentation](http://jasperfx.github.io/documentation/) for more information in general, and [Azure Service Bus Transport](http://jasperfx.github.io/documentation/messaging/transports/azureservicebus/) for more specific information about the Azure Service Bus integration into Jasper.