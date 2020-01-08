<!--title:Using Jasper as a Lightweight Service Bus-->

Using its own built in <[linkto:transports/tcp]>, Jasper can be used as a very lightweight service bus to send messages asynchronously between Jasper applications. In this tutorial, we're going to create two Jasper services that exchange messages with each either. All of the [code for the sample Ping Pong application is in GitHub](https://github.com/JasperFx/JasperSamples/tree/master/PingPong).


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

<[sample:PingAndPongMessages]>

## In the Ponger Project

We need to make a couple changes to *Ponger*. First, open the generated `JasperConfig` class
and add the line of code shown below to tell *Ponger* to listen for messages on port 3051:

<[sample:PongerJasperConfig]>

Next, we need to handle the `PingMessage` message, and send a `PongMessage` back to the original sender:

<[sample:PingHandler]>


## In the Pinger Project

In *Pinger*, we need a handler for the `PongMessage` messages coming back, so add this class:


<[sample:PongHandler]>

We also need something that just runs in the background and sends `PingMessage` messages out. For that, we'll use
an implementation of ASP.Net Core's `IHostedService`:

<[sample:PingerService]>

Lastly in *Pinger*, open the `JasperConfig` class and add a single line of code shown below that directs Jasper to 
listen for messages using its <[linkto:transports/tcp]> at port 3050. We'll also make
a static publishing rule to publish all messages to the *Ponger* service. Lastly, we need to register the `PingSender`
with the system so that it runs continuously when the application is started.

All that is shown below:

<[sample:PingerJasperConfig]>



## Running Pinger and Ponger

Now you'll want a pair of command windows open, one to the root directory of `Pinger` and one to `Ponger`.

Since it's a little bit cleaner, start up *Ponger* first with a simple call to `dotnet run` from the *Ponger* directory and you should see output like this:

```
No Jasper extensions are detected
Searching 'Jasper, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null' for commands
Hosting environment: Production
Content root path: /JasperSamples/PingPong/Ponger
Press CTRL + C to quit
```

Then start *Pinger* with the same `dotnet run` command from the root of the *Pinger* project and well, you'll get a boatload of logging like this:

```
No Jasper extensions are detected
Searching 'Jasper, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null' for commands
Got pong #1
Got pong #2
Got pong #3
Got pong #4
Got pong #5
Got pong #6

```

Alright, you've got a fully functional system of two services who constantly chat with each other. For more information about the topics we covered in this tutorial, see the documentation for <[linkto:messaging]>