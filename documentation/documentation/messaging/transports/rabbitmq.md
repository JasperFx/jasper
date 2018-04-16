<!--title:RabbitMQ Transport-->

To use [RabbitMQ](http://www.rabbitmq.com/) as a transport with Jasper, first install the `Jasper.RabbitMQ` library via nuget to your project. Behind the scenes, this package uses the [RabbitMQ C# Client](https://www.rabbitmq.com/dotnet.html) to both send and receive 
messages from RabbitMQ.

To listen for messages from RabbitMQ - assuming that another Jasper application is the original publisher - you simply specify that 
Jasper should listen to a specific RabbitMQ queue with a Uri like this:

<[sample:AppListeningToRabbitMQ]>

Likewise, to publish messages to RabbitMQ, use syntax like this:

<[sample:AppPublishingToRabbitMQ]>

As with other transport types, the actual Uri values can be <[linkto:documentation/bootstrapping/configuration;title=resolved from configuration]>. By moving the Uri values to configuration, it also becomes possible to use one of Jasper's built in transports
like the <[linkto:documentation/messaging/transports/tcp]> when developing locally to debug interatiions between applications without
having to have RabbitMQ running locally, but still use RabbitMQ in production without changing any of the application code.

## Uri Structure

The Uri structure to identify a RabbitMQ queue to Jasper should follow this structure:

`rabbitmq://[host]:[port]/[durable]/[exchange type]/[exchange name]/[queue name]`

* `host` -- the name RabbitMQ host or possibly a load balancer name
* `port` -- Optional. If specified, this will simply override the port number from the default 5672
* `durable` -- Optional, but if the first segment is "durable," this directs Jasper to use the <[linkto:documentation/messaging/transports/durable;title=durable message persistence]>
* `exchange type` -- Optional, but if it exists, this would specify the type of exchange to create or bind to, with the value options 
  *direct*, *fanout*, *topic*, or *headers*. See the [RabbitMQ C# Client documentation](http://www.rabbitmq.com/tutorials/tutorial-four-dotnet.html) for more information on these exchange types.
* `exchange name` -- Optional, but must be present if you are specifying the exchange type
* `queue name` -- The name of the RabbitMQ queue to bind to and send to. If there are additional segments, the queue name is
  determined by doing a join of the segments with the '/' character.

Here are some examples:

* `rabbitmq://localhost/messages` -- bind to your local machine at the default port to queue "messages"
* `rabbitmq://rabbitserver1:5673/durable/messages` -- bind to a RabbitMQ instance on server "rabbitserver1" at port 5673
  to a queue named "messages" with durable, store and forward messaging applied.
* `rabbitmq://rabbitserver1:5673/fanout/exchange1/messages` -- bind to a RabbitMQ instance on server "rabbitserver1" at port 5673
  to a queue named "messages" without durable messages, and using a "fanout" exchange named "exchange1"
* `rabbitmq://rabbitserver1:5673/durable/fanout/exchange1/messages` -- same as above, but this time with the durable messaging
  applied to all incoming or outgoing messages

## Customizing the RabbitMQ Behavior

Definitely see the [documentation for the RabbitMQ C# client](http://www.rabbitmq.com/tutorials/tutorial-one-dotnet.html) for
much more information about all the possible configuration items, but just know that all of the client configuration is
surfaced in Jasper through its <[linkto:documentation/bootstrapping/configuration;title="Settings" model]> with the `RabbitMQSettings`
object as shown below:

<[sample:CustomizedRabbitMQApp]>

If you need to integrate with a non-Jasper application using the RabbitMQ transport, you can use a custom `IEnvelopeMapper` and apply
it to map a different RabbitMQ header structure or to translate message types so that Jasper can successfully handle the message. That interface looks like this:

<[sample:RabbitMQ-IEnvelopeMapper]>



