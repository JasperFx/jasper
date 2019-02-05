<!--title:Using Jasper as a Lightweight Service Bus-->

Using its own built in <[linkto:documentation/messaging/transports/tcp]>, Jasper can be used as a very lightweight service bus to send messages asynchronously between Jasper applications. In this tutorial, we're going to create two Jasper services that exchange messages with each either.


<[info]>
While this was originally meant for production applications -- and its predecessor implementation from FubuMVC has been in production under significant load for 5-6 years, we think this functionality will be mostly useful for testing scenarios where your production transport is unusable locally. Looking at you Azure Service Bus.
<[/info]>

For the purpose of this tutorial, we're going to heavily leverage the `dotnet` command line tools, so if you would, open your favorite command line tool. 

The first step is just to ensure you have the latest Jasper project templates by installing `JasperTemplates` as shown below:

```
dotnet new -i JasperTemplates
```

Next, let's build up a new .Net solution with these three projects:

1. *Messages* -- just a class library that holds shared message types we'll be sending back and forth. 
1. *Pinger* -- a Jasper service that will send `PingMessage` messages every 5 seconds
1. *Ponger* -- a Jasper service that will receive `PingMessage` messages, and send a corresponding `PongMessage`
   back to the original sender service

<[info]>
You do **not** have to use shared libraries of message DTO classes in order to use messaging between Jasper applications,
but it's the simplest possible way to get started, so here it is.
<[/info]>

To build out this new solution, you can use the following command line script and then open the newly created *PingAndPong.sln* file:

```
mkdir PingAndPong
cd PingAndPong

dotnet new classlib --name Messages
dotnet new jasper.service --name Pinger
dotnet new jasper.service --name Ponger

dotnet add Pinger/Pinger.csproj reference Messages/Messages.csproj

dotnet new sln

dotnet sln PingAndPong.sln add Messages/Messages.csproj Pinger/Pinger.csproj Ponger/Ponger.csproj
```

## In the Messages Project

All we need to do in the *Messages* project is to add these message types for later:

```
public class PingMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class PongMessage
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
```

## In the Ponger Project

We need to make a couple changes to *Ponger*. First, open the generated `JasperConfig` class
and add the line of code shown below to tell *Ponger* to listen for messages on port 3051:

```
    internal class JasperConfig : JasperRegistry
    {
        public JasperConfig()
        {
            Transports.LightweightListenerAt(3051);
        }
    }
```

Next, we need to handle the `PingMessage` message, and send a `PongMessage` back to the original sender:

```
public class PingHandler
{
    public Response Handle(PingMessage message)
    {
        ConsoleWriter.Write(ConsoleColor.Cyan, "Got a ping with name: " + message.Name);

        var response = new PongMessage
        {
            Name = message.Name
        };

        // Don't know if you'd use this very often,
        // but this is a special syntax that will send
        // the "response" back to the original sender
        return Respond.With(response).ToSender();
    }
}
```


## In the Pinger Project

In *Pinger*, we need a handler for the `PongMessage` messages coming back, so add this class:


```
public class PongHandler
{
    public void Handle(PongMessage message)
    {
        ConsoleWriter.Write(ConsoleColor.Cyan, "Got a pong back with name: " + message.Name);
    }
}
```

We also need something that just runs in the background and sends `PingMessage` messages out. For that, we'll use
an implementation of ASP.Net Core's `IHostedService`:

```
    // In this case, BackgroundService is a base class
    // for the IHostedService that is *supposed* to be
    // in a future version of ASP.Net Core that I shoved
    // into Jasper so we could use it now. The one in Jasper
    // will be removed later when the real one exists in
    // ASP.Net Core itself
    public class PingSender : BackgroundService
    {
        private readonly IMessageContext _bus;

        public PingSender(IMessageContext bus)
        {
            _bus = bus;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var count = 1;

            return Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);

                    await _bus.Send(new PingMessage
                    {
                        Name = "Message" + count++
                    });
                }
            }, stoppingToken);
        }
    }
```

Lastly in *Pinger*, open the `JasperConfig` class and add a single line of code shown below that directs Jasper to 
listen for messages using its <[linkto:documentation/messaging/transports/tcp]> at port 3050. We'll also make
a static publishing rule to publish all messages to the *Ponger* service. Lastly, we need to register the `PingSender`
with the system so that it runs continuously when the application is started.

All that is shown below:

```
    internal class JasperConfig : JasperRegistry
    {
        public JasperConfig()
        {
            
            // Directs Jasper to use the TCP listener and
            // bind to port 3050. 
            Transports.LightweightListenerAt(3050);
            
            // Send all published messages to this location
            Publish.AllMessagesTo("tcp://localhost:3051");

            Services.AddSingleton<IHostedService, PingSender>();
        }
    }
```



## Running Pinger and Ponger

Now you'll want a pair of command windows open, one to the root directory of `Pinger` and one to `Ponger`.

Since it's a little bit cleaner, start up *Ponger* first with a simple call to `dotnet run` from the *Ponger* directory and you should see output like this:

```
ComputerName:Ponger user$ dotnet run
dbug: Microsoft.AspNetCore.Hosting.Internal.WebHost[3]
      Hosting starting
Jasper 'Nullo' startup is being used to start the ASP.Net Core application
dbug: Microsoft.AspNetCore.Hosting.Internal.WebHost[4]
      Hosting started
dbug: Microsoft.AspNetCore.Hosting.Internal.WebHost[0]
      Loaded hosting startup assembly Ponger, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Running service 'JasperConfig'
Application Assembly: Ponger, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
Hosting environment: Production
Content root path: /Users/jeremydmiller/code/PingAndPong/Ponger/bin/Debug/netcoreapp2.1/
Hosted Service: Jasper.JasperActivator
Hosted Service: Jasper.Messaging.Logging.MetricsCollector
Hosted Service: Jasper.Messaging.BackPressureAgent
Listening for loopback messages

Active sending agent to loopback://retries/
Handles messages:
  PingMessage: HandlerType: Ponger.PingHandler, Method: Jasper.Messaging.Runtime.Invocation.Response Handle(Messages.PingMessage)

Application started. Press Ctrl+C to shut down.

```

Then start *Pinger* with the same `dotnet run` command from the root of the *Pinger* project and well, you'll get a boatload of logging like this:

```
dbug: Jasper.Messages[101]
      Received PongMessage#0168be78-ddcd-43b6-8fc4-15cc855354c3 at tcp://localhost:3050/ from tcp://localhost:3051/
Got a pong back with name: Message18
dbug: Jasper.Messages[102]
      Started processing PongMessage#0168be78-ddcd-43b6-8fc4-15cc855354c3
dbug: Jasper.Messages[103]
      Finished processing PongMessage#0168be78-ddcd-43b6-8fc4-15cc855354c3
info: Jasper.Messages[104]
      Successfully processed message PongMessage#0168be78-ddcd-43b6-8fc4-15cc855354c3 from tcp://localhost:3051/
dbug: Jasper.Messages[100]
      Sent PingMessage#0168be78-e0ed-4eec-8081-05e7b1f36006 to tcp://localhost:3051/
info: Jasper.Transports[204]
      Sending agent for tcp://localhost:3051/ has resumed
dbug: Jasper.Transports[200]
      Successfully sent 1 messages to tcp://localhost:3051/
dbug: Jasper.Transports[202]
      Received 1 message(s) from tcp://localhost:3051/
dbug: Jasper.Messages[101]
      Received PongMessage#0168be78-e1b9-4bc8-bc58-98cdccf9824b at tcp://localhost:3050/ from tcp://localhost:3051/
dbug: Jasper.Messages[102]
      Started processing PongMessage#0168be78-e1b9-4bc8-bc58-98cdccf9824b
Got a pong back with name: Message19
dbug: Jasper.Messages[103]
      Finished processing PongMessage#0168be78-e1b9-4bc8-bc58-98cdccf9824b
info: Jasper.Messages[104]
      Successfully processed message PongMessage#0168be78-e1b9-4bc8-bc58-98cdccf9824b from tcp://localhost:3051/
dbug: Jasper.Messages[100]
      Sent PingMessage#0168be78-e4da-4c36-95e3-aa9d15ee0ff2 to tcp://localhost:3051/
info: Jasper.Transports[204]
      Sending agent for tcp://localhost:3051/ has resumed
dbug: Jasper.Transports[200]
      Successfully sent 1 messages to tcp://localhost:3051/
dbug: Jasper.Transports[202]
      Received 1 message(s) from tcp://localhost:3051/
dbug: Jasper.Messages[101]
      Received PongMessage#0168be78-e5a7-4e03-9894-4c3f61ed7413 at tcp://localhost:3050/ from tcp://localhost:3051/
dbug: Jasper.Messages[102]
      Started processing PongMessage#0168be78-e5a7-4e03-9894-4c3f61ed7413
Got a pong back with name: Message20
dbug: Jasper.Messages[103]
      Finished processing PongMessage#0168be78-e5a7-4e03-9894-4c3f61ed7413
info: Jasper.Messages[104]
      Successfully processed message PongMessage#0168be78-e5a7-4e03-9894-4c3f61ed7413 from tcp://localhost:3051/
dbug: Jasper.Messages[100]
      Sent PingMessage#0168be78-e8c5-4130-a919-81bfe235becb to tcp://localhost:3051/
info: Jasper.Transports[204]
      Sending agent for tcp://localhost:3051/ has resumed
dbug: Jasper.Transports[200]
      Successfully sent 1 messages to tcp://localhost:3051/
dbug: Jasper.Transports[202]
      Received 1 message(s) from tcp://localhost:3051/
dbug: Jasper.Messages[101]
      Received PongMessage#0168be78-e991-41b9-b1f0-16ee2fa75377 at tcp://localhost:3050/ from tcp://localhost:3051/
dbug: Jasper.Messages[102]
      Started processing PongMessage#0168be78-e991-41b9-b1f0-16ee2fa75377
Got a pong back with name: Message21

```

Alright, you've got a fully functional system of two services who constantly chat with each other. For more information about the topics we covered in this tutorial, see the documentation for <[linkto:documentation/messaging]>